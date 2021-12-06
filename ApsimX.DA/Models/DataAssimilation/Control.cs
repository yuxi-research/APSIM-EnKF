using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
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
using APSIM.Shared.Utilities;
using Models.PMF.OldPlant;

namespace Models.DataAssimilation
{
    /// <summary>
    /// A model to determine key data assimilation parameters.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(IDataAssimilation))]

    public class Control : Model
    {
        /// <summary> Data assimilation option. </summary>
        [Description("DA Option: OpenLoop, DirectInsertion, or EnKF?")]
        public string DAOption { get; set; }
        /// <summary> Ensemble Size. </summary>
        [Description("Ensemble size")]
        public int EnsembleSize { get; set; }
        /// <summary> Ensemble Size. </summary>
        [Description("Add model error?")]
        public bool AddModelError { get; set; }
        /// <summary> Writting results to SQLite? </summary>
        //[Description("Write results to SQLite?")]
        public bool WriteSQL { get { return true; } }  //false for debug
        /// <summary>Do phenology on a known date?</summary>
        [Description("Fix phenology?")]
        public bool FixPhenology { get; set; }
        //public bool FixPhenology { get { return false; } }
        /// <summary>
        /// Set the value of ensemble inflator.
        /// </summary>
        [Description("Ensemble inflator")]
        public double EnsembleInflator { get; set; }

        /// <summary>Known date of phenology in Day after Start.</summary>
        [Description("DayafterStart of known phenology.")]
        // 0-Sowng, 1-Germination, 2-Emergence, 4-EndOfJuvenile, 5-FlowerInitiation, 6-Flowering
        // 7-StartOfGrainFill, 8-EndOfGrainFill, 9-Maturity, 10-ReadyToHarvest
        // Sowing is fixed, and can be set to 0.
        public double[] DaSPheno { get; set; }
        /// <summary>Known date of phenology in DoY. Not in use as replaced by DaSPheno.</summary>
        //[Description("DayofYear of known phenology.")]
        // 0-Sowng, 1-Germination, 2-Emergence, 4-EndOfJuvenile, 5-FlowerInitiation, 6-Flowering
        // 7-StartOfGrainFill, 8-EndOfGrainFill, 9-Maturity, 10-ReadyToHarvest
        // Sowing is fixed, and can be set to 0.
        public double[] DoYPheno { get; set; }
    }
}
