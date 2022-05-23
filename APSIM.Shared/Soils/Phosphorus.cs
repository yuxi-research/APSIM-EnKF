// -----------------------------------------------------------------------
// <copyright file="Phosphorus.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Phosphorus model.
    /// </summary>
    [Serializable]
    public class Phosphorus
    {
        /// <summary>Gets or sets the name.</summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the root cp.</summary>
        [Description("Root C:P ratio")]
        public double RootCP { get; set; }

        /// <summary>Gets or sets the rate dissol rock.</summary>
        [Description("Rate disolved rock")]
        public double RateDissolRock { get; set; }

        /// <summary>Gets or sets the rate loss avail.</summary>
        [Description("Rate loss available")]
        public double RateLossAvail { get; set; }

        /// <summary>Gets or sets the sorption coeff.</summary>
        [Description("Sorption coefficient")]
        public double SorptionCoeff { get; set; }

        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the labile p.</summary>
        [Units("mg/kg")]
        public double[] LabileP { get; set; }

        /// <summary>Gets or sets the banded p.</summary>
        [Units("kg/ha")]
        public double[] BandedP { get; set; }

        /// <summary>Gets or sets the rock p.</summary>
        [Units("kg/ha")]
        public double[] RockP { get; set; }

        /// <summary>Gets or sets the sorption.</summary>
        [Units("-")]
        public double[] Sorption { get; set; }
    }


}
