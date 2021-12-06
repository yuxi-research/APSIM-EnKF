// -----------------------------------------------------------------------
// <copyright file="Soil.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>The soil class encapsulates a soil characterisation and 0 or more soil samples.</summary>
    [Serializable]
    public class Soil
    {
        /// <summary>Gets or sets the name.</summary>
        [UIIgnore]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the record number.</summary>
        [Description("Record number")]
        public int RecordNumber { get; set; }

        /// <summary>Gets or sets the asc order.</summary>
        [Description("Australian Soil Classification Order")]
        public string ASCOrder { get; set; }

        /// <summary>Gets or sets the asc sub order.</summary>
        [Description("Australian Soil Classification Sub-Order")]
        public string ASCSubOrder { get; set; }

        /// <summary>Gets or sets the type of the soil.</summary>
        [Description("Soil texture or other descriptor")]
        public string SoilType { get; set; }

        /// <summary>Gets or sets the name of the local.</summary>
        [Description("Local name")]
        public string LocalName { get; set; }

        /// <summary>Gets or sets the site.</summary>
        public string Site { get; set; }

        /// <summary>Gets or sets the nearest town.</summary>
        [Description("Nearest town")]
        public string NearestTown { get; set; }

        /// <summary>Gets or sets the region.</summary>
        public string Region { get; set; }

        /// <summary>Gets or sets the state.</summary>
        public string State { get; set; }

        /// <summary>Gets or sets the country.</summary>
        public string Country { get; set; }

        /// <summary>Gets or sets the natural vegetation.</summary>
        [Description("Natural vegetation")]
        public string NaturalVegetation { get; set; }

        /// <summary>Gets or sets the apsoil number.</summary>
        [Description("APSoil number")]
        public string ApsoilNumber { get; set; }

        /// <summary>Gets or sets the latitude.</summary>
        [Description("Latitude (WGS84)")]
        public double Latitude { get; set; }

        /// <summary>Gets or sets the longitude.</summary>
        [Description("Longitude (WGS84)")]
        public double Longitude { get; set; }

        /// <summary>Gets or sets the location accuracy.</summary>
        [Description("Location accuracy")]
        public string LocationAccuracy { get; set; }

        /// <summary>Gets or sets the year of sampling.</summary>
        [Description("Year of sampling")]
        public int YearOfSampling { get; set; }

        /// <summary>Gets or sets the data source.</summary>
        [UILargeText]
        [Description("Data source")]
        public string DataSource { get; set; }

        /// <summary>Gets or sets the comments.</summary>
        [UILargeText]
        public string Comments { get; set; }

        /// <summary>Gets or sets the water.</summary>
        public Water Water { get; set; }

        /// <summary>Gets or sets the soil water.</summary>
        public SoilWater SoilWater { get; set; }

        /// <summary>Gets or sets the soil organic matter.</summary>
        public SoilOrganicMatter SoilOrganicMatter { get; set; }

        /// <summary>Gets or sets the analysis.</summary>
        public Analysis Analysis { get; set; }

        /// <summary>Gets or sets the Nitrogen</summary>
        public Nitrogen Nitrogen { get; set; }

        /// <summary>Gets or sets the initial water.</summary>
        /// <value>The initial water.</value>
        public InitialWater InitialWater { get; set; }

        /// <summary>Gets or sets the samples.</summary>
        [XmlElement("Sample")]
        public List<Sample> Samples { get; set; }

        /// <summary>Gets or sets the layer structure.</summary>
        public LayerStructure LayerStructure { get; set; }

        /// <summary>Gets or sets the phosphorus.</summary>
        public Phosphorus Phosphorus { get; set; }

        /// <summary>Gets or sets the swim.</summary>
        public Swim Swim { get; set; }

        /// <summary>Gets or sets the soil temperature.</summary>
        public SoilTemperature SoilTemperature { get; set; }

        /// <summary>Gets or sets the soil temperature2.</summary>
        public SoilTemperature2 SoilTemperature2 { get; set; }
    }
}
