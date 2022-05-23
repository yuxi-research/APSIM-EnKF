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
        Control Control = null;

        [Link]
        StateVariables States = null;

        #endregion

        #region ******* Public field. *******
        /// <summary> Obs Number. </summary>
        public int ObsNumber { get { return StateNamesObs.Count(); } }
        /// <summary> Observation index from State vector. </summary>
        [Description("Observation index from State vector")]
        public int[] ObsIndex { get; set; }
        /// <summary> State observations. </summary>
        [Description("State observations")]
        public string[] StateNamesObs { get; set; }     //Only need one of ObsIndex and StateNames Obs.
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
            if (Control.DAOption != null)
            {
                ReadAllObs();
                Initialize_H();
            }
        }

        #endregion

        #region Methods.

        /// <summary>
        /// Initialize the full H matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix Initialize_H()
        {
            H0 = new Matrix(ObsNumber, States.Count);
            if (Control.HOption == 0)   // 0 = Linear;
            {
                for (int i = 0; i < H0.Row; i++)
                {
                    for (int j = 0; j < H0.Col; j++)
                    {
                        if (ObsIndex[i] == j)
                        {
                            H0.Arr[i, j] = 1;
                        }
                        else
                        {
                            H0.Arr[i, j] = 0;
                        }
                    }
                }
            }
            return H0;
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
            for (int i = 0; i < StateNamesObs.Count(); i++)
            {
                ReadSingleObs(StateNamesObs[i]);
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
