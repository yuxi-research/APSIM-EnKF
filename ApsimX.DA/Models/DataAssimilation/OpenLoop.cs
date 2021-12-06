using System;
using Models.Core;
using Models.DataAssimilation.DataType;
using System.Linq;

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

    public class OpenLoop : Model, IDataAssimilation
    {
        #region ******* Links. *******

        [Link]
        Control Control = null;

        [Link]
        StateVariables StateVariables = null;

        [Link(IsOptional = true)]
        Clock Clock = null;

        [Link(IsOptional = true)]
        Observations Observations = null;

        #endregion

        #region ******* Public field. *******

        /// <summary>True if data assmilation is activated.</summary>
        public bool DAActivated { get { return true; } }

        #endregion

        #region ******* Private field. *******

        private Matrix OL;
        private Matrix Prior;
        private Matrix PriorMean;
        private Matrix Posterior;
        private Matrix PosteriorMean { get; set; }
        private Function Fun = new Function();

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
            if (Control.DAOption == "OpenLoop" || Control.DAOption == "Openloop")
            {
                StatesOfTheDay States = StateVariables.GetPrior();
                StatesOfTheDay Output = StateVariables.GetOutputPrior();

                DoOpenLoop(States, Output);

                StateVariables.ReturnPosterior(States);
                StateVariables.ReturnOutputPosterior(Output);

            }
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public void DoOpenLoop(StatesOfTheDay States, StatesOfTheDay Output)
        {
            OL = States.ListToVec(States.PriorOL);
            Prior = States.ListToMat(States.Prior);
            Posterior = new Matrix(Prior.Row, Prior.Col);

            if (Control.AddModelError)
                Add_ModelError();
            else
                Non_ModelError();

            States.PosteriorOL = States.VecToList(OL);
            States.Posterior = States.MatToList(Posterior);

            PriorMean = new Matrix(Prior.Row, 1);
            PosteriorMean = new Matrix(Prior.Row, 1);
            Calc_Mean();

            States.PriorMean = States.VecToList(PriorMean);
            States.PosteriorMean = States.VecToList(PosteriorMean);

            // Return observations to SQLite.
            if (Observations != null)
            {
                Matrix Obs = Observations.GetObs(Clock.ID);
                Matrix Obs_SQLite = new Matrix(StateVariables.Count, 1);
                Matrix Obs_SQLiteExtra = new Matrix(StateVariables.OutputExtra.Count(), 1);

                Obs_SQLite.Set(-99);
                Obs_SQLiteExtra.Set(-99);

                for (int i = 0; i < Observations.ObsNumber; i++)
                {
                    if (Observations.ObsIndexInStates[i] >= 0)
                        Obs_SQLite.Arr[Observations.ObsIndexInStates[i], 0] = Obs.Arr[i, 0];
                }

                // Return observations to OutputOftheDay.
                for (int i = 0; i < StateVariables.OutputExtra.Count(); i++)
                {
                    if (Observations.OutputIndexInObs[i] >= 0)
                    {
                        Obs_SQLiteExtra.Arr[i, 0] = Obs.Arr[Observations.OutputIndexInObs[i], 0];
                    }
                }
                States.Obs = States.VecToList(Obs_SQLite);
                Output.Obs = Output.VecToList(Obs_SQLiteExtra);
            }
        }

        /// <summary>
        /// Add model error.
        /// </summary>
        public void Add_ModelError()
        {
            for (int i = 0; i < Prior.Row; i++)
            {
                if (StateVariables.ModelErrorOption[i] == 0)
                {
                    for (int j = 0; j < Prior.Col; j++)
                    {
                        Posterior.Arr[i, j] = Prior.Arr[i, j];
                    }
                }
                else
                {
                    double OLRand = Fun.NormalRand();

                    double[] EnsembleRand = new double[Prior.Col];
                    for (int j = 0; j < Prior.Col; j++)
                    {
                        EnsembleRand[j] = Fun.NormalRand() + OLRand;
                    }

                    if (StateVariables.ModelErrorOption[i] == 1)    // Addtive
                    {
                        if (OL.Arr[i, 0] > 0)
                        {
                            OL.Arr[i, 0] = OL.Arr[i, 0] + StateVariables.ModelError[i] * OLRand;
                            OL.Arr[i, 0] = Math.Max(OL.Arr[i, 0], 0);
                        }

                        for (int j = 0; j < Prior.Col; j++)
                        {
                            if (Prior.Arr[i, j] > 0)
                            {
                                Posterior.Arr[i, j] = Prior.Arr[i, j] + StateVariables.ModelError[i] * EnsembleRand[j];
                                Posterior.Arr[i, j] = Math.Max(Posterior.Arr[i, j], 0);
                            }
                            else
                            {
                                Posterior.Arr[i, j] = Prior.Arr[i, j];
                            }
                        }
                    }
                    else if (StateVariables.ModelErrorOption[i] == 2)   //Multiplicative
                    {
                        if (OL.Arr[i, 0] > 0)
                        {
                            OL.Arr[i, 0] = OL.Arr[i, 0] + OL.Arr[i, 0] * StateVariables.ModelError[i] * OLRand;
                            OL.Arr[i, 0] = Math.Max(OL.Arr[i, 0], 0);
                        }

                        for (int j = 0; j < Prior.Col; j++)
                        {
                            if (Prior.Arr[i, j] > 0)
                            {
                                Posterior.Arr[i, j] = Prior.Arr[i, j] + Prior.Arr[i, j] * StateVariables.ModelError[i] * EnsembleRand[j];
                                Posterior.Arr[i, j] = Math.Max(Posterior.Arr[i, j], 0);
                            }
                            else
                            {
                                Posterior.Arr[i, j] = Prior.Arr[i, j];
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Wrong values assigned to ModelErrorOption! Please check input file.");
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

        /// <summary>
        /// Calculate Prior and Posterior Mean.
        /// </summary>
        public void Calc_Mean()
        {
            for (int i = 0; i < Prior.Row; i++)
            {
                double priorMean = 0;
                double posteriorMean = 0;

                for (int j = 0; j < Prior.Col; j++)
                {
                    priorMean += Prior.Arr[i, j];
                    posteriorMean += Posterior.Arr[i, j];

                }
                PriorMean.Arr[i, 0] = priorMean / Prior.Col;
                PosteriorMean.Arr[i, 0] = posteriorMean / Prior.Col;
            }
        }

        #endregion
    }
}
