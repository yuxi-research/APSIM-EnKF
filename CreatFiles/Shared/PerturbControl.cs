using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data;
using System.Xml.Serialization;

namespace Shared
{
    [Serializable]
    public class PerturbControl
    {
        public int EnsembleSize { get; set; }
        public bool FixedOL { get; set; }
        public bool Fix_Pheno { get; set; }
        public bool Perturb_Weather { get; set; }
        public bool Perturb_Cultivar { get; set; }
        public bool Perturb_Soil { get; set; }

        public string[] weatherNames { get; set; }
        public double[] WeatherError { get; set; }
        public double[] Xi { get; set; }
        public double[] Chi { get; set; }
        public double[] WeatherLowerBound { get; set; }
        public double[] WeatherUpperBound { get; set; }
        public int[] WeatherPerturbOption { get; set; }
        public string[] ParaName { get; set; }
        public double[] ParaBaseline { get; set; }
        public double[] ParaOpenLoop { get; set; }
        public double[] ParaError { get; set; }
        public double[] InitSW { get; set; }
        public double[] InitSWError { get; set; }
        public double[] InitSWOpenLoop { get; set; }

        public double[] DUL { get; set; }
        public double[] DULError { get; set; }
        public double[] DULOpenLoop { get; set; }
        public double[] WheatLL { get; set; }
        public double[] WheatLLError { get; set; }
        public double[] WheatLLOpenLoop { get; set; }
        public double[] LL15 { get; set; }
        public double[] LL15Error { get; set; }
        public double[] AirDry { get; set; }
        public double[] AirDryError { get; set; }
        public double[] SAT { get; set; }
        public double[] SATError { get; set; }
        public double[] SW_Std { get; set; }
        public List<double[]> SW_Corr { get; set; }
        public DataType.Matrix MultiRand { get; set; }
        public double AirDry_LL { get; set; }
        public double SAT_DUL { get; set; }
        // Fix OL to a known value but different to Truth.

        public PerturbControl()
        {
            EnsembleSize = 50;
            FixedOL = true;
            Fix_Pheno = false;
            Perturb_Weather = true;
            Perturb_Cultivar = true;
            Perturb_Soil = true;    // (Assigned to "true" at serialization.)

            weatherNames = new string[6] { "radn", "maxt", "mint", "rain", "evap", "vp" };
            // Addtive or multiplicative method
            WeatherError = new double[6] { 0.3, 1, 1, 0.15, 0, 0 };     // Addtive or multiplicative method only

            //Weather with Turner's method.
            WeatherPerturbOption = new int[6] { 2, 1, 1, 2, 0, 0 };  // 0 = not perturbed. 1 = Unrestricted, 2 = Semi-restricted (lower-bounded);
            WeatherLowerBound = new double[6] { 0, -99, -99, 0, 0, 0 };
            WeatherUpperBound = new double[6] { 100, 100, 100, 100, 100, 100 };
            Xi = new double[] { 0.038, 1.4, 1.4, 0.25, 0.2, 2.04 };     //short-wave radiation, temp,temp, precipitation, evaporation, air pressure
            Chi = new double[] { 0.038, 0.6, 0.6, 0.25, 0.2, 2.04 };

            //Cultivar parameters.
            ParaName = new string[9] { "Para1", "Para2", "Para3", "Para4", "Para5", "Para6", "Para7", "Para8", "Para9" };
            ParaBaseline = new double[9] { 2, 3.5, 400.0, 580.0, 120.0, 590.0, 0.002, 0.001, 0.000055 };
            ParaOpenLoop = new double[9] { 2.1, 3.6, 410.0, 600.0, 125.0, 610.0, 0.0021, 0.0011, 0.00005 };  //Old standard: with perturbation.
            ParaError = new double[9] { 0.1, 0.1, 20, 30, 6, 30, 0.0002, 0.0001, 0.000005 }; //5%   //Old standard: with perturbation.
            #region Commented

            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //ParaError = new double[9] { 0.1, 0.1, 10, 15, 3, 15, 0.0001, 0.0001, 0.000005 }; //2.5%

            //ParaError = new double[9] { 0.1, 0, 0, 0, 0, 0, 0, 0, 0 };     //1
            //ParaError = new double[9] { 0, 0.1, 0, 0, 0, 0, 0, 0, 0 };   //2
            //ParaError = new double[9] { 0, 0, 10, 0, 0, 0, 0, 0, 0 };     //3
            //ParaError = new double[9] { 0, 0, 0, 15, 0, 0, 0, 0, 0 };     //4
            //ParaError = new double[9] { 0, 0, 0, 0, 3, 0, 0, 0, 0 };     //5
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 15, 0, 0, 0 };     //6
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0.0001, 0, 0 };     //7
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0.0001, 0 };     //8
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0.000005 };   //9

            //ParaError = new double[9] { 0.1, 0, 0, 0, 0, 0, 0, 0, 0 };     //1
            //ParaError = new double[9] { 0, 0.1, 0, 0, 0, 0, 0, 0, 0 };   //2
            //ParaError = new double[9] { 0, 0, 20, 0, 0, 0, 0, 0, 0 };     //3
            //ParaError = new double[9] { 0, 0, 0, 30, 0, 0, 0, 0, 0 };     //4
            //ParaError = new double[9] { 0, 0, 0, 0, 6, 0, 0, 0, 0 };     //5
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 30, 0, 0, 0 };     //6
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0.0002, 0, 0 };     //7
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0.0001, 0 };     //8
            //ParaError = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0.000005 };   //9

            //DULError = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
            //WheatLLError = new double[7] { 0, 0, 0, 0, 0, 0, 0 };

            #endregion

            // Soil parameters.
            SetSoil();
            UpdateSoil();
        }

        /// <summary>
        /// Set soil parameters. Allow soil parameters set at serialization.
        /// </summary>
        public void SetSoil()
        {
            InitSW = new double[7] { 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25 };    //Standard
            DUL = new double[7] { 0.36, 0.35, 0.34, 0.33, 0.32, 0.32, 0.32 };
            WheatLL = new double[7] { 0.10, 0.11, 0.12, 0.13, 0.14, 0.14, 0.14 };

            InitSWOpenLoop = new double[7] { 0.35, 0.34, 0.33, 0.32, 0.31, 0.31, 0.31 };    //New Standard
            DULOpenLoop = new double[7] { 0.38, 0.37, 0.36, 0.35, 0.34, 0.34, 0.34 };
            WheatLLOpenLoop = new double[7] { 0.09, 0.10, 0.11, 0.12, 0.13, 0.13, 0.13 };

            //InitSWError = new double[7] { 0.11, 0.10, 0.09, 0.08, 0.07, 0.07, 0.07 }; //Standard
            InitSWError = new double[7] { 0.10, 0.09, 0.08, 0.07, 0.06, 0.06, 0.06 }; //New Standard
            DULError = new double[7] { 0.03, 0.03, 0.03, 0.03, 0.03, 0.03, 0.03 };      //New Standard
            WheatLLError = new double[7] { 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02 };      //Standard

            SW_Corr = new List<double[]> {new double[] { 1, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4 },
                                          new double[] { 0.9, 1, 0.9, 0.8, 0.7, 0.6, 0.5 },
                                          new double[] { 0.8, 0.9, 1, 0.9, 0.8, 0.7, 0.6 },
                                          new double[] { 0.7, 0.8, 0.9, 1, 0.9, 0.8, 0.7 },
                                          new double[] { 0.6, 0.7, 0.8, 0.9, 1, 0.9, 0.8 },
                                          new double[] { 0.5, 0.6, 0.7, 0.8, 0.9, 1, 0.9 },
                                          new double[] { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 } };

            LL15 = WheatLL;
            LL15Error = WheatLLError;
            AirDry_LL = -0.02;
            SAT_DUL = 0.1;
            AirDry = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
            SAT = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
            AirDryError = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
            SATError = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Update soil parameters if Perturb_Soil is false.
        /// </summary>
        /// <param name="update"></param>
        public void UpdateSoil()
        {
            if (!Perturb_Soil)
                for (int i = 0; i < InitSW.Count(); i++)
                {
                    InitSWOpenLoop[i] = InitSW[i];
                    DULOpenLoop[i] = DUL[i];
                    WheatLLOpenLoop[i] = WheatLL[i];
                    InitSWError[i] = 0;
                    DULError[i] = 0;
                    WheatLLError[i] = 0;
                }
            //Set Airdry and LL15 to a fixed value relative to DUL and WheatLL.
            LL15 = WheatLL;
            LL15Error = WheatLLError;
        }
    }
}