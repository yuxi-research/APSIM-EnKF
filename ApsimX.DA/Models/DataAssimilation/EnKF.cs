using System;
using Models.Core;
using Models.DataAssimilation.DataType;
using System.Collections.Generic;
using Models.PMF.OldPlant;

namespace Models.DataAssimilation
{
    /// <summary>
    /// A test model for open-loop run.
    /// This class get PriorStates and calculate PosteriorStates, linking to StateTable and StatesOfTheDay.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(IDataAssimilation))]
    [ValidParent(ParentType = typeof(Control))]

    public class EnKF : Model, IDataAssimilation
    {
        #region ******* Links. *******
        [Link]
        Clock Clock = null;

        [Link]
        Control Control = null;

        [Link]
        StateVariables StateVariables = null;

        [Link]
        Observations Observations = null;

        #endregion

        #region ******* Public field. *******

        /// <summary>True if data assmilation is activated.</summary>
        public bool DAActivated { get { return true; } }

        #endregion

        #region ******* Private field. *******

        private int ObsNumber_Red;
        private Matrix OL;
        private Matrix Prior;
        private Matrix PriorMean;
        private Matrix Posterior;
        private Matrix Obs;
        private List<int> Map;
        private Matrix H;
        private Matrix H_Red;
        private Matrix D;
        private Matrix R;
        private Matrix R_Red;
        private Matrix P;
        private Matrix K;
        private Matrix y;
        private Matrix y_Red;
        //private Matrix hx;
        private Matrix hx_Red;
        private Matrix v;
        private Matrix v_Red;
        private Matrix PosteriorMean;
        private Function Fun = new Function();

        //Defind SQLite for output.

        #endregion

        #region ******* EventHandlers. *******

        /// <summary> 
        /// Write prior results.
        /// Called only once a day.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("DoDataAssimilation")]
        private void OnDoDataAssimilation(object sender, EventArgs e)
        {
            if (Control.DAOption == "EnKF")
            {
                StatesOfTheDay States = StateVariables.GetPrior();
                States = DoEnKF(States);
                StateVariables.ReturnPosterior(States);
            }
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public StatesOfTheDay DoEnKF(StatesOfTheDay States)
        {
            //Get Obs of today.
            Obs = Observations.GetObs(Clock.ID);

            //Add model errors.
            OL = States.ListToVec(States.PriorOL);
            Prior = States.ListToMat(States.Prior);
            Posterior = new Matrix(Prior.Row, Prior.Col);
            //Prior.Display();
            ////Check NaN. Yuxi: No need for NaN check.
            //Matrix MapNaN = Prior.CheckNaN();

            if (Control.AddModelError)
                Add_ModelError();
            else
                Non_ModelError();

            //Calculate PriorMean.
            PriorMean = new Matrix(Prior.Row, 1);
            Calc_PriorMean();

            //Check the number of available observation.
            CheckObs();

            if (ObsNumber_Red > 0)
            {
                Console.Write("Available observation number = {0}, observations are\t", ObsNumber_Red);
                foreach (int index in Map)
                {
                    Console.Write(Observations.StateNamesObs[index] + "\t");
                }
                Console.WriteLine();

                //Calculate D and P.
                D = new Matrix(Prior.Row, Prior.Row);
                Calc_D();
                Calc_P();

                //Calculate H.
                H = Observations.H0;
                H_Red = new Matrix(ObsNumber_Red, Prior.Row);
                Reduce_H();

                //Calculate R, K.
                R = new Matrix(Observations.ObsNumber, Observations.ObsNumber);
                R_Red = new Matrix(ObsNumber_Red, ObsNumber_Red);
                Calc_R();
                Reduce_R();
                Calc_K();

                //Calculate hx_Red.
                Calc_hx_Red();

                //Generate observational errors.
                v = new Matrix(Observations.ObsNumber, Prior.Col);
                v_Red = new Matrix(ObsNumber_Red, Prior.Col);
                Calc_v();
                Reduce_v();

                //Perturb observations.
                y = new Matrix(Observations.ObsNumber, Prior.Col);
                y_Red = new Matrix(ObsNumber_Red, Prior.Col);
                PerturbObs();   //Calculate y.
                Reduce_y();
            }
            else
            { }

            // Calculate posterior.
            Calc_PosteriorStates();
            Posterior.CheckNegative();
            PosteriorMean = new Matrix(Posterior.Row, 1);
            Calc_PosteriorMean();
            //Posterior.Display();

            //Map observations to SQLite.
            Matrix Obs_Map = new Matrix(Observations.ObsNumber, 1);
            Matrix PerturbedObs_Map = new Matrix(Observations.ObsNumber, Control.EnsembleSize);
            Matrix Obs_SQLite = new Matrix(StateVariables.Count, 1);
            Matrix PerturbedObs_SQLite = new Matrix(StateVariables.Count, Control.EnsembleSize);

            Obs_Map.Set(-99);
            PerturbedObs_Map.Set(-99);
            Obs_SQLite.Set(-99);
            PerturbedObs_SQLite.Set(-99);

            for (int i = 0; i < Map.Count; i++)
            {
                Obs_Map.Arr[Map[i], 0] = Obs.Arr[Map[i], 0];
                for (int j = 0; j < PerturbedObs_Map.Col; j++)
                {
                    PerturbedObs_Map.Arr[Map[i], j] = y.Arr[Map[i], j];
                }
            }

            //Return to StatesOfTheDay.
            States.PriorMean = States.VecToList(PriorMean);
            States.PosteriorOL = States.VecToList(OL);
            States.Posterior = States.MatToList(Posterior);
            States.PosteriorMean = States.VecToList(PosteriorMean);

            if (ObsNumber_Red > 0)
            {
                for (int i = 0; i < Observations.ObsNumber; i++)
                {
                    Obs_SQLite.Arr[Observations.ObsIndex[i], 0] = Obs_Map.Arr[i, 0];
                    for (int j = 0; j < PerturbedObs_Map.Col; j++)
                    {
                        PerturbedObs_SQLite.Arr[Observations.ObsIndex[i], j] = PerturbedObs_Map.Arr[i, j];
                    }
                }
            }
            States.Obs = States.VecToList(Obs_SQLite);
            States.ObsPerturb = States.MatToList(PerturbedObs_SQLite);

            return States;
        }

        #region Reduce H, R, y, v.
        /// <summary>
        /// Check the number of valid values in Obs matrix. Creat a Map according to which the matrix size of [H, R, y, v] can be reduced.
        /// </summary>
        public void CheckObs()
        {
            ObsNumber_Red = 0;
            Map = new List<int>();
            for (int i = 0; i < Obs.Row; i++)
            {
                if (Obs.Arr[i, 0] > 0)     //Assume negative values of observation are invalid. Invalid values are set to -99 in observation files.
                {
                    Map.Add(i);
                    ObsNumber_Red++;
                }
            }
        }

        /// <summary>
        /// Reduce H through Map.
        /// </summary>
        public void Reduce_H()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                for (int j = 0; j < H_Red.Col; j++)
                {
                    H_Red.Arr[i, j] = H.Arr[Map[i], j];
                }
            }
        }

        /// <summary>
        /// Reduce R through Map.
        /// </summary>
        public void Reduce_R()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                for (int j = 0; j < Map.Count; j++)
                {
                    R_Red.Arr[i, j] = R.Arr[Map[i], Map[j]];
                }
            }
        }

        /// <summary>
        /// Reduce y (perturbed observation) through Map.
        /// </summary>
        public void Reduce_y()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                for (int j = 0; j < y.Col; j++)
                {
                    y_Red.Arr[i, j] = y.Arr[Map[i], j];
                }
            }
        }

        /// <summary>
        /// Reduce v (observational errors) through Map.
        /// </summary>
        public void Reduce_v()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                for (int j = 0; j < v.Col; j++)
                {
                    v_Red.Arr[i, j] = v.Arr[Map[i], j];
                }
            }
            //Console.WriteLine("v_Red = ");
            //v_Red.Display();
        }

        #endregion

        #region Model errors.
        /// <summary>
        /// Add model error.
        /// </summary>
        public void Add_ModelError()
        {
            for (int i = 0; i < Prior.Row; i++)
            {
                double OLRand = Fun.NormalRand();

                double[] EnsembleRand = new double[Prior.Col];
                for (int j = 0; j < Prior.Col; j++)
                {
                    EnsembleRand[j] = Fun.NormalRand() + OLRand;
                }

                if (StateVariables.ModelErrorOption[i] == 1)
                {
                    OL.Arr[i, 0] = OL.Arr[i, 0] + OL.Arr[i, 0] * StateVariables.ModelError[i] * OLRand;
                    OL.Arr[i, 0] = Math.Max(OL.Arr[i, 0], 0);

                    for (int j = 0; j < Prior.Col; j++)
                    {
                        Posterior.Arr[i, j] = Prior.Arr[i, j] + Prior.Arr[i, j] * StateVariables.ModelError[i] * EnsembleRand[j];
                        Posterior.Arr[i, j] = Math.Max(Posterior.Arr[i, j], 0);
                    }
                }
                else
                {
                    OL.Arr[i, 0] = OL.Arr[i, 0] + OL.Arr[i, 0] * StateVariables.ModelError[i] * OLRand;
                    for (int j = 0; j < Prior.Col; j++)
                    {
                        Posterior.Arr[i, j] = Prior.Arr[i, j] + StateVariables.ModelError[i] * EnsembleRand[j];
                    }
                }
            }
        }

        /// <summary>
        /// Not to add model error.
        /// </summary>
        public void Non_ModelError()
        {
            for (int i = 0; i < Prior.Row; i++)
            {
                for (int j = 0; j < Prior.Col; j++)
                {
                    Posterior.Arr[i, j] = Prior.Arr[i, j];
                }
            }
        }

        #endregion

        #region EnKF calculation
        /// <summary>
        /// Calculate Prior and Posterior Mean.
        /// </summary>
        public void Calc_PriorMean()
        {
            for (int i = 0; i < Prior.Row; i++)
            {
                double priorMean = 0;
                for (int j = 0; j < Prior.Col; j++)
                    priorMean += Prior.Arr[i, j];

                PriorMean.Arr[i, 0] = priorMean / Prior.Col;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Calc_D()
        {
            D = Prior - PriorMean;
            //Console.WriteLine("d = ");
            //D.Display();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Calc_P()
        {
            P = (1.0 / (Prior.Col - 1)) * (D * D.Transpose());

            ////Assume all states are unrelated.    //No need for this.
            //for (int i = 0; i < P.Row; i++)
            //{
            //    if (i != 0)
            //    {
            //        P.Arr[i, 0] = 0;
            //    }
            //}
            //for (int i = 0; i < P.Col; i++)
            //{
            //    if (i != 0)
            //    {
            //        P.Arr[0, i] = 0;
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        public void Calc_R()
        {
            for (int i = 0; i < R.Col; i++)
            {
                for (int j = 0; j < R.Col; j++)
                {
                    if (i == j)
                        if (Observations.ObsErrorOption[i] == 1 && Obs.Arr[i, 0] != 0)
                        {
                            R.Arr[i, j] = Observations.ObsError[i] * Observations.ObsError[i] * Obs.Arr[i, 0] * Obs.Arr[i, 0];        //Default. Read R matrix directly will be better.
                        }
                        else if (Observations.ObsErrorOption[i] == 1 && Obs.Arr[i, 0] == 0)
                        {
                            R.Arr[i, j] = Observations.ObsError[i] * Observations.ObsError[i] * PriorMean.Arr[i, 0] * PriorMean.Arr[i, 0];
                        }
                        else
                        {
                            R.Arr[i, j] = Observations.ObsError[i] * Observations.ObsError[i];        //Default. Read R matrix directly will be better.
                        }
                    else
                        R.Arr[i, j] = 0;    //Assume independent.
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Calc_K()
        {
            //Console.WriteLine("H = ");
            //H.Display();
            //Console.WriteLine("K = ");
            //K = H * P * H.Transpose() + R;
            //K.Display();
            //K = K.Inverse();
            //K.Display();
            //K = P* H * K;
            Matrix temp = H_Red * P * H_Red.Transpose() + R_Red;
            //Check if temp or H can be reversed.   Not a full check.
            if (temp.EqualTo(0) || H_Red.EqualTo(0))
            {
                K = new Matrix(Prior.Row, Observations.ObsNumber);
                K.Assign(0);
            }
            else
            {
                K = P * H_Red.Transpose() * temp.Inverse();
            }
            //Console.WriteLine("K = ");
            //K.Display();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Calc_hx_Red()
        {
            hx_Red = H_Red * Prior;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Calc_v()
        {
            //Observational errors.
            for (int i = 0; i < v.Row; i++)
            {
                if (Observations.ObsErrorOption[i] == 1)
                {
                    //Proportional
                    if (Obs.Arr[i, 0] != 0)
                    {
                        for (int j = 0; j < v.Col; j++)
                            v.Arr[i, j] = Observations.ObsError[i] * Obs.Arr[i, 0] * Fun.NormalRand();
                    }
                    else
                    {
                        for (int j = 0; j < v.Col; j++)
                            v.Arr[i, j] = 0;
                    }
                }
                else
                {
                    //Additional
                    if (Obs.Arr[i, 0] != 0)
                    {
                        for (int j = 0; j < v.Col; j++)
                            v.Arr[i, j] = Observations.ObsError[i] * Fun.NormalRand();
                    }
                    else
                    {
                        for (int j = 0; j < v.Col; j++)
                            v.Arr[i, j] = 0;
                    }
                }
            }
            //Console.WriteLine("v = ");
            //v.Display();
        }

        /// <summary>
        /// 
        /// </summary>
        public void PerturbObs()
        {
            for (int i = 0; i < y.Row; i++)
            {
                if (Observations.ObsErrorOption[i] == 1)
                {
                    for (int j = 0; j < y.Col; j++)
                    {
                        if (Obs.Arr[i, 0] != 0)
                        {
                            y.Arr[i, j] = Math.Max(0, Obs.Arr[i, 0] + Observations.ObsError[i] * Obs.Arr[i, 0] * Fun.NormalRand());
                        }
                        else
                        {
                            y.Arr[i, j] = Obs.Arr[i, 0];
                        }
                    }
                }
                else if (Observations.ObsErrorOption[i] == 0)
                {
                    for (int j = 0; j < y.Col; j++)
                    {
                        if (Obs.Arr[i, 0] != 0)
                        {
                            y.Arr[i, j] = Math.Max(0, Obs.Arr[i, 0] + Observations.ObsError[i] * Fun.NormalRand());
                        }
                        else
                        {
                            y.Arr[i, j] = Obs.Arr[i, 0];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Calc_PosteriorStates()
        {
            if (ObsNumber_Red > 0)
            {
                // If a prior state is zero, do not update its value.
                Matrix delta = K * (y_Red - hx_Red + v_Red);
                Posterior = Prior.NonZeroAdd(delta);
            }
            else
            {
                Posterior = Prior * 1;
            }
            //Console.WriteLine("Posterior = ");
            //Posterior.Display();
        }

        /// <summary>
        /// Calculate Prior and Posterior Mean.
        /// </summary>
        public void Calc_PosteriorMean()
        {
            for (int i = 0; i < Prior.Row; i++)
            {
                double posteriorMean = 0;
                for (int j = 0; j < Prior.Col; j++)
                    posteriorMean += Posterior.Arr[i, j];

                PosteriorMean.Arr[i, 0] = posteriorMean / Prior.Col;
            }
        }
        #endregion

        #endregion
    }
}
