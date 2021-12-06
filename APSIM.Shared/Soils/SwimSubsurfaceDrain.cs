// -----------------------------------------------------------------------
// <copyright file="SwimSubsurfaceDrain.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>SWIM subsurface drain parameters.</summary>
    [Serializable]
    public class SwimSubsurfaceDrain
    {
        /// <summary>Gets or sets the drain depth.</summary>
        [Description("Depth of subsurface drain (mm)")]
        public double DrainDepth { get; set; }

        /// <summary>Gets or sets the drain spacing.</summary>
        [Description("Distance between subsurface drains (mm)")]
        public double DrainSpacing { get; set; }

        /// <summary>Gets or sets the drain radius.</summary>
        [Description("Radius of each subsurface drain (mm)")]
        public double DrainRadius { get; set; }

        /// <summary>Gets or sets the klat.</summary>
        [Description("Lateral saturated soil water conductivity (mm/d)")]
        public double Klat { get; set; }

        /// <summary>Gets or sets the imperm depth.</summary>
        [Description("Depth to impermeable soil (mm)")]
        public double ImpermDepth { get; set; }
    }
}
