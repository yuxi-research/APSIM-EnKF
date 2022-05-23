using System;
using Models.Core;
using Models.DataAssimilation.DataType;

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

    public class DirectInsertion : Model, IDataAssimilation
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

        private Matrix OL;
        private Matrix Prior;
        private Matrix PriorMean;
        private Matrix Posterior;
        private Matrix Obs;
        private Matrix PosteriorMean { get; set; }
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
            if (Control.DAOption == "DirectInsertion")
            {
                StatesOfTheDay States = StateVariables.GetPrior();
                States = DoDirectInsertion(States);
                StateVariables.ReturnPosterior(States);
            }
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public StatesOfTheDay DoDirectInsertion(StatesOfTheDay States)
        {
            //Add model errors.
            OL = States.ListToVec(States.PriorOL);
            Prior = States.ListToMat(States.Prior);
            Posterior = new Matrix(Prior.Row, Prior.Col);

            if (Control.AddModelError)
                Add_ModelError();
            else
                Non_ModelError();

            States.PosteriorOL = States.VecToList(OL);

            //Calculate posterior (Direct Insertion).
            //Need a better solution in the future.
            Obs = Observations.GetObs(Clock.ID);

            if (!Obs.EqualTo(0))
            {
                for (int i = 0; i < Posterior.Row; i++)
                {
                    if (Obs.Arr[i, 0] != 0)
                    {
                        for (int j = 0; j < Posterior.Col; j++)
                        {
                            Posterior.Arr[Observations.ObsIndex[i], j] = Obs.Arr[i, 0];
                        }
                    }
                }
            }

            States.Posterior = States.MatToList(Posterior);

            PriorMean = new Matrix(Prior.Row, 1);
            PosteriorMean = new Matrix(Prior.Row, 1);
            Calc_Mean();
            States.PriorMean = States.VecToList(PriorMean);
            States.PosteriorMean = States.VecToList(PosteriorMean);
            return States;
        }

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
