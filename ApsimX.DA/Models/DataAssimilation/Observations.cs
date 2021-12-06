using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using Models.DataAssimilation.DataType;
using System.IO;

namespace Models.DataAssimilation
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.ProfileView")]
    [PresenterName("UserInterface.Presenters.ProfilePresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(IDataAssimilation))]
    [ValidParent(ParentType = typeof(Control))]

    public class Observations : Model
    {

        #region ******* Links. *******

        [Link]
        StateVariables States = null;

        #endregion

        #region ******* Public field. *******
        /// <summary> Obs Number. </summary>
        public int ObsNumber { get { return ObsNames.Count(); } }
        /// <summary> Observation index in State vector (-1 if obs is not in state vector). </summary>
        public static int[] ObsIndexInStates { get; set; }
        /// <summary> Observation index in OutputExtra. </summary>
        public static int[] OutputIndexInObs { get; set; }
        /// <summary> State observations. </summary>
        [Description("State observations")]
        public string[] ObsNames { get; set; }     //Only need one of ObsIndex and StateNames Obs.
        /// <summary> Obs Error. </summary>
        [Description("Observational errors")]
        public double[] ObsError { get; set; }
        /// <summary> Obs Error Options. </summary>
        [Description("Obs error options (0=additive, 1=multiplicative)")]
        public int[] ObsErrorOption { get; set; }

        /// <summary> The full H matrix. </summary>
        public static Matrix H0 { get; set; }
        /// <summary>Observation data path. </summary>
        public string[] ObsFiles { get; set; }

        #endregion

        #region ******* Private field. *******

        private FolderInfo Info = new FolderInfo();
        private static Matrix AllObs;
        private static List<List<double>> AllObsList = new List<List<double>>();

        #endregion

        #region ******* EventHandlers. *******

        /// <summary> Read external observations. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("PrepareAssimilation")]
        private void OnPrepareAssimilation(object sender, EventArgs e)
        {
            SetObsIndexInStates();
            SetOutputIndexInObs();
            ReadAllObs();
            Initialize_H();
        }

        #endregion

        #region Methods.
        /// <summary>
        /// 
        /// </summary>
        public void SetObsIndexInStates()
        {
            ObsIndexInStates = new int[ObsNames.Count()];
            // Loop obsnames.
            for (int i = 0; i < ObsIndexInStates.Count(); i++)
            {
                ObsIndexInStates[i] = -1;

                //find the name of state in obs, return index.
                for (int j = 0; j < States.StateNames.Count(); j++)
                {
                    if (ObsNames[i] == States.StateNames[j])
                    {
                        ObsIndexInStates[i] = j;
                    }
                }
            }
        }

        /// <summary>
        /// For extra output.
        /// </summary>
        public void SetOutputIndexInObs()
        {
            OutputIndexInObs = new int[States.OutputExtra.Count()];
            // Loop obsnames.
            for (int i = 0; i < States.OutputExtra.Count(); i++)
            {
                OutputIndexInObs[i] = -1;
                //find the name of output in obs, return index.
                for (int j = 0; j < ObsNames.Count(); j++)
                {
                    if (States.OutputExtra[i] == ObsNames[j])
                    {
                        OutputIndexInObs[i] = j;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the full H matrix.
        /// </summary>
        /// <returns></returns>
        public void Initialize_H()
        {
            H0 = new Matrix(ObsNames.Count(), States.StateNames.Count());
            H0.Set(0);

            List<ObsOperator> H0Collect = new List<ObsOperator>();
            List<string> ObsCollect = new List<string>();

            H0Collect.Add(new ObsOperator("LAI", new string[] { "LAI" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SW1", new string[] { "SW1" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SW2", new string[] { "SW2" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SW3", new string[] { "SW3" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SW4", new string[] { "SW4" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SW5", new string[] { "SW5" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SW6", new string[] { "SW6" }, new double[] { 1 }));

            H0Collect.Add(new ObsOperator("LeafWt", new string[] { "LeafWt" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("StemWt", new string[] { "StemWt" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("GrainWt", new string[] { "GrainWt" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("PodWt", new string[] { "PodWt" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("RootWt", new string[] { "RootWt" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("LeafN", new string[] { "LeafN" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("StemN", new string[] { "StemN" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("GrainN", new string[] { "GrainN" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("PodN", new string[] { "PodN" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("RootN", new string[] { "RootN" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("Height", new string[] { "Height" }, new double[] { 1 }));

            H0Collect.Add(new ObsOperator("SoilNO3_1", new string[] { "SoilNO3_1" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNO3_2", new string[] { "SoilNO3_2" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNO3_3", new string[] { "SoilNO3_3" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNO3_4", new string[] { "SoilNO3_4" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNO3_5", new string[] { "SoilNO3_5" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNO3_6", new string[] { "SoilNO3_6" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNH4_1", new string[] { "SoilNH4_1" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNH4_2", new string[] { "SoilNH4_2" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNH4_3", new string[] { "SoilNH4_3" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNH4_4", new string[] { "SoilNH4_4" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNH4_5", new string[] { "SoilNH4_5" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("SoilNH4_6", new string[] { "SoilNH4_6" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("RootDepth", new string[] { "RootDepth" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("RootDepth", new string[] { "RootDepth" }, new double[] { 1 }));
            H0Collect.Add(new ObsOperator("RootDepth", new string[] { "RootDepth" }, new double[] { 1 }));

            H0Collect.Add(new ObsOperator("Biomass", new string[] { "LeafWt", "StemWt", "PodWt", "GrainWt" }, new double[] { 1, 1, 1, 1 }));
            H0Collect.Add(new ObsOperator("HeadWt", new string[] { "PodWt", "GrainWt" }, new double[] { 1, 1 }));
            H0Collect.Add(new ObsOperator("HeadN", new string[] { "PodN", "GrainN" }, new double[] { 1, 1 }));

            H0Collect.Add(new ObsOperator("SoilNTotal_1", new string[] { "SoilNH4_1", "SoilNO3_1" }, new double[] { 1, 1 }));
            H0Collect.Add(new ObsOperator("SoilNTotal_2", new string[] { "SoilNH4_2", "SoilNO3_2" }, new double[] { 1, 1 }));
            H0Collect.Add(new ObsOperator("SoilNTotal_3", new string[] { "SoilNH4_3", "SoilNO3_3" }, new double[] { 1, 1 }));
            H0Collect.Add(new ObsOperator("SoilNTotal_4", new string[] { "SoilNH4_4", "SoilNO3_4" }, new double[] { 1, 1 }));
            H0Collect.Add(new ObsOperator("SoilNTotal_5", new string[] { "SoilNH4_5", "SoilNO3_5" }, new double[] { 1, 1 }));
            H0Collect.Add(new ObsOperator("SoilNTotal_6", new string[] { "SoilNH4_6", "SoilNO3_6" }, new double[] { 1, 1 }));

            // A collection of ObsNames from H0Collect.
            foreach (ObsOperator collect in H0Collect)
            {
                ObsCollect.Add(collect.ObsName);
                Console.WriteLine("H0 is");
                collect.Display();
            }

            int obsIndex;

            // Consctruct H0 matrix.
            for (int i = 0; i < ObsNames.Count(); i++)
            {
                // Get obsIndex from H0Collection.
                obsIndex = ObsCollect.IndexOf(ObsNames[i]);
                if (obsIndex < 0)
                    throw new Exception("Obs type [" + ObsNames[i] + "] is not defined in ObsCollection.");

                // Get related state names and H values from H0Collect using obsIndex.
                string[] stateList = H0Collect[obsIndex].StateList;
                double[] hList = H0Collect[obsIndex].HList;

                for (int k = 0; k < stateList.Count(); k++)
                {
                    bool found = false;
                    // Recursively find the related state names from the EnKF state vector.
                    for (int j = 0; j < States.StateNames.Count(); j++)
                    {
                        if (stateList[k] == States.StateNames[j])
                        {
                            H0.Arr[i, j] = hList[k];
                            found = true;
                            break;
                        }
                    }
                    // If state is not found, check the s
                    if (!found)
                        throw new Exception("States is not found in the observation operator. Check the state vector.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Matrix GetObs(int ID)
        {
            Matrix Obs = new Matrix(ObsNumber, 1);
            for (int i = 0; i < Obs.Row; i++)
                Obs.Arr[i, 0] = AllObsList[i][ID];
            return Obs;
        }

        #region Old read observations

        /// <summary>
        /// 
        /// </summary>
        public void ReadAllObs()
        {
            for (int i = 0; i < ObsNames.Count(); i++)
            {
                ReadSingleObs(ObsNames[i]);
            }
            AllObs = new Matrix(AllObsList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obsName"></param>
        public void ReadSingleObs(string obsName)
        {
            List<double> obs = new List<double>();
            string row;
            StreamReader sr;
            if (File.Exists(Info.Obs + "/" + obsName + "_Obs.csv"))
            {
                sr = new StreamReader(Info.Obs + "/" + obsName + "_Obs.csv");
            }
            else
            {
                sr = new StreamReader(Info.Obs + "/Default.csv");
            }
            while ((row = sr.ReadLine()) != null)
            {
                obs.Add(Convert.ToDouble(row));
            }
            AllObsList.Add(obs);
            sr.Close();

        }
    }

    #endregion

    #endregion
}
