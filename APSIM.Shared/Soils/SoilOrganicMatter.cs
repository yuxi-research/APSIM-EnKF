// -----------------------------------------------------------------------
// <copyright file="SoilOrganicMatter.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>A soil organic matter class.</summary>
    [Serializable]
    public class SoilOrganicMatter
    {
        /// <summary>Gets or sets the root cn.</summary>
        [Description("Root C:N ratio")]
        public double RootCN { get; set; }

        /// <summary>Gets or sets the root wt.</summary>
        [Description("Root Weight (kg/ha)")]
        public double RootWt { get; set; }
        
        /// <summary>Gets or sets the soil cn.</summary>
        [Description("Soil C:N ratio")]
        public double SoilCN { get; set; }
        
        /// <summary>Gets or sets the enr a coeff.</summary>
        [Description("Erosion enrichment coefficient A")]
        public double EnrACoeff { get; set; }
        
        /// <summary>Gets or sets the enr b coeff.</summary>
        [Description("Erosion enrichment coefficient B")]
        public double EnrBCoeff { get; set; }
        
        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }
        
        /// <summary>Gets or sets the oc.</summary>
        public double[] OC { get; set; }
        
        /// <summary>Gets or sets the oc metadata.</summary>
        public string[] OCMetadata { get; set; }
        
        /// <summary>Gets or sets the f biom.</summary>
        [Units("0-1")]
        public double[] FBiom { get; set; }
        
        /// <summary>Gets or sets the f inert.</summary>
        [Units("0-1")]
        public double[] FInert { get; set; }

        /// <summary>Allowable units for OC</summary>
        public enum OCUnitsEnum 
        {
            /// <summary>total (%)</summary>
            Total,

            /// <summary>walkley black (%)</summary>
            WalkleyBlack 
        }

        /// <summary>Gets or sets the oc units.</summary>
        public OCUnitsEnum OCUnits { get; set; }
    }
}
