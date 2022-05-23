// -----------------------------------------------------------------------
// <copyright file="Nitrogen.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>Represents the initial nitrogen state of a soil.</summary>
    [Serializable]
    public class Nitrogen
    {
        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the NO3.</summary>
        public double[] NO3 { get; set; }

        /// <summary>Gets or sets the NH4.</summary>
        public double[] NH4 { get; set; }

        /// <summary>Valid units for NO3</summary>
        public enum NUnitsEnum 
        {
            /// <summary>PPM units</summary>
            ppm,

            /// <summary>kgha units.</summary>
            kgha 
        }

        /// <summary>Gets or sets the NO3 units.</summary>
        public NUnitsEnum NO3Units { get; set; }
                 
        /// <summary>Gets or sets the NH4 units.</summary>
        public NUnitsEnum NH4Units { get; set; }
    }
}
