using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using System.Xml.Serialization;

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
    public class WeatherSens : Model, ISensitivity
    {

        #region ******* Links. *******



        #endregion

        #region ******* Public field. *******

        /// <summary></summary>
        [Description("Sensitivity analysis of weather input")]
        public string[] WeatherInput { get; set; }

        /// <summary>Input offset</summary>
        [Description("Model offset")]
        public double[] Offset { get; set; }

        /// <summary>InputOffsetOption</summary>
        [Description("Input offset option")]
        public int[] OffsetOption { get; set; }

        #endregion

        #region ******* Private field. *******



        #endregion


        #region ******* EventHandlers. *******



        #endregion

    }
}