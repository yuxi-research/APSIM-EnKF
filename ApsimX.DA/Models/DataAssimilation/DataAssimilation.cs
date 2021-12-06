using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
using Models.PMF;
using System.Reflection;
using System.Collections;
using Models.PMF.Functions;
using Models.PMF.Organs;
using Models.PMF.Phen;
using System.Xml.Serialization;
using System.IO;
using Models.Soils;
using Models.Soils.Arbitrator;
using Models.PMF.Interfaces;
using Models.Interfaces;
using APSIM.Shared.Utilities;
using Models.PMF.OldPlant;
using Models.SurfaceOM;
using Models.DataAssimilation.DataType;

namespace Models.DataAssimilation
{
    /// <summary>
    /// An example.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(IDataAssimilation))]
    [ValidParent(ParentType = typeof(Control))]

    public class DataAssimilation : Model, IDataAssimilation
    {
        #region ******* Links. *******


        #endregion

        #region ******* Public field. *******

        /// <summary>True if data assmilation is activated.</summary>
        public bool DAActivated { get { return true; } }

        #endregion

        #region ******* Private field. *******



        #endregion

        #region ******* EventHandlers. *******


        #endregion

        #region ******* Methods. *******



        #endregion

    }
}
