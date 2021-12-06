// -----------------------------------------------------------------------
// <copyright file="SoilTemperature.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>Soil temperature parameters.</summary>
    [Serializable]
    public class SoilTemperature
    {
        /// <summary>Gets or sets the boundary layer conductance.</summary>
        [Description("Boundary layer conductance")]
        [Units("(W/m2/K)")]
        public double BoundaryLayerConductance { get; set; }

        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the initial soil temperature.</summary>
        [Description("Initial soil temperature")]
        [Units("oC")]
        public double[] InitialSoilTemperature { get; set; }
    }

}


      