using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using System.Xml.Serialization;
using Models;
using Models.Soils;
using Models.PMF.OldPlant;

namespace Models.Sensitivity
{
    /// <summary>
    /// Sensitivity analysis for input data.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulations))]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(ISensitivity))]
    public class StateSens : Model, ISensitivity
    {

        #region ******* Links. *******

        [Link(IsOptional = true)]
        SoilWater SoilWater = null;

        [Link(IsOptional = true)]
        CERESSoilTemperature SoilTemperature = null;

        [Link(IsOptional = true)]
        SoilNitrogen SoilNitrogen = null;

        //[Link(IsOptional = true)]
        //Grain Grain = null;

        [Link(IsOptional = true)]
        Leaf1 Leaf = null;

        [Link(IsOptional = true)]
        Root1 Root = null;

        [Link(IsOptional = true)]
        Stem1 Stem = null;

        //[Link(IsOptional = true)]
        //Pod Pod = null;


        #endregion

        #region ******* Public field. *******

        /// <summary></summary>
        [Description("Sensitivity analysis of state variables")]
        public string[] StateNames { get; set; }

        /// <summary>cultivar offset</summary>
        [Description("Cultivar offset")]
        public double[] Offset { get; set; }

        /// <summary>InputOffsetOption</summary>
        [Description("Input offset option")]
        public int[] OffsetOption { get; set; }

        /// <summary></summary>
        [Description("Change state variables on date")]
        public DateTime Date { get; set; }

        #endregion

        #region ******* Private field. *******



        #endregion


        #region ******* EventHandlers. *******

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("DoStateSens")]
        private void OnStateSens(object sender, EventArgs e)
        {
            DoSensitivity();
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        private void DoSensitivity()
        {
            for (int i = 0; i < StateNames.Length; i++)
            {
                // Note: Array is a reference type.
                double[] newSW = SoilWater.SW;
                double[] newST = SoilTemperature.Value;
                double[] newSNO3 = SoilNitrogen.NO3;
                double[] newSNH4 = SoilNitrogen.NH4;

                switch (StateNames[i])
                {
                    // Plant
                    case "LAI":
                        if (Leaf.LAI > 0)
                        {
                            Leaf.LAI = Leaf.LAI * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                            Leaf.LAI = ConstrainToBound(Leaf.LAI, 0, 100);
                        }
                        break;
                    case "Height":
                        Stem.Height = Stem.Height * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        Stem.Height = ConstrainToBound(Stem.Height, 0, 100);

                        break;

                    case "RootDepth":
                        Root.RootDepth = Root.RootDepth * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        Root.RootDepth = ConstrainToBound(Root.RootDepth, 0, 100);
                        break;

                    case "Width":
                        Stem.Width = Stem.Width * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        Stem.Width = ConstrainToBound(Stem.Width, 0, 100);
                        break;

                    // Soil.
                    case "SW1":
                        newSW[0] = SoilWater.SW[0] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "SW2":
                        newSW[1] = SoilWater.SW[1] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "SW3":
                        newSW[2] = SoilWater.SW[2] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "SW4":
                        newSW[3] = SoilWater.SW[3] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "SW5":
                        newSW[4] = SoilWater.SW[4] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "SW6":
                        newSW[5] = SoilWater.SW[5] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "SW7":
                        newSW[6] = SoilWater.SW[6] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "ST1":
                        newST[0] = SoilTemperature.Value[0] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "ST2":
                        newST[1] = SoilTemperature.Value[1] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "ST3":
                        newST[2] = SoilTemperature.Value[2] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "ST4":
                        newST[3] = SoilTemperature.Value[3] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "ST5":
                        newST[4] = SoilTemperature.Value[4] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "ST6":
                        newST[5] = SoilTemperature.Value[5] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    case "ST7":
                        newST[6] = SoilTemperature.Value[6] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_1":
                        newSNO3[0] = SoilNitrogen.NO3[0] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_2":
                        newSNO3[1] = SoilNitrogen.NO3[1] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_3":
                        newSNO3[2] = SoilNitrogen.NO3[2] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_4":
                        newSNO3[3] = SoilNitrogen.NO3[3] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_5":
                        newSNO3[4] = SoilNitrogen.NO3[4] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_6":
                        newSNO3[5] = SoilNitrogen.NO3[5] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNO3_7":
                        newSNO3[6] = SoilNitrogen.NO3[6] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_1":
                        newSNH4[0] = SoilNitrogen.NH4[0] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_2":
                        newSNH4[1] = SoilNitrogen.NH4[1] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_3":
                        newSNH4[2] = SoilNitrogen.NH4[2] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_4":
                        newSNH4[3] = SoilNitrogen.NH4[3] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_5":
                        newSNH4[4] = SoilNitrogen.NH4[4] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_6":
                        newSNH4[5] = SoilNitrogen.NH4[5] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;

                    case "SoilNH4_7":
                        newSNH4[6] = SoilNitrogen.NH4[6] * (1 + OffsetOption[i] * Offset[i]) + Offset[i] * (1 - OffsetOption[i]);
                        break;
                    default:
                        throw new Exception("Wrong state name!");
                }

                //Check: Need to assign?

                SoilWater.SW = newSW;
                SoilTemperature.Value = newST;
                SoilNitrogen.NO3 = newSNO3;
                SoilNitrogen.NH4 = newSNH4;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        private double ConstrainToBound(double value, double lower, double upper)
        {
            if (value < lower)
                value = lower;
            else if (value > upper)
                value = upper;
            return value;
        }
        #endregion
    }

}
