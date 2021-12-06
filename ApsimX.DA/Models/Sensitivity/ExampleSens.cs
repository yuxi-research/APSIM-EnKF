using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using System.Xml.Serialization;
using Models;

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
    public class ExampleSens : Model, ISensitivity
    {

        #region ******* Links. *******


        #endregion

        #region ******* Public field. *******

        /// <summary></summary>
        [Description("Sensitivity analysis of cultivar parameters")]
        public string[] CultivarPara { get; set; }

        /// <summary>cultivar offset</summary>
        [Description("Cultivar offset")]
        public double[] Offset { get; set; }

        /// <summary>InputOffsetOption</summary>
        [Description("Input offset option")]
        public int[] OffsetOption { get; set; }

        #endregion

        #region ******* Private field. *******



        #endregion


        #region ******* EventHandlers. *******

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("CultivarParaSensitivity")]
        private void OnCultivarSensitivity(object sender, EventArgs e)
        {
            OnCultivarSensitivity();
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public void OnCultivarSensitivity()
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