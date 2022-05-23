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
                States = DoOpenLoop(States);
                StateVariables.ReturnPosterior(States);
            }
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public StatesOfTheDay DoOpenLoop(StatesOfTheDay States)
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

            if (Observations != null)
            {
                Matrix Obs = Observations.GetObs(Clock.ID);
                Matrix Obs_SQLite = new Matrix(StateVariables.Count, 1);
                Obs_SQLite.Set(-99);

                for (int i = 0; i < Observations.ObsNumber; i++)
                {
                    Obs_SQLite.Arr[Observations.ObsIndex[i], 0] = Obs.Arr[i, 0];  
                }
                States.Obs = States.VecToList(Obs_SQLite);
            }

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
