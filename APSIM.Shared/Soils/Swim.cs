// -----------------------------------------------------------------------
// <copyright file="Swim.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>SWIM parameters.</summary>
    [Serializable]
    public class Swim
    {
        /// <summary>Gets or sets the salb.</summary>
        [Description("Bare soil albedo")]
        public double Salb { get; set; }

        /// <summary>Gets or sets the c n2 bare.</summary>
        [Description("Bare soil runoff curve number")]
        public double CN2Bare { get; set; }

        /// <summary>Gets or sets the cn red.</summary>
        [Description("Max. reduction in curve number due to cover")]
        public double CNRed { get; set; }

        /// <summary>Gets or sets the cn cov.</summary>
        [Description("Cover for max curve number reduction")]
        public double CNCov { get; set; }

        /// <summary>Gets or sets the k dul.</summary>
        [Description("Hydraulic conductivity at DUL (mm/d)")]
        public double KDul { get; set; }

        /// <summary>Gets or sets the psi dul.</summary>
        [Description("Matric Potential at DUL (cm)")]
        public double PSIDul { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Swim"/> is vc.</summary>
        [Description("Vapour Conductivity Calculations?")]
        public bool VC { get; set; }

        /// <summary>Gets or sets the d tmin.</summary>
        [Description("Minimum Timestep (min)")]
        public double DTmin { get; set; }

        /// <summary>Gets or sets the d tmax.</summary>
        [Description("Maximum Timestep (min)")]
        public double DTmax { get; set; }

        /// <summary>Gets or sets the maximum water increment.</summary>
        [Description("Maximum water increment (mm)")]
        public double MaxWaterIncrement { get; set; }

        /// <summary>Gets or sets the space weighting factor.</summary>
        [Description("Space weighting factor")]
        public double SpaceWeightingFactor { get; set; }

        /// <summary>Gets or sets the solute space weighting factor.</summary>
        [Description("Solute space weighting factor")]
        public double SoluteSpaceWeightingFactor { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Swim"/> is diagnostics.</summary>
        [Description("Diagnostic Information?")]
        public bool Diagnostics { get; set; }

        /// <summary>Gets or sets the swim solute parameters.</summary>
        public SwimSoluteParameters SwimSoluteParameters { get; set; }

        /// <summary>Gets or sets the swim water table.</summary>
        public SwimWaterTable SwimWaterTable { get; set; }

        /// <summary>Gets or sets the swim subsurface drain.</summary>
        public SwimSubsurfaceDrain SwimSubsurfaceDrain { get; set; }
    }

}
