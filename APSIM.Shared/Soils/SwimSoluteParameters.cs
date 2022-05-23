// -----------------------------------------------------------------------
// <copyright file="SwimSoluteParameters.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>SWIM solute parameters.</summary>
    [Serializable]
    public class SwimSoluteParameters
    {
        /// <summary>Gets or sets the dis.</summary>
        [Description("Dispersivity - dis ((cm^2/h)/(cm/h)^p)")]
        public double Dis { get; set; }

        /// <summary>Gets or sets the disp.</summary>
        [Description("Dispersivity Power - disp")]
        public double Disp { get; set; }

        /// <summary>Gets or sets a.</summary>
        [Description("Tortuosity Constant - a")]
        public double A { get; set; }

        /// <summary>Gets or sets the DTHC.</summary>
        [Description("Tortuoisty Offset - dthc")]
        public double DTHC { get; set; }

        /// <summary>Gets or sets the DTHP.</summary>
        [Description("Tortuoisty Power - dthp")]
        public double DTHP { get; set; }

        /// <summary>Gets or sets the water table cl.</summary>
        [Description("Water Table Cl Concentration (ppm)")]
        public double WaterTableCl { get; set; }

        /// <summary>Gets or sets the water table n o3.</summary>
        [Description("Water Table NO3 Concentration (ppm)")]
        public double WaterTableNO3 { get; set; }

        /// <summary>Gets or sets the water table n h4.</summary>
        [Description("Water Table NH4 Concentration (ppm)")]
        public double WaterTableNH4 { get; set; }

        /// <summary>Gets or sets the water table urea.</summary>
        [Description("Water Table Urea Concentration (ppm)")]
        public double WaterTableUrea { get; set; }

        /// <summary>Gets or sets the water table tracer.</summary>
        [Description("Water Table Tracer (ppm)")]
        public double WaterTableTracer { get; set; }

        /// <summary>Gets or sets the water table mineralisation inhibitor.</summary>
        [Description("Water Table Mineralisation Inhibitor (ppm)")]
        public double WaterTableMineralisationInhibitor { get; set; }

        /// <summary>Gets or sets the water table urease inhibitor.</summary>
        [Description("Water Table Urease Inhibitor (ppm)")]
        public double WaterTableUreaseInhibitor { get; set; }

        /// <summary>Gets or sets the water table nitrification inhibitor.</summary>
        [Description("Water Table Nitrification Inhibitor (ppm)")]
        public double WaterTableNitrificationInhibitor { get; set; }

        /// <summary>Gets or sets the water table denitrification inhibitor.</summary>
        [Description("Water Table Denitrification Inhibitor (ppm)")]
        public double WaterTableDenitrificationInhibitor { get; set; }

        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the n o3 exco.</summary>
        public double[] NO3Exco { get; set; }

        /// <summary>Gets or sets the n o3 fip.</summary>
        public double[] NO3FIP { get; set; }

        /// <summary>Gets or sets the n h4 exco.</summary>
        public double[] NH4Exco { get; set; }

        /// <summary>Gets or sets the n h4 fip.</summary>
        public double[] NH4FIP { get; set; }

        /// <summary>Gets or sets the urea exco.</summary>
        public double[] UreaExco { get; set; }

        /// <summary>Gets or sets the urea fip.</summary>
        public double[] UreaFIP { get; set; }

        /// <summary>Gets or sets the cl exco.</summary>
        public double[] ClExco { get; set; }

        /// <summary>Gets or sets the cl fip.</summary>
        public double[] ClFIP { get; set; }

        /// <summary>Gets or sets the tracer exco.</summary>
        public double[] TracerExco { get; set; }

        /// <summary>Gets or sets the tracer fip.</summary>
        public double[] TracerFIP { get; set; }

        /// <summary>Gets or sets the mineralisation inhibitor exco.</summary>
        public double[] MineralisationInhibitorExco { get; set; }

        /// <summary>Gets or sets the mineralisation inhibitor fip.</summary>
        public double[] MineralisationInhibitorFIP { get; set; }

        /// <summary>Gets or sets the urease inhibitor exco.</summary>
        public double[] UreaseInhibitorExco { get; set; }

        /// <summary>Gets or sets the urease inhibitor fip.</summary>
        public double[] UreaseInhibitorFIP { get; set; }

        /// <summary>Gets or sets the nitrification inhibitor exco.</summary>
        public double[] NitrificationInhibitorExco { get; set; }

        /// <summary>Gets or sets the nitrification inhibitor fip.</summary>
        public double[] NitrificationInhibitorFIP { get; set; }

        /// <summary>Gets or sets the denitrification inhibitor exco.</summary>
        public double[] DenitrificationInhibitorExco { get; set; }

        /// <summary>Gets or sets the denitrification inhibitor fip.</summary>
        public double[] DenitrificationInhibitorFIP { get; set; }
    }

}
