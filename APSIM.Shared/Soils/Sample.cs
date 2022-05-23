// -----------------------------------------------------------------------
// <copyright file="Sample.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using System;
    using System.Xml.Serialization;

    /// <summary>Represents a soil sample.</summary>
    [Serializable]
    public class Sample
    {
        /// <summary>Gets or sets the name.</summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the sample date.</summary>
        public string Date { get; set; }

        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the NO3.</summary>
        public double[] NO3 { get; set; }

        /// <summary>Gets or sets the NH4.</summary>
        public double[] NH4 { get; set; }

        /// <summary>Gets or sets the soil water</summary>
        public double[] SW { get; set; }

        /// <summary>Gets or sets the organic carbon</summary>
        public double[] OC { get; set; }

        /// <summary>Gets or sets the EC.</summary>
        [Units("1:5 dS/m")]
        public double[] EC { get; set; }

        /// <summary>Gets or sets the CL.</summary>
        [Units("mg/kg")]
        public double[] CL { get; set; }

        /// <summary>Gets or sets the ESP.</summary>
        [Units("%")]
        public double[] ESP { get; set; }

        /// <summary>Gets or sets the PH.</summary>
        public double[] PH { get; set; }
       
        /// <summary>Gets or sets the NO3 units.</summary>
        public Nitrogen.NUnitsEnum NO3Units { get; set; }
                 
        // Support for NH4 units.
        /// <summary>Gets or sets the NH4 units.</summary>
        public Nitrogen.NUnitsEnum NH4Units { get; set; }
         
        /// <summary>Valid units for soil water</summary>
        public enum SWUnitsEnum 
        {
            /// <summary>volumetric (mm/mm)</summary>
            Volumetric,

            /// <summary>gravimetric (mm/mm)</summary>
            Gravimetric,

            /// <summary>Total water (mm)</summary>
            mm 
        }

        /// <summary>Gets or sets the SW units.</summary>
        public SWUnitsEnum SWUnits { get; set; }

        /// <summary>Gets or sets the OC units.</summary>
        public SoilOrganicMatter.OCUnitsEnum OCUnits { get; set; }

        /// <summary>Gets or sets the PH units.</summary>
        public Analysis.PHUnitsEnum PHUnits { get; set; }
    }
}
