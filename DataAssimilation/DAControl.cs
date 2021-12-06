using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAssimilation
{
    [Serializable]
    public class DAControl
    {
        public bool OpenLoop { get; set; }

        public int EnsembleSize { get; set; }
        public string[] StateNames { get; set; }
        public string[] StateNamesObs { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double[] InitialSW { get; set; }
        public double[] InitialSW2 { get; set; }
        public double[] ObsError { get; set; }
        public int[] ObsErrorOption { get; set; }
        public double[] InitialSWError { get; set; }
        public double[] ModelError { get; set; }
        public int[] ModelErrorOption { get; set; }

        public DAControl(int option)


        {
            switch (option)
            {
                case 0:
                    {
                        EnsembleSize = 10;
                        OpenLoop = false;
                        if (OpenLoop)
                        {
                            EnsembleSize = EnsembleSize + 1;    //EnsembleSize +1 for open loop
                        }
                        StateNames = new string[] { "LAI", "SW1", "SW2", "SW3", "SW4", "SW5", "SW6", "SW7" };
                        int[] index = new int[] { 0, 1 };       //Define which observation is available.
                        StateNamesObs = new string[index.Count()];
                        for (int i = 0; i < StateNamesObs.Count(); i++)
                        {
                            StateNamesObs[i] = StateNames[index[i]];
                        }
                        StartIndex = 5;     //Maybe change this: Select column names that contain "Ensemble" and "Prior"
                        EndIndex = StartIndex + EnsembleSize;   //End before reaching this index.

                        InitialSW = new double[] { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };
                        InitialSW2 = new double[] { 0.46, 0.46, 0.46, 0.46, 0.46, 0.46, 0.46 };

                        //Standard deviation.
                        ObsError = new double[] { 0.1, 0, 04 };
                        ObsErrorOption = new int[] { 1, 0 };

                        InitialSWError = new double[] { 0.04, 0.04, 0.04, 0.04, 0.04, 0.04, 0.04 };

                        ModelError = new double[] { 0.01, 0.002, 0.002, 0.001, 0.001, 0.0005, 0.0005, 0.0005 };
                        ModelErrorOption = new int[] { 1, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }
    }
}
