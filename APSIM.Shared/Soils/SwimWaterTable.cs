// -----------------------------------------------------------------------
// <copyright file="SwimSubsurfaceDrain.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>SWIM water table parameters.</summary>
    [Serializable]
    public class SwimWaterTable
    {
        /// <summary>Gets or sets the water table depth.</summary>
        [Description("Depth of Water Table (mm)")]
        public double WaterTableDepth { get; set; }
    }

}
