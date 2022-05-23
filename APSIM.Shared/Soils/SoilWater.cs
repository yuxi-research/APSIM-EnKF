// -----------------------------------------------------------------------
// <copyright file="SoilWater.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>A specification of soil water model constants and parameters.</summary>
    [Serializable]
    public class SoilWater
    {
        /// <summary>Gets or sets the summer cona.</summary>
        [Description("Summer Cona")]
        public double SummerCona { get; set; }

        /// <summary>Gets or sets the summer u.</summary>
        [Description("Summer U")]
        public double SummerU { get; set; }

        /// <summary>Gets or sets the summer date.</summary>
        [Description("Summer Date")]
        public string SummerDate { get; set; }

        /// <summary>Gets or sets the winter cona.</summary>
        [Description("Winter Cona")]
        public double WinterCona { get; set; }

        /// <summary>Gets or sets the winter u.</summary>
        [Description("Winter U")]
        public double WinterU { get; set; }

        /// <summary>Gets or sets the winter date.</summary>
        [Description("Winter Date")]
        public string WinterDate { get; set; }

        /// <summary>Gets or sets the diffus constant.</summary>
        [Description("Diffusivity Constant")]
        public double DiffusConst { get; set; }

        /// <summary>Gets or sets the diffus slope.</summary>
        [Description("Diffusivity Slope")]
        public double DiffusSlope { get; set; }

        /// <summary>Gets or sets the salb.</summary>
        [Description("Soil albedo")]
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

        /// <summary>Gets or sets the slope.</summary>
        public double Slope { get; set; }

        /// <summary>Gets or sets the width of the discharge.</summary>
        [Description("Discharge width")]
        public double DischargeWidth { get; set; }

        /// <summary>Gets or sets the catchment area.</summary>
        [Description("Catchment area")]
        public double CatchmentArea { get; set; }

        /// <summary>Gets or sets the maximum pond.</summary>
        [Description("Maximum pond")]
        public double MaxPond { get; set; }

        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the swcon.</summary>
        [Units("0-1")]
        public double[] SWCON { get; set; }

        /// <summary>Gets or sets the mwcon.</summary>
        [Units("0-1")]
        public double[] MWCON { get; set; }

        /// <summary>Gets or sets the klat.</summary>
        [Units("mm/day")]
        public double[] KLAT { get; set; }
    }
}
