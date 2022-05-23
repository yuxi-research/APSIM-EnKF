// -----------------------------------------------------------------------
// <copyright file="Water.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>A water specification for a soil.</summary>
    [Serializable]
    public class Water
    {
        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the bd.</summary>
        [Units("g/cc")]
        public double[] BD { get; set; }

        /// <summary>Gets or sets the air dry.</summary>
        [Units("mm/mm")]
        public double[] AirDry { get; set; }

        /// <summary>Gets or sets the l L15.</summary>
        [Units("mm/mm")]
        public double[] LL15 { get; set; }

        /// <summary>Gets or sets the dul.</summary>
        [Units("mm/mm")]
        public double[] DUL { get; set; }

        /// <summary>Gets or sets the sat.</summary>
        [Units("mm/mm")]
        public double[] SAT { get; set; }

        /// <summary>Gets or sets the initial soil water (mm/mm).</summary>
        [Units("mm/mm")]
        public double[] SW { get; set; }

        /// <summary>Gets or sets the ks.</summary>
        [Units("mm/day")]
        public double[] KS { get; set; }

        /// <summary>Gets or sets the bd metadata.</summary>
        public string[] BDMetadata { get; set; }

        /// <summary>Gets or sets the air dry metadata.</summary>
        public string[] AirDryMetadata { get; set; }

        /// <summary>Gets or sets the l L15 metadata.</summary>
        public string[] LL15Metadata { get; set; }

        /// <summary>Gets or sets the dul metadata.</summary>
        public string[] DULMetadata { get; set; }

        /// <summary>Gets or sets the sat metadata.</summary>
        public string[] SATMetadata { get; set; }

        /// <summary>Gets or sets the ks metadata.</summary>
        public string[] KSMetadata { get; set; }

        /// <summary>Gets or sets the crops.</summary>
        [XmlElement("SoilCrop")]
        public List<SoilCrop> Crops { get; set; }
    }
}
