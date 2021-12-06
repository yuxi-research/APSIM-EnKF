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
    public class CultivarSens : Model, ISensitivity
    {

        #region ******* Links. *******


        #endregion

        #region ******* Public field. *******

        /// <summary></summary>
        [Description("Cultivar commands")]
        public string[] Commands { get; set; }

        #endregion

        #region ******* Private field. *******



        #endregion


        #region ******* EventHandlers. *******

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("DoCultivarParaSens")]
        private void OnDoCultivarParaSens(object sender, EventArgs e)
        {
            DoSensitivity();
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public void DoSensitivity()
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