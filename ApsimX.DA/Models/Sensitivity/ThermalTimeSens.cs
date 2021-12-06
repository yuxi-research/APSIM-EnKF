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
    public class ThermalTimeSens : Model, ISensitivity
    {

        #region ******* Links. *******

        //[Link(IsOptional = true)]
        //Leaf1 Leaf = null;

        #endregion

        #region ******* Public field. *******

        /// <summary></summary>
        [Description("Do thermal time sensitivity?")]
        public bool DoTTSens { get; set; }

        /// <summary></summary>
        [Description("ThermalTime offset")]
        public double TTOffset { get; set; }

        /// <summary></summary>
        [Description("Change TT on date")]
        public DateTime Date { get; set; }

        #endregion

        #region ******* Private field. *******



        #endregion

        #region ******* EventHandlers. *******


        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        private void DoSensitivity()
        {

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
