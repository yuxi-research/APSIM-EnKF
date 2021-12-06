// -----------------------------------------------------------------------
// <copyright file="SoilTemperature2.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{

    /// <summary>Soil temperature 2 parameters.</summary>
    [Serializable]
    public class SoilTemperature2
    {
        /// <summary>Gets or sets the maximum t time default.</summary>
        [Units("hours")]
        public double MaxTTimeDefault { get; set; }

        /// <summary>Gets or sets the boundary layer conductance source.</summary>
        [Description("Boundary layer conductance source")]
        [Units("(calc/constant)")]
        public string BoundaryLayerConductanceSource { get; set; }

        /// <summary>Gets or sets the boundary layer conductance.</summary>
        [Description("Boundary layer conductance")]
        [Units("(W/m2/K)")]
        public double BoundaryLayerConductance { get; set; }

        /// <summary>Gets or sets the boundary layer conductance iterations.</summary>
        [Description("Number of iterations to calc boundary layer conductance (0-10)")]
        public int BoundaryLayerConductanceIterations { get; set; }

        /// <summary>Gets or sets the net radiation source.</summary>
        [Description("Net radiation source (calc/eos)")]
        public string NetRadiationSource { get; set; }

        /// <summary>Gets or sets the default wind speed.</summary>
        [Description("Default wind speed")]
        [Units("m/s")]
        public double DefaultWindSpeed { get; set; }

        /// <summary>Gets or sets the default altitude.</summary>
        [Description("Default altitude (m) 275m (700 ft) is approx 980 hPa")]
        [Units("m")]
        public double DefaultAltitude { get; set; }

        /// <summary>Gets or sets the default height of the instrument.</summary>
        [Description("Default instrument height for wind and temperature")]
        [Units("m")]
        public double DefaultInstrumentHeight { get; set; }

        /// <summary>Gets or sets the height of the bare soil.</summary>
        [Description("Height of bare soil")]
        [Units("mm")]
        public double BareSoilHeight { get; set; }

        /// <summary>Gets or sets the note.</summary>
        [Description("Note")]
        public string Note { get; set; }
    }

}
