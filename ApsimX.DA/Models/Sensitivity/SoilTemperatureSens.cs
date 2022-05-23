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
    public class SoilTemperatureSens : Model, ISensitivity
    {

        #region ******* Links. *******
        [Link(IsOptional = true)]
        CERESSoilTemperature SoilTemperature = null;

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
        [EventSubscribe("DoSoilTempSens")]
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
                double[] newST = SoilTemperature.Value;

                switch (StateNames[i])
                {
                    // Soil.
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
                    default:
                        throw new Exception("Wrong state name!");
                }
                SoilTemperature.Value = newST;
            }
        }

        #endregion
    }

}
