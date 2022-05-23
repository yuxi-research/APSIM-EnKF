using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using StdUnits;

namespace  Models.GrazPlan
{
    /// <summary>
    /// The animal parameters object
    /// </summary>
    static public class TGAnimalParams
    {
        static private TParameterSet _GAnimalParams = null;
        /// <summary>
        /// The object that contains the animal parameters
        /// </summary>
        /// <returns></returns>
        static public TAnimalParamSet AnimalParamsGlb()
        {
            if (_GAnimalParams == null)
            {
                _GAnimalParams = new TAnimalParamSet();
                TGParamFactory.ParamXMLFactory().readDefaults("RUMINANT_PARAM_GLB", ref _GAnimalParams);
            }
            return (TAnimalParamSet)_GAnimalParams;
        }
    }

    /// <summary>
    /// Contains a blended genotype
    /// </summary>
    public struct TAnimalParamBlend
    {
        /// <summary>
        /// Breed parameters
        /// </summary>
        public TAnimalParamSet Breed;
        /// <summary>
        /// Proportion of the breed
        /// </summary>
        public double fPropn;
    }

    /// <summary>
    /// Animal parameter set
    /// </summary>
    [Serializable]
    public class TAnimalParamSet : TParameterSet
    {
        /*
        public float[] TPregArray       = array[0..299] of Float;
        TLactArray       = array[0..365] of Float;
        TConceptionArray = array[1..  3] of Float;
        */
        /// <summary>
        /// Return a copy of this object
        /// </summary>
        /// <returns></returns>
        public TAnimalParamSet Copy()
        {
            return ObjectCopier.Clone(this);
        }
        /// <summary>
        /// Condition score system to use
        /// </summary>
        public enum TCond_System { 
            /// <summary>
            /// 
            /// </summary>
            csSYSTEM1_5, 
            /// <summary>
            /// 
            /// </summary>
            csSYSTEM1_8, 
            /// <summary>
            /// 
            /// </summary>
            csSYSTEM1_9 };

        [Serializable]
        internal struct TAncestry
        {
            public string sBaseBreed;
            public double fPropn;
        }

        private double FBreedSRW;
        private double FPotFleeceWt;

        private double FDairyIntakePeak;
        private double FDairyIntakeTime;
        private double FDairyIntakeShape;
        private bool FUseDairyCurve;

        private TAncestry[] FParentage = new TAncestry[0];

        private void setSRW(double fValue)
        {
            FBreedSRW = fValue;
            FPotFleeceWt = FleeceRatio * fValue;
            if (bUseDairyCurve)
                setPeakMilk(IntakeC[11] * fValue);
        }
        private void setPotGFW(double fValue)
        {
            FPotFleeceWt = fValue;
            FleeceRatio = fValue / BreedSRW;
        }
        private void setPeakMilk(double fValue)
        {
            double fRelPeakMilk;

            if (Animal == GrazType.AnimalType.Cattle)
            {
                FUseDairyCurve = true;
                PeakMilk = fValue;
                fRelPeakMilk = PeakMilk / (IntakeC[11] * BreedSRW);

                IntakeC[8] = FDairyIntakeTime;
                IntakeC[9] = FDairyIntakeShape;
                IntakeLactC[0] = FDairyIntakePeak * ((1.0 - IntakeC[10]) + IntakeC[10] * fRelPeakMilk);
            }
        }

        /// <summary>
        /// TODO: Test this
        /// </summary>
        /// <param name="bIsWeaner"></param>
        /// <returns></returns>
        private double getDeaths(bool bIsWeaner)
        {
            if (bIsWeaner)
            {
                if (1.0 - MortRate[2] < 0)
                    throw new Exception("Power of negative number attempted in getDeaths():1.0-MortRate[2]");
            }
            else
            {
                if (1.0 - MortRate[1] < 0)
                    throw new Exception("Power of negative number attempted in getDeaths():1.0-MortRate[1]");
            }

            if (bIsWeaner)
                return 1.0 - Math.Pow(1.0 - MortRate[2], DAYSPERYR);
            else
                return 1.0 - Math.Pow(1.0 - MortRate[1], DAYSPERYR);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bIsWeaner"></param>
        /// <param name="AnnDeaths"></param>
        private void setDeaths(bool bIsWeaner, double AnnDeaths)
        {
            if (1.0 - AnnDeaths < 0)
                throw new Exception("Power of negative number attempted in setDeaths():1.0-AnnDeaths");

            if (bIsWeaner)
                MortRate[2] = 1.0 - Math.Pow(1.0 - AnnDeaths, 1.0 / DAYSPERYR);
            else
                MortRate[1] = 1.0 - Math.Pow(1.0 - AnnDeaths, 1.0 / DAYSPERYR);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private double[] getConceptions()
        {
            double[] result = new double[4];
            double fCR1 = 0.0;
            int N;

            for (N = 1; N <= MaxYoung; N++)
            {
                result[N] = computeConception(ConceiveSigs[N], N, ref fCR1);
                result[N] = 1.0E-5 * Math.Round(result[N] / 1.0E-5);
            }
            for (N = 1; N <= MaxYoung - 1; N++)
            {
                result[N] = result[N] - result[N + 1];
            }
            for (N = MaxYoung + 1; N <= 3; N++)
                result[N] = 0.0;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sigs"></param>
        /// <param name="N"></param>
        /// <param name="fCR1"></param>
        /// <returns></returns>
        private double computeConception(double[] Sigs, int N, ref double fCR1)
        {
            double fCR_N;

            if (Sigs[0] < 5.0)
                fCR_N = StdMath.SIG(1.0, Sigs);
            else
                fCR_N = 0.0;

            if (N == 1)
                fCR1 = fCR_N;
            if (1.0 - fCR1 < 0)
                throw new Exception("Power of negative number attempted in computeConception():1.0-fCR1");
            return StdMath.XDiv(fCR_N, fCR1) * (1.0 - StdMath.Pow(1.0 - fCR1, NC));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Rates">Rates array[1..  3]</param>
        private void setConceptions(double[] Rates)
        {
            double[] InitScale = new double[2] { 0.08, -0.05 };

            double PR, SeekPR, PrevPR;
            double Scale;
            double fCR1;
            double[] Sigs = new double[2];
            int N, Idx, P;

            for (N = 1; N <= MaxYoung; N++)
            {
                SeekPR = 0.0;                                                           // SeekPR is the proportion of mothers      
                for (P = N; P <= MaxYoung; P++)                                         //   conceiving at least N young            
                    SeekPR = SeekPR + Rates[P];
                SeekPR = Math.Min(SeekPR, 0.9975);                                      // If 1.0, no sensitivity to condition      

                if (SeekPR <= 0.0)                                                      // Force zero conception rate if SeekPR = 0 
                    ConceiveSigs[N][0] = 10.0;
                else
                {
                    if (Animal == GrazType.AnimalType.Sheep)                            // For sheep, use the default value for   
                    {                                                                   //   curvature and fit the 50% point      
                        Sigs = ConceiveSigs[N];
                        Idx = 0;
                    }
                    else if ((Animal == GrazType.AnimalType.Cattle) && (N == 1))        // For single calves, use the default value 
                    {                                                                   //   for the 50% point and fit the curvature
                        Sigs = ConceiveSigs[N];
                        Idx = 1;
                    }
                    else                                                                // For twin calves, use the curvature for   
                    {                                                                   //   single calves and fit the 50% point    
                        Sigs = ConceiveSigs[N - 1];
                        Idx = 0;
                    }

                    fCR1 = 0;
                    if (N > 1)
                        computeConception(ConceiveSigs[1], 1, ref fCR1);
                    PR = computeConception(Sigs, N, ref fCR1);                          // Search algorithm begins.  Only a little  
                    if (PR > SeekPR)                                                    //   search, so coded for size not speed    
                        Scale = Math.Abs(InitScale[Idx]);
                    else
                        Scale = -InitScale[Idx];

                    do
                    {
                        PrevPR = PR;
                        Sigs[Idx] = Sigs[Idx] + Scale;                                  // Move the parameter up or down...         
                        PR = computeConception(Sigs, N, ref fCR1);                      // Compute the corresponding pregnancy rate 
                                                                                        //   at (BC x size) = 1                     
                        if ((PrevPR > SeekPR) && (PR <= SeekPR))                        // If the difference (current-wanted)       
                            Scale = -0.25 * Scale;                                      //   changes sign, reduce the step size and 
                        else if ((PrevPR < SeekPR) && (PR >= SeekPR))                   //   change direction                       
                            Scale = -0.25 * Scale;
                    } while ((Math.Abs(SeekPR - PR) >= 1.0E-6) && (Math.Abs(Scale) >= 0.00001));     // until (Abs(SeekPR-PR) < 1.0E-6) or (Abs(Scale) < 0.00001);

                    Array.Copy(Sigs, ConceiveSigs[N], ConceiveSigs[N].Length);
                } //{_ SeekPR > 0 _}
            } //{_ FOR N := 1 TO MaxYoung _}
        }

        private int getGestation()
        {
            return Convert.ToInt32(Math.Round(PregC[1]));
        }

        /// <summary>
        /// Overrides the base function and copies all the animal parameters
        /// </summary>
        /// <param name="srcSet"></param>
        /// <param name="bCopyData"></param>
        override protected void copyParams(TParameterSet srcSet, bool bCopyData)
        {
            int Idx;

            base.copyParams(srcSet, false);

            TAnimalParamSet prmSet = (TAnimalParamSet)srcSet;

            if (bCopyData && (prmSet != null))
            {
                FBreedSRW = prmSet.FBreedSRW;
                FPotFleeceWt = prmSet.FPotFleeceWt;
                FDairyIntakePeak = prmSet.FDairyIntakePeak;
                FDairyIntakeTime = prmSet.FDairyIntakeTime;
                FDairyIntakeShape = prmSet.FDairyIntakeShape;
                FUseDairyCurve = prmSet.FUseDairyCurve;
                Array.Resize(ref FParentage, prmSet.FParentage.Length);
                for (Idx = 0; Idx <= FParentage.Length - 1; Idx++)
                    FParentage[Idx] = prmSet.FParentage[Idx];

                sEditor = prmSet.sEditor;
                sEditDate = prmSet.sEditDate;
                Animal = prmSet.Animal;
                MaxYoung = prmSet.MaxYoung;
                Array.Copy(prmSet.SRWScalars,SRWScalars, prmSet.SRWScalars.Length);
                FleeceRatio = prmSet.FleeceRatio;
                MaxFleeceDiam = prmSet.MaxFleeceDiam;
                bDairyBreed = prmSet.bDairyBreed;
                PeakMilk = prmSet.PeakMilk;
                Array.Copy(prmSet.MortRate, MortRate, prmSet.MortRate.Length);
                Array.Copy(prmSet.MortAge, MortAge, prmSet.MortAge.Length);
                MortIntensity = prmSet.MortIntensity;
                MortCondConst = prmSet.MortCondConst;
                MortWtDiff = prmSet.MortWtDiff;
                Array.Copy(prmSet.GrowthC,GrowthC, prmSet.GrowthC.Length);
                Array.Copy(prmSet.IntakeC, IntakeC,prmSet.IntakeC.Length);
                Array.Copy(prmSet.IntakeLactC, IntakeLactC, prmSet.IntakeLactC.Length);
                Array.Copy(prmSet.GrazeC, GrazeC, prmSet.GrazeC.Length);
                Array.Copy(prmSet.EfficC, EfficC, prmSet.EfficC.Length);
                Array.Copy(prmSet.MaintC,MaintC, prmSet.MaintC.Length);
                Array.Copy(prmSet.DgProtC,DgProtC, prmSet.DgProtC.Length);
                Array.Copy(prmSet.ProtC, ProtC, prmSet.ProtC.Length);
                Array.Copy(prmSet.PregC, PregC, prmSet.PregC.Length);
                Array.Copy(prmSet.PregScale, PregScale, prmSet.PregScale.Length);
                Array.Copy(prmSet.BirthWtScale, BirthWtScale,prmSet.BirthWtScale.Length);
                Array.Copy(prmSet.PeakLactC, PeakLactC, prmSet.PeakLactC.Length);
                Array.Copy(prmSet.LactC,LactC, prmSet.LactC.Length);
                Array.Copy(prmSet.WoolC, WoolC, prmSet.WoolC.Length);
                Array.Copy(prmSet.ChillC, ChillC,prmSet.ChillC.Length);
                Array.Copy(prmSet.GainC, GainC, prmSet.GainC.Length);
                Array.Copy(prmSet.PhosC, PhosC, prmSet.PhosC.Length);
                Array.Copy(prmSet.SulfC, SulfC, prmSet.SulfC.Length);
                Array.Copy(prmSet.MethC, MethC, prmSet.MethC.Length);
                Array.Copy(prmSet.AshAlkC, AshAlkC, prmSet.AshAlkC.Length);
                OvulationPeriod = prmSet.OvulationPeriod;
                Array.Copy(prmSet.Puberty, Puberty, prmSet.Puberty.Length);
                Array.Copy(prmSet.DayLengthConst, DayLengthConst, prmSet.DayLengthConst.Length);
                for (int i = 0; i < prmSet.ConceiveSigs.Length; i++)
                    Array.Copy(prmSet.ConceiveSigs[i], ConceiveSigs[i], prmSet.ConceiveSigs[i].Length);
                FertWtDiff = prmSet.FertWtDiff;
                Array.Copy(prmSet.ToxaemiaSigs, ToxaemiaSigs, prmSet.ToxaemiaSigs.Length);
                Array.Copy(prmSet.DystokiaSigs, DystokiaSigs, prmSet.DystokiaSigs.Length);
                Array.Copy(prmSet.ExposureConsts,ExposureConsts, prmSet.ExposureConsts.Length);
                SelfWeanPropn = prmSet.SelfWeanPropn;

                for (Idx = 0; Idx <= iDefinitionCount() - 1; Idx++)
                    getDefinition(Idx).setDefined(prmSet.getDefinition(Idx));
            }
        }
        /// <summary>
        /// Make a new animal parameter set that is a child of this one
        /// </summary>
        /// <returns></returns>
        override protected TParameterSet makeChild()
        {
            return new TAnimalParamSet(this);
        }

        /// <summary>
        /// 
        /// </summary>
        override protected void defineEntries()
        {
            defineParameters("editor", ptyText);
            defineParameters("edited", ptyText);

            defineParameters("animal", ptyText);
            defineParameters("srw", ptyReal);
            defineParameters("dairy", ptyBool);
            defineParameters("c-pfw", ptyReal);
            defineParameters("c-mu", ptyReal);
            defineParameters("c-srs-castr;male", ptyReal);
            defineParameters("c-n-1:4", ptyReal);
            defineParameters("c-i-1:20", ptyReal);
            defineParameters("c-idy-1:3", ptyReal);
            defineParameters("c-imx-1:3", ptyReal);
            defineParameters("c-r-1:20", ptyReal);
            defineParameters("c-k-1:16", ptyReal);
            defineParameters("c-m-1:17", ptyReal);
            defineParameters("c-rd-1:8", ptyReal);
            defineParameters("c-a-1:9", ptyReal);
            defineParameters("c-p-1:13", ptyReal);
            defineParameters("c-p14-1:3", ptyReal);
            defineParameters("c-p15-1:3", ptyReal);
            defineParameters("c-l0-1:3", ptyReal);
            defineParameters("c-l-1:25", ptyReal);
            defineParameters("c-w-1:14", ptyReal);
            defineParameters("c-c-1:16", ptyReal);
            defineParameters("c-g-1:18", ptyReal);
            defineParameters("c-ph-1:15", ptyReal);
            defineParameters("c-su-1:4", ptyReal);
            defineParameters("c-h-1:7", ptyReal);
            defineParameters("c-aa-1:3", ptyReal);
            defineParameters("c-f1-1:3", ptyReal);
            defineParameters("c-f2-1:3", ptyReal);
            defineParameters("c-f3-1:3", ptyReal);
            defineParameters("c-f4", ptyInt);
            defineParameters("c-pbt-female;male", ptyInt);
            defineParameters("c-d-1:15", ptyReal);
            defineParameters("c-swn", ptyReal);
        }
        /// <summary>
        /// Get the floating point value
        /// </summary>
        /// <param name="sTagList"></param>
        /// <returns></returns>
        override protected double getRealParam(string[] sTagList)
        {
            int Idx;

            double result = 0.0;

            if (sTagList[0] == "srw")
                result = BreedSRW;
            else if (sTagList[0] == "c")
            {
                if (sTagList[1] == "pfw")
                    result = FleeceRatio;
                else if (sTagList[1] == "mu")
                    result = MaxFleeceDiam;
                else if (sTagList[1] == "srs")
                {
                    if (sTagList[2] == "castr")
                        result = SRWScalars[(int)GrazType.ReproType.Castrated];
                    else if (sTagList[2] == "male")
                        result = SRWScalars[(int)GrazType.ReproType.Male];
                }
                else if (sTagList[1] == "swn")
                    result = SelfWeanPropn;
                else
                {
                    Idx = Convert.ToInt32(sTagList[2]);

                    if (sTagList[1] == "n")
                        result = GrowthC[Idx];
                    else if (sTagList[1] == "i")
                        result = IntakeC[Idx];
                    else if (sTagList[1] == "idy")
                    {
                        switch (Idx)
                        {
                            case 1: result = FDairyIntakePeak;
                                break;
                            case 2: result = FDairyIntakeTime;
                                break;
                            case 3: result = FDairyIntakeShape;
                                break;
                        }
                    }
                    else if (sTagList[1] == "imx")
                        result = IntakeLactC[Idx];
                    else if (sTagList[1] == "r")
                        result = GrazeC[Idx];
                    else if (sTagList[1] == "k")
                        result = EfficC[Idx];
                    else if (sTagList[1] == "m")
                        result = MaintC[Idx];
                    else if (sTagList[1] == "rd")
                        result = DgProtC[Idx];
                    else if (sTagList[1] == "a")
                        result = ProtC[Idx];
                    else if (sTagList[1] == "p")
                        result = PregC[Idx];
                    else if (sTagList[1] == "p14")
                        result = PregScale[Idx];
                    else if (sTagList[1] == "p15")
                        result = BirthWtScale[Idx];
                    else if (sTagList[1] == "l0")
                        result = PeakLactC[Idx];
                    else if (sTagList[1] == "l")
                        result = LactC[Idx];
                    else if (sTagList[1] == "w")
                        result = WoolC[Idx];
                    else if (sTagList[1] == "c")
                        result = ChillC[Idx];
                    else if (sTagList[1] == "g")
                        result = GainC[Idx];
                    else if (sTagList[1] == "ph")
                        result = PhosC[Idx];
                    else if (sTagList[1] == "su")
                        result = SulfC[Idx];
                    else if (sTagList[1] == "h")
                        result = MethC[Idx];
                    else if (sTagList[1] == "aa")
                        result = AshAlkC[Idx];
                    else if (sTagList[1] == "f1")
                        result = DayLengthConst[Idx];
                    else if (sTagList[1] == "f2")
                        result = ConceiveSigs[Idx][0];
                    else if (sTagList[1] == "f3")
                        result = ConceiveSigs[Idx][1];
                    else if (sTagList[1] == "d")
                    {
                        switch (Idx)
                        {
                            case 1: result = MortRate[1];
                                break;
                            case 2: result = MortIntensity;
                                break;
                            case 3: result = MortCondConst;
                                break;
                            case 4:
                            case 5: result = ToxaemiaSigs[Idx - 4];
                                break;
                            case 6:
                            case 7: result = DystokiaSigs[Idx - 6];
                                break;
                            case 8:
                            case 9:
                            case 10:
                            case 11: result = ExposureConsts[Idx - 8];
                                break;
                            case 12: result = MortWtDiff;
                                break;
                            case 13: result = MortRate[2];
                                break;
                            case 14: result = MortAge[1];
                                break;
                            case 15: result = MortAge[2];
                                break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTagList"></param>
        /// <returns></returns>
        override protected int getIntParam(string[] sTagList)
        {
            int result = 0;

            if (sTagList[0] == "c")
            {
                if (sTagList[1] == "f4")
                    result = OvulationPeriod;
                else if (sTagList[1] == "pbt")
                {
                    if (sTagList[2] == "female")
                        result = Puberty[0];    //[false]
                    else if (sTagList[2] == "male")
                        result = Puberty[1]; //[true]
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTagList"></param>
        /// <returns></returns>
        override protected string getTextParam(string[] sTagList)
        {
            string result = "";
            if (sTagList[0] == "editor")
                result = sEditor;
            else if (sTagList[0] == "edited")
                result = sEditDate;
            else if (sTagList[0] == "animal")
                result = GrazType.AnimalText[(int)Animal].ToLower();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTagList"></param>
        /// <returns></returns>
        override protected bool getBoolParam(string[] sTagList)
        {
            bool result = false;
            if (sTagList[0] == "dairy")
                result = bDairyBreed;

            return result;
        }
        /// <summary>
        /// Set the floating point value
        /// </summary>
        /// <param name="sTagList"></param>
        /// <param name="fValue"></param>
        protected override void setRealParam(string[] sTagList, double fValue)
        {
            int Idx;

            if (sTagList[0] == "srw")
                FBreedSRW = fValue;
            else if (sTagList[0] == "c")
            {
                if (sTagList[1] == "pfw")
                    FleeceRatio = fValue;
                else if (sTagList[1] == "mu")
                    MaxFleeceDiam = fValue;
                else if (sTagList[1] == "srs")
                {
                    if (sTagList[2] == "castr")
                        SRWScalars[(int)GrazType.ReproType.Castrated] = fValue;
                    else if (sTagList[2] == "male")
                        SRWScalars[(int)GrazType.ReproType.Male] = fValue;
                }
                else if (sTagList[1] == "swn")
                    SelfWeanPropn = fValue;
                else
                {
                    Idx = Convert.ToInt32(sTagList[2]);

                    if (sTagList[1] == "n")
                        GrowthC[Idx] = fValue;
                    else if (sTagList[1] == "i")
                        IntakeC[Idx] = fValue;
                    else if (sTagList[1] == "idy")
                    {
                        switch (Idx)
                        {
                            case 1: FDairyIntakePeak = fValue;
                                break;
                            case 2: FDairyIntakeTime = fValue;
                                break;
                            case 3: FDairyIntakeShape = fValue;
                                break;
                        }
                    }
                    else if (sTagList[1] == "imx")
                        IntakeLactC[Idx] = fValue;
                    else if (sTagList[1] == "r")
                        GrazeC[Idx] = fValue;
                    else if (sTagList[1] == "k")
                        EfficC[Idx] = fValue;
                    else if (sTagList[1] == "m")
                        MaintC[Idx] = fValue;
                    else if (sTagList[1] == "rd")
                        DgProtC[Idx] = fValue;
                    else if (sTagList[1] == "a")
                        ProtC[Idx] = fValue;
                    else if (sTagList[1] == "p")
                        PregC[Idx] = fValue;
                    else if (sTagList[1] == "p14")
                        PregScale[Idx] = fValue;
                    else if (sTagList[1] == "p15")
                        BirthWtScale[Idx] = fValue;
                    else if (sTagList[1] == "l0")
                        PeakLactC[Idx] = fValue;
                    else if (sTagList[1] == "l")
                        LactC[Idx] = fValue;
                    else if (sTagList[1] == "w")
                        WoolC[Idx] = fValue;
                    else if (sTagList[1] == "c")
                        ChillC[Idx] = fValue;
                    else if (sTagList[1] == "g")
                        GainC[Idx] = fValue;
                    else if (sTagList[1] == "ph")
                        PhosC[Idx] = fValue;
                    else if (sTagList[1] == "su")
                        SulfC[Idx] = fValue;
                    else if (sTagList[1] == "h")
                        MethC[Idx] = fValue;
                    else if (sTagList[1] == "aa")
                        AshAlkC[Idx] = fValue;
                    else if (sTagList[1] == "f1")
                        DayLengthConst[Idx] = fValue;
                    else if (sTagList[1] == "f2")
                        ConceiveSigs[Idx][0] = fValue;
                    else if (sTagList[1] == "f3")
                        ConceiveSigs[Idx][1] = fValue;
                    else if (sTagList[1] == "d")
                    {
                        switch (Idx)
                        {
                            case 1: MortRate[1] = fValue;
                                break;
                            case 2: MortIntensity = fValue;
                                break;
                            case 3: MortCondConst = fValue;
                                break;
                            case 4:
                            case 5: ToxaemiaSigs[Idx - 4] = fValue;
                                break;
                            case 6:
                            case 7: DystokiaSigs[Idx - 6] = fValue;
                                break;
                            case 8:
                            case 9:
                            case 10:
                            case 11: ExposureConsts[Idx - 8] = fValue;
                                break;
                            case 12: MortWtDiff = fValue;
                                break;
                            case 13: MortRate[2] = fValue;
                                break;
                            case 14: MortAge[1] = fValue;
                                break;
                            case 15: MortAge[2] = fValue;
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTagList"></param>
        /// <param name="iValue"></param>
        override protected void setIntParam(string[] sTagList, int iValue)
        {
            if (sTagList[0] == "c")
            {
                if (sTagList[1] == "f4")
                    OvulationPeriod = iValue;
                else if (sTagList[1] == "pbt")
                {
                    if (sTagList[2] == "female")
                        Puberty[0] = iValue;    //[false]
                    else if (sTagList[2] == "male")
                        Puberty[1] = iValue; //[true]
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTagList"></param>
        /// <param name="sValue"></param>
        override protected void setTextParam(string[] sTagList, string sValue)
        {
            if (sTagList[0] == "editor")
                sEditor = sValue;
            else if (sTagList[0] == "edited")
                sEditDate = sValue;
            else if (sTagList[0] == "animal")
            {
                if (sValue.ToLower().Trim() == GrazType.AnimalText[(int)GrazType.AnimalType.Cattle].ToLower())
                    Animal = GrazType.AnimalType.Cattle;
                else
                    Animal = GrazType.AnimalType.Sheep;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTagList"></param>
        /// <param name="bValue"></param>
        override protected void setBoolParam(string[] sTagList, bool bValue)
        {
            if (sTagList[0] == "dairy")
                bDairyBreed = bValue;
        }

        /// <summary>
        /// Editor of the parameters
        /// </summary>
        public string sEditor;
        /// <summary>
        /// Date edited
        /// </summary>
        public string sEditDate;
        /// <summary>
        /// Animal type
        /// </summary>
        public GrazType.AnimalType Animal;
        /// <summary>
        /// Maximum young
        /// </summary>
        public int MaxYoung;
        /// <summary>
        /// Standard reference weights
        /// </summary>
        public double[] SRWScalars = new double[2];

        /// <summary>
        /// Potential greasy fleece weight:SRW
        /// </summary>
        public double FleeceRatio;                          
        /// <summary>
        /// In microns
        /// </summary>
        public double MaxFleeceDiam;                                                    
        /// <summary>
        /// Fixed attribute (read in)
        /// </summary>
        public bool bDairyBreed;                            

        /// <summary>
        /// WM(peak)
        /// </summary>
        public double PeakMilk;                                                          

        /// <summary>
        /// Background death rate, per day  [1..2]      
        /// </summary>
        public double[] MortRate = new double[3];           
        /// <summary>
        /// 
        /// </summary>
        public double[] MortAge = new double[3];            //[1..2]
        /// <summary>
        /// Rate of mortality increase for underweight animals
        /// </summary>
        public double MortIntensity;                               
        /// <summary>
        /// Fraction of normal body weight in animals of Size=1 at which mortality starts to increase
        /// </summary>
        public double MortCondConst;                        
        /// <summary>
        /// Weight differential in dying animals  
        /// </summary>
        public double MortWtDiff;                           
        /// <summary>
        /// C(N)
        /// </summary>
        public double[] GrowthC = new double[5];            
        /// <summary>
        /// C(I)
        /// </summary>
        public double[] IntakeC = new double[21];                                             
        /// <summary>
        /// C(I,15)
        /// </summary>
        public double[] IntakeLactC = new double[4];                                       
        /// <summary>
        /// C(R)
        /// </summary>
        public double[] GrazeC = new double[21];                                              
        /// <summary>
        /// C(K)
        /// </summary>
        public double[] EfficC = new double[17];                                              
        /// <summary>
        /// C(M)
        /// </summary>
        public double[] MaintC = new double[18];                                              
        /// <summary>
        /// C(RDP)
        /// </summary>
        public double[] DgProtC = new double[9];                                            
        /// <summary>
        /// C(A)
        /// </summary>
        public double[] ProtC = new double[10];                                               
        /// <summary>
        /// C(P)
        /// </summary>
        public double[] PregC = new double[14];                                               
        /// <summary>
        /// C(P,14,Y)
        /// </summary>
        public double[] PregScale = new double[4];                                    
        /// <summary>
        /// C(P,15,Y)
        /// </summary>
        public double[] BirthWtScale = new double[4];                              
        /// <summary>
        /// C(L,0,Y)
        /// </summary>
        public double[] PeakLactC = new double[4];                                     
        /// <summary>
        /// C(L)
        /// </summary>
        public double[] LactC = new double[26];                                               
        /// <summary>
        /// C(W)
        /// </summary>
        public double[] WoolC = new double[15];                                               
        /// <summary>
        /// C(C)
        /// </summary>
        public double[] ChillC = new double[17];                                             
        /// <summary>
        /// C(G)
        /// </summary>
        public double[] GainC = new double[19];                                               
        /// <summary>
        /// 
        /// </summary>
        public double[] PhosC = new double[16];
        /// <summary>
        /// 
        /// </summary>
        public double[] SulfC = new double[5];
        /// <summary>
        /// 
        /// </summary>
        public double[] MethC = new double[8];
        /// <summary>
        /// Ash alkalinity values
        /// </summary>
        public double[] AshAlkC = new double[4];            
        /// <summary>
        /// 
        /// </summary>
        public int OvulationPeriod;
        /// <summary>
        /// 
        /// </summary>
        public int[] Puberty = new int[2];                  //array[Boolean]
        /// <summary>
        /// 
        /// </summary>
        public double[] DayLengthConst = new double[4];     //array[1..3]

        /// <summary>
        /// 
        /// </summary>
        public double[][] ConceiveSigs = new double[4][];   //[0..3][0..1]

        /// <summary>
        /// 
        /// </summary>
        public double FertWtDiff;
        /// <summary>
        /// 
        /// </summary>
        public double[] ToxaemiaSigs = new double[2];       //array[0..1]
        /// <summary>
        /// 
        /// </summary>
        public double[] DystokiaSigs = new double[2];       //array[0..1]
        /// <summary>
        /// 
        /// </summary>
        public double[] ExposureConsts = new double[4];     //array[0..3]
        /// <summary>
        /// 
        /// </summary>
        public double SelfWeanPropn;

        /// <summary>
        /// Construct and animal parameter set
        /// </summary>
        public TAnimalParamSet()
            : base()
        {
            //create a new array
            for (int i = 0; i < ConceiveSigs.Length; i++)
                ConceiveSigs[i] = new double[2];
            ConstructCopy(null);
        }

        /// <summary>
        /// Construct an animal parameter set from a source one
        /// </summary>
        public TAnimalParamSet(TAnimalParamSet src)
            : base(src)
        {
            for (int i = 0; i < ConceiveSigs.Length; i++)
                ConceiveSigs[i] = new double[2];
            ConstructCopy(src);
        }

        /// <summary>
        /// Alternative copy constructor
        /// </summary>
        /// <param name="aParent"></param>
        /// <param name="srcSet"></param>
        public TAnimalParamSet(TParameterSet aParent, TAnimalParamSet srcSet)
            : base(aParent/*, srcSet*/)
        {
            //create a new array
            for (int i = 0; i < ConceiveSigs.Length; i++)
                ConceiveSigs[i] = new double[2];
            ConstructCopy(srcSet);

            int Jdx;

            if (srcSet != null)
            {
                if (srcSet.Animal == GrazType.AnimalType.Sheep)
                    setPotGFW(srcSet.PotentialGFW);

                if (srcSet.bUseDairyCurve)
                    setPeakMilk(srcSet.PotMilkYield);

                if (srcSet.FParentage.Length == 0)
                {
                    Array.Resize(ref FParentage, 1);
                    FParentage[0].sBaseBreed = srcSet.sName;
                    FParentage[0].fPropn = 1.0;
                }
                else
                {
                    Array.Resize(ref FParentage, srcSet.FParentage.Length);
                    for (Jdx = 0; Jdx <= FParentage.Length - 1; Jdx++)
                    {
                        FParentage[Jdx].sBaseBreed = srcSet.FParentage[Jdx].sBaseBreed;
                        FParentage[Jdx].fPropn = srcSet.FParentage[Jdx].fPropn;
                    }
                }
            }
        }

        /// <summary>
        /// Copies a parameter set from AnimalParamsGlb
        /// </summary>
        /// <param name="sBreedName"></param>
        public static TAnimalParamSet CreateFactory(string sBreedName)
        {
            TAnimalParamSet newObj = null;

            TAnimalParamSet baseParams;

            baseParams = (TAnimalParamSet)TGAnimalParams.AnimalParamsGlb().getNode(sBreedName);
            if (baseParams != null)
                newObj = new TAnimalParamSet(null, baseParams);
            else
            {
                newObj = new TAnimalParamSet();
                throw new Exception("Breed name \"" + sBreedName + "\" not recognised");
            }
            
            return newObj;
        }

        /// <summary>
        /// Creates an object based on the parameters passed
        /// </summary>
        /// <param name="sBreedName"></param>
        /// <param name="Blend"></param>
        /// <returns></returns>
        public static TAnimalParamSet CreateFactory(string sBreedName, TAnimalParamBlend[] Blend)
        {
            TAnimalParamSet newObj = null;
            if (Blend.Length == 0)                                                   // No mixture of breeds provided, so     
                newObj = CreateFactory(sBreedName);                                     //   copy a breed from AnimalParamsGlb   
            else if (Blend.Length == 2)                                             // Special case: optimized for speed     
            {
                newObj = new TAnimalParamSet((TParameterSet)null, (TAnimalParamSet)null);
            }
            else
            {
                newObj = new TAnimalParamSet(null, Blend[0].Breed);                            // Sets the integer, string and Boolean  
            }
            newObj.InitParameterSet(sBreedName, Blend);

            return newObj;
        }

        /// <summary>
        /// Called by CreateFactory() and creates a mixture of several genotypes                                       
        /// </summary>
        /// <param name="sBreedName"></param>
        /// <param name="Blend"></param>
        virtual public void InitParameterSet(string sBreedName, TAnimalParamBlend[] Blend)
        {
            TParameterDefinition prmDefn;
            TAnimalParamSet Breed0;
            TAnimalParamSet Breed1;
            double fPropn0;
            double fPropn1;
            double fParamSum;
            double fPropnSum;
            int iDecPlaces;
            int Idx, Jdx, Kdx;
            //TGrazType.ReproType Repro;

            if (Blend.Length == 2)                                             // Special case: optimized for speed     
            {                                                                  //   (used in producing offspring)       
                Breed0 = Blend[0].Breed;
                Breed1 = Blend[1].Breed;

                fPropn0 = Blend[0].fPropn;
                fPropn1 = Blend[1].fPropn;
                if (fPropn1 > 0.0)
                    fPropn0 = fPropn0 / (fPropn0 + fPropn1);
                else
                    fPropn0 = 1.0;
                fPropn1 = 1.0 - fPropn0;

                sEditor = Breed0.sEditor;                                       // String and integer parameters         
                sEditDate = Breed0.sEditDate;                                     //   (consistent with the general case)  
                Animal = Breed0.Animal;
                bDairyBreed = Breed0.bDairyBreed;
                MaxYoung = Breed0.MaxYoung;
                FUseDairyCurve = Breed0.FUseDairyCurve;
                OvulationPeriod = Breed0.OvulationPeriod;
                Puberty = Breed0.Puberty;

                FBreedSRW = fPropn0 * Breed0.FBreedSRW + fPropn1 * Breed1.FBreedSRW;
                FPotFleeceWt = fPropn0 * Breed0.FPotFleeceWt + fPropn1 * Breed1.FPotFleeceWt;
                FDairyIntakePeak = fPropn0 * Breed0.FDairyIntakePeak + fPropn1 * Breed1.FDairyIntakePeak;
                FDairyIntakeTime = fPropn0 * Breed0.FDairyIntakeTime + fPropn1 * Breed1.FDairyIntakeTime;
                FDairyIntakeShape = fPropn0 * Breed0.FDairyIntakeShape + fPropn1 * Breed1.FDairyIntakeShape;
                FleeceRatio = fPropn0 * Breed0.FleeceRatio + fPropn1 * Breed1.FleeceRatio;
                MaxFleeceDiam = fPropn0 * Breed0.MaxFleeceDiam + fPropn1 * Breed1.MaxFleeceDiam;
                PeakMilk = fPropn0 * Breed0.PeakMilk + fPropn1 * Breed1.PeakMilk;
                for (Idx = 1; Idx <= 2; Idx++)
                    MortRate[Idx] = fPropn0 * Breed0.MortRate[Idx] + fPropn1 * Breed1.MortRate[Idx];
                for (Idx = 1; Idx <= 2; Idx++)
                    MortAge[Idx] = fPropn0 * Breed0.MortAge[Idx] + fPropn1 * Breed1.MortAge[Idx];
                MortIntensity = fPropn0 * Breed0.MortIntensity + fPropn1 * Breed1.MortIntensity;
                MortCondConst = fPropn0 * Breed0.MortCondConst + fPropn1 * Breed1.MortCondConst;
                MortWtDiff = fPropn0 * Breed0.MortWtDiff + fPropn1 * Breed1.MortWtDiff;

                for (Idx = 0; Idx < SRWScalars.Length; Idx++) SRWScalars[Idx] = fPropn0 * Breed0.SRWScalars[Idx] + fPropn1 * Breed1.SRWScalars[Idx];
                for (Idx = 1; Idx < GrowthC.Length; Idx++) GrowthC[Idx] = fPropn0 * Breed0.GrowthC[Idx] + fPropn1 * Breed1.GrowthC[Idx];
                for (Idx = 1; Idx < IntakeC.Length; Idx++) IntakeC[Idx] = fPropn0 * Breed0.IntakeC[Idx] + fPropn1 * Breed1.IntakeC[Idx];
                for (Idx = 0; Idx < IntakeLactC.Length; Idx++) IntakeLactC[Idx] = fPropn0 * Breed0.IntakeLactC[Idx] + fPropn1 * Breed1.IntakeLactC[Idx];
                for (Idx = 1; Idx < GrazeC.Length; Idx++) GrazeC[Idx] = fPropn0 * Breed0.GrazeC[Idx] + fPropn1 * Breed1.GrazeC[Idx];
                for (Idx = 1; Idx < EfficC.Length; Idx++) EfficC[Idx] = fPropn0 * Breed0.EfficC[Idx] + fPropn1 * Breed1.EfficC[Idx];
                for (Idx = 1; Idx < MaintC.Length; Idx++) MaintC[Idx] = fPropn0 * Breed0.MaintC[Idx] + fPropn1 * Breed1.MaintC[Idx];
                for (Idx = 1; Idx < DgProtC.Length; Idx++) DgProtC[Idx] = fPropn0 * Breed0.DgProtC[Idx] + fPropn1 * Breed1.DgProtC[Idx];
                for (Idx = 1; Idx < ProtC.Length; Idx++) ProtC[Idx] = fPropn0 * Breed0.ProtC[Idx] + fPropn1 * Breed1.ProtC[Idx];
                for (Idx = 1; Idx < PregC.Length; Idx++) PregC[Idx] = fPropn0 * Breed0.PregC[Idx] + fPropn1 * Breed1.PregC[Idx];
                for (Idx = 1; Idx < PregScale.Length; Idx++) PregScale[Idx] = fPropn0 * Breed0.PregScale[Idx] + fPropn1 * Breed1.PregScale[Idx];
                for (Idx = 1; Idx < BirthWtScale.Length; Idx++) BirthWtScale[Idx] = fPropn0 * Breed0.BirthWtScale[Idx] + fPropn1 * Breed1.BirthWtScale[Idx];
                for (Idx = 1; Idx < PeakLactC.Length; Idx++) PeakLactC[Idx] = fPropn0 * Breed0.PeakLactC[Idx] + fPropn1 * Breed1.PeakLactC[Idx];
                for (Idx = 1; Idx < LactC.Length; Idx++) LactC[Idx] = fPropn0 * Breed0.LactC[Idx] + fPropn1 * Breed1.LactC[Idx];
                for (Idx = 1; Idx < WoolC.Length; Idx++) WoolC[Idx] = fPropn0 * Breed0.WoolC[Idx] + fPropn1 * Breed1.WoolC[Idx];
                for (Idx = 1; Idx < ChillC.Length; Idx++) ChillC[Idx] = fPropn0 * Breed0.ChillC[Idx] + fPropn1 * Breed1.ChillC[Idx];
                for (Idx = 1; Idx < GainC.Length; Idx++) GainC[Idx] = fPropn0 * Breed0.GainC[Idx] + fPropn1 * Breed1.GainC[Idx];
                for (Idx = 1; Idx < PhosC.Length; Idx++) PhosC[Idx] = fPropn0 * Breed0.PhosC[Idx] + fPropn1 * Breed1.PhosC[Idx];
                for (Idx = 1; Idx < SulfC.Length; Idx++) SulfC[Idx] = fPropn0 * Breed0.SulfC[Idx] + fPropn1 * Breed1.SulfC[Idx];
                for (Idx = 1; Idx < MethC.Length; Idx++) MethC[Idx] = fPropn0 * Breed0.MethC[Idx] + fPropn1 * Breed1.MethC[Idx];
                for (Idx = 1; Idx < AshAlkC.Length; Idx++) AshAlkC[Idx] = fPropn0 * Breed0.AshAlkC[Idx] + fPropn1 * Breed1.AshAlkC[Idx];
                for (Idx = 1; Idx < DayLengthConst.Length; Idx++) DayLengthConst[Idx] = fPropn0 * Breed0.DayLengthConst[Idx] + fPropn1 * Breed1.DayLengthConst[Idx];
                for (Idx = 0; Idx < ToxaemiaSigs.Length; Idx++) ToxaemiaSigs[Idx] = fPropn0 * Breed0.ToxaemiaSigs[Idx] + fPropn1 * Breed1.ToxaemiaSigs[Idx];
                for (Idx = 0; Idx < DystokiaSigs.Length; Idx++) DystokiaSigs[Idx] = fPropn0 * Breed0.DystokiaSigs[Idx] + fPropn1 * Breed1.DystokiaSigs[Idx];
                for (Idx = 0; Idx < ExposureConsts.Length; Idx++) ExposureConsts[Idx] = fPropn0 * Breed0.ExposureConsts[Idx] + fPropn1 * Breed1.ExposureConsts[Idx];

                FertWtDiff = fPropn0 * Breed0.FertWtDiff + fPropn1 * Breed1.FertWtDiff;
                SelfWeanPropn = fPropn0 * Breed0.SelfWeanPropn + fPropn1 * Breed1.SelfWeanPropn;
                for (Idx = 1; Idx < ConceiveSigs.Length; Idx++)
                    for (Jdx = 0; Jdx < ConceiveSigs[Idx].Length; Jdx++)
                        ConceiveSigs[Idx][Jdx] = fPropn0 * Breed0.ConceiveSigs[Idx][Jdx] + fPropn1 * Breed1.ConceiveSigs[Idx][Jdx];

                for (Idx = 0; Idx <= iDefinitionCount() - 1; Idx++)
                    getDefinition(Idx).setDefined(Blend[0].Breed.getDefinition(Idx));
            }
            else                                                                         // Mixture of breeds provided            
            {
                if (Blend.Length > 1)                                                 // Blend the numeric parameter values    
                {
                    for (Idx = 0; Idx <= iParamCount() - 1; Idx++)
                    {
                        prmDefn = getParam(Idx);
                        if (prmDefn.paramType == ptyReal)
                        {
                            fParamSum = 0.0;
                            fPropnSum = 0.0;
                            for (Jdx = 0; Jdx <= Blend.Length - 1; Jdx++)
                            {
                                if (Blend[Jdx].Breed.bIsDefined(prmDefn.sFullName))
                                {
                                    fParamSum = fParamSum + Blend[Jdx].fPropn * Blend[Jdx].Breed.fParam(prmDefn.sFullName);
                                    fPropnSum = fPropnSum + Blend[Jdx].fPropn;
                                }
                            }
                            if (fPropnSum > 0.0)
                                setParam(prmDefn.sFullName, fParamSum / fPropnSum);
                        }
                    }
                }
            }

            if (Blend.Length > 0)
            {
                fPropnSum = 0.0;
                for (Jdx = 0; Jdx <= Blend.Length - 1; Jdx++)
                    fPropnSum = fPropnSum + Blend[Jdx].fPropn;

                if (fPropnSum > 0.0)
                {
                    for (Idx = 0; Idx <= FParentage.Length - 1; Idx++)
                        FParentage[Idx].fPropn = 0.0;
                    for (Jdx = 0; Jdx <= Blend.Length - 1; Jdx++)
                    {
                        for (Kdx = 0; Kdx <= Blend[Jdx].Breed.FParentage.Length - 1; Kdx++)
                        {
                            Idx = 0;
                            while ((Idx < FParentage.Length) && (Blend[Jdx].Breed.FParentage[Kdx].sBaseBreed != FParentage[Idx].sBaseBreed))
                                Idx++;
                            if (Idx == FParentage.Length)
                            {
                                Array.Resize(ref FParentage, Idx + 1);
                                FParentage[Idx].sBaseBreed = Blend[Jdx].Breed.FParentage[Kdx].sBaseBreed;
                                FParentage[Idx].fPropn = 0.0;
                            }
                            FParentage[Idx].fPropn = FParentage[Idx].fPropn
                                                      + (Blend[Jdx].fPropn / fPropnSum) * Blend[Jdx].Breed.FParentage[Kdx].fPropn;
                        }
                    }
                }
            }

            if (sBreedName != "")                                                    // Construct a name for the new genotype 
                sName = sBreedName;
            else if (FParentage.Length == 1)
                sName = FParentage[0].sBaseBreed;
            else if (FParentage.Length > 1)
            {
                iDecPlaces = 0;
                for (Idx = 0; Idx <= FParentage.Length - 1; Idx++)
                {
                    if ((FParentage[Idx].fPropn > 0.0005) && (FParentage[Idx].fPropn <= 0.05))
                        iDecPlaces = 1;
                }

                sName = "";
                for (Idx = 0; Idx <= FParentage.Length - 1; Idx++)
                {
                    if (FParentage[Idx].fPropn > 0.0005)
                    {
                        if (sName != "")
                            sName = sName + ", ";
                        sName = sName + FParentage[Idx].sBaseBreed + " "
                                         + String.Format("{0:0." + new String('0', iDecPlaces) + "}", 100.0 * FParentage[Idx].fPropn) + "%";
                    }
                }
            }
        }

        /// <summary>
        /// Mix of two genotypes (as at mating)
        /// </summary>
        /// <param name="sBreedName"></param>
        /// <param name="damBreed"></param>
        /// <param name="sireBreed"></param>
        /// <param name="iGeneration"></param>
        /// <returns>The new object</returns>
        public static TAnimalParamSet CreateFactory(string sBreedName, TAnimalParamSet damBreed, TAnimalParamSet sireBreed, int iGeneration = 1)
        {
            TAnimalParamBlend[] aBlend = new TAnimalParamBlend[2];

            aBlend[0].Breed = damBreed;
            aBlend[0].fPropn = Math.Pow(0.5, iGeneration);
            aBlend[1].Breed = sireBreed;
            aBlend[1].fPropn = 1.0 - aBlend[0].fPropn;
            return CreateFactory(sBreedName, aBlend);
        }

        /// <summary>
        /// 
        /// </summary>
        override public void deriveParams()
        {
            MaxYoung = 1;
            while ((MaxYoung < 3) && (BirthWtScale[MaxYoung + 1] > 0.0))
                MaxYoung++;

            FPotFleeceWt = FBreedSRW * FleeceRatio;
            FUseDairyCurve = false;
            if (Animal == GrazType.AnimalType.Cattle)
                PeakMilk = IntakeC[11] * BreedSRW;

            if (GrazeC[20] == 0.0)
                GrazeC[20] = 11.5;
        }

        /// <summary>
        /// Returns TRUE i.f.f. all parameters other than the breed name are identical
        /// </summary>
        /// <param name="otherSet"></param>
        /// <returns></returns>
        public bool bFunctionallySame(TAnimalParamSet otherSet)
        {
            int iCount;
            TParameterDefinition Defn;
            string sTag;
            int iPrm;

            bool result = true;
            iCount = this.iParamCount();

            iPrm = 0;
            while ((iPrm < iCount) && result)
            {
                Defn = this.getParam(iPrm);
                if (Defn != null)
                {
                    sTag = Defn.sFullName;
                    if (this.bIsDefined(sTag))
                    {
                        switch (Defn.paramType)
                        {
                            case ptyText: result = (result && (this.sParam(sTag) == otherSet.sParam(sTag)));
                                break;
                            case ptyReal: result = (result && (this.fParam(sTag) == otherSet.fParam(sTag)));
                                break;
                            case ptyInt: result = (result && (this.iParam(sTag) == otherSet.iParam(sTag)));
                                break;
                            case ptyBool: result = (result && (this.bParam(sTag) == otherSet.bParam(sTag)));
                                break;
                        }
                    }
                    else
                        result = (result && !otherSet.bIsDefined(Defn.sFullName));
                }
                else
                    result = false;

                iPrm++;
            }
            return result;
        }

        /// <summary>
        /// Returns the parameter set corresponding to a given name.                 
        /// * sBreedName may actually be the name of a "breed group", i.e. a comma-   
        ///   separated list of functionally identical breeds. In this case the       
        ///   parameter set for the first member of the group is returned.            
        /// </summary>
        /// <param name="sBreedName"></param>
        /// <returns></returns>
        public TAnimalParamSet Match(string sBreedName)
        {
            if (sBreedName.IndexOf(',') >= 0)
                sBreedName = sBreedName.Remove(sBreedName.IndexOf(','), sBreedName.Length - sBreedName.IndexOf(','));

            return (TAnimalParamSet)getNode(sBreedName);
        }

        /// <summary>
        /// Returns the number of breeds of a given animal type
        /// </summary>
        /// <param name="aAnimal"></param>
        /// <returns></returns>
        public int iBreedCount(GrazType.AnimalType aAnimal)
        {
            TAnimalParamSet breedSet;
            int Idx;

            int result = 0;
            for (Idx = 0; Idx <= iLeafCount(true) - 1; Idx++)                                     // Current locale only                      
            {
                breedSet = (TAnimalParamSet)getLeaf(Idx, true);
                if (breedSet.Animal == aAnimal)
                    result++;
            }

            return result;
        }

        /// <summary>
        /// Iterates through breeds of a given animal type and returns the breed name
        /// </summary>
        /// <param name="aAnimal"></param>
        /// <param name="iBreed"></param>
        /// <returns></returns>
        public string sBreedName(GrazType.AnimalType aAnimal, int iBreed)
        {
            TAnimalParamSet breedSet;
            int iCount;
            int iFound;
            int Idx;

            iCount = iLeafCount(true);                                             // Current locale only                      
            iFound = -1;
            Idx = 0;
            breedSet = null;
            while ((Idx < iCount) && (iFound < iBreed))
            {
                breedSet = (TAnimalParamSet)getLeaf(Idx, true);
                if (breedSet.Animal == aAnimal)
                    iFound++;
                Idx++;
            }

            if (iFound == iBreed)
                return breedSet.sName;
            else
                return "";
        }

        /// <summary>
        /// Populates a string list with the names of "breed groups", i.e. sets of    
        /// parameter sets that are identical in all respects save their names.       
        /// </summary>
        /// <param name="aAnimal"></param>
        /// <param name="aList"></param>
        public void getBreedGroups(GrazType.AnimalType aAnimal, List<string> aList)
        {
            bool bSameFound;
            int Idx, Jdx;

            aList.Clear();                                                              // Start by forming a list of all breeds.   
            for (Idx = 0; Idx <= iBreedCount(aAnimal) - 1; Idx++)
                aList.Add(sBreedName(aAnimal, Idx));

            for (Idx = aList.Count - 1; Idx >= 1; Idx--)
            {
                bSameFound = false;
                for (Jdx = Idx - 1; Jdx >= 0; Jdx--)
                    if (!bSameFound)
                    {
                        bSameFound = Match(aList[Idx]).bFunctionallySame(Match(aList[Jdx]));
                        if (bSameFound)
                        {
                            aList[Jdx] = aList[Jdx] + ", " + aList[Idx];
                            aList.RemoveAt(Idx);
                        }
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int iParentageCount()
        {
            int result;

            if (FParentage != null)
                result = FParentage.Length;
            else
                result = 0;
            if (result == 0)
                result = 1;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Idx"></param>
        /// <returns></returns>
        public string sParentageBreed(int Idx)
        {
            if ((FParentage.Length == 0) && (Idx == 0))
                return sName;
            else
                return FParentage[Idx].sBaseBreed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Idx"></param>
        /// <returns></returns>
        public double fParentagePropn(int Idx)
        {
            if ((FParentage.Length == 0) && (Idx == 0))
                return 1.0;
            else
                return FParentage[Idx].fPropn;
        }
        /// <summary>
        /// Breed standard reference weight
        /// </summary>
        public double BreedSRW
        {
            get { return FBreedSRW; }
            set { setSRW(value); }
        }
        /// <summary>
        /// Potential fleece weight
        /// </summary>
        public double PotentialGFW
        {
            get { return FPotFleeceWt; }
            set { setPotGFW(value); }
        }
        /// <summary>
        /// Maximum fleece microns
        /// </summary>
        public double MaxMicrons
        {
            get { return MaxFleeceDiam; }
            set { MaxFleeceDiam = value; }
        }
        /// <summary>
        /// Fleece yield
        /// </summary>
        public double FleeceYield
        {
            get { return WoolC[3]; }
            set { WoolC[3] = value; }
        }
        /// <summary>
        /// Potential milk yield
        /// </summary>
        public double PotMilkYield
        {
            get { return PeakMilk; }
            set { setPeakMilk(value); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bWeaners"></param>
        /// <returns></returns>
        public double AnnualDeaths(bool bWeaners)
        {
            return getDeaths(bWeaners);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bWeaners"></param>
        /// <param name="AnnDeaths"></param>
        public void SetAnnualDeaths(bool bWeaners, double AnnDeaths)
        {
            setDeaths(bWeaners, AnnDeaths);
        }

        /// <summary>
        /// Conception values
        /// </summary>
        public double[] Conceptions
        {
            get { return getConceptions(); }
            set { setConceptions(value); }
        }
        /// <summary>
        /// Get gestation
        /// </summary>
        public int Gestation
        {
            get { return getGestation(); }
        }
        /// <summary>
        /// Standard reference weight
        /// </summary>
        /// <param name="Repro"></param>
        /// <returns></returns>
        public double fSexStdRefWt(GrazType.ReproType Repro)
        {
            if ((Repro == GrazType.ReproType.Castrated) || (Repro == GrazType.ReproType.Male))
                return SRWScalars[(int)Repro] * BreedSRW;
            else
                return BreedSRW;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fSRW"></param>
        /// <returns></returns>
        public double fSRWToPFW(double fSRW)
        {
            return FleeceRatio * fSRW;
        }
        /// <summary>
        /// Standard birth weight
        /// </summary>
        /// <param name="iNoYoung"></param>
        /// <returns></returns>
        public double StdBirthWt(int iNoYoung)
        {
            return BreedSRW * BirthWtScale[iNoYoung];
        }
        /// <summary>
        /// Flag that is set when PotMilkYield is assigned
        /// </summary>
        public bool bUseDairyCurve
        {
            get { return FUseDairyCurve; }                                  // Flag that is set when PotMilkYield is assigned
        }

        private const double SIGVAL = 5.88878;                              // 2*ln(0.95/0.05)                         
        private const double DAYSPERYR = 365.25;
        private const double NC = 2.5;                                      // 2.5 cycles joining is assumed            

        // Convert between condition scores and relative condition values   
        /// <summary>
        /// Condition score for condition = 1.0
        /// </summary>
        static public double[] BASESCORE = { 3.0, 4.0, 4.5 };                 
        /// <summary>
        /// Change in condition for unit CS change   
        /// </summary>
        static public double[] SCOREUNIT = { 0.15, 0.09, 0.08 };              
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CondScore"></param>
        /// <param name="System"></param>
        /// <returns></returns>
        static public double CondScore2Condition(double CondScore, TCond_System System = TCond_System.csSYSTEM1_5)
        {
            return 1.0 + (CondScore - BASESCORE[(int)System]) * SCOREUNIT[(int)System];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="System"></param>
        /// <returns></returns>
        static public double Condition2CondScore(double Condition, TCond_System System = TCond_System.csSYSTEM1_5)
        {
            return BASESCORE[(int)System] + (Condition - 1.0) / SCOREUNIT[(int)System];
        }
        /// <summary>
        /// Default fleece weight as a function of age, sex and time since shearing     
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="iAgeDays"></param>
        /// <param name="Repr"></param>
        /// <param name="iFleeceDays"></param>
        /// <returns></returns>
        static public double fDefaultFleece(TAnimalParamSet Params,
                                     int iAgeDays,
                                     GrazType.ReproType Repr,
                                     int iFleeceDays)
        {
            double Result;
            double fMeanAgeFactor;

            iFleeceDays = Math.Min(iFleeceDays, iAgeDays);

            if ((Params.Animal == GrazType.AnimalType.Sheep) && (iFleeceDays > 0))
            {
                fMeanAgeFactor = 1.0 - (1.0 - Params.WoolC[5])
                                        * (Math.Exp(-Params.WoolC[12] * (iAgeDays - iFleeceDays)) - Math.Exp(-Params.WoolC[12] * iAgeDays))
                                        / (Params.WoolC[12] * iFleeceDays);
                Result = Params.FleeceRatio * Params.fSexStdRefWt(Repr) * fMeanAgeFactor * iFleeceDays / 365.0;
            }
            else
                Result = 0.0;
            return Result;
        }

        /// <summary>
        /// Default fibre diameter as a function of age, sex, time since shearing and fleece weight                                                             
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="iAgeDays"></param>
        /// <param name="Repr"></param>
        /// <param name="iFleeceDays"></param>
        /// <param name="fGFW"></param>
        /// <returns></returns>
        static public double fDefaultMicron(TAnimalParamSet Params, int iAgeDays, GrazType.ReproType Repr, int iFleeceDays, double fGFW)
        {
            double fPotFleece;

            if ((iFleeceDays > 0) && (fGFW > 0.0))
            {
                fPotFleece = fDefaultFleece(Params, iAgeDays, Repr, iFleeceDays);
                return Params.MaxMicrons * Math.Pow(fGFW / fPotFleece, Params.WoolC[13]);
            }
            else
                return Params.MaxMicrons;
        }
    }
}