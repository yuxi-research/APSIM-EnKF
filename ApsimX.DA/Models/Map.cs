﻿using System;
using System.Text;
using Models.Core;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;
using APSIM.Shared.Utilities;

namespace Models
{
    /// <summary>This is a memo/text component that stores user entered text information.</summary>
    [Serializable]
    [ViewName("UserInterface.Views.MapView")]
    [PresenterName("UserInterface.Presenters.MapPresenter")]
    [ValidParent(DropAnywhere = true)]
    public class Map : Model, AutoDocumentation.ITag
    {
        /// <summary>
        /// Class for representing a latitude and longitude.
        /// </summary>
        [Serializable]
        public class Coordinate
        {
            /// <summary>The latitude</summary>
            public double Latitude { get; set; }

            /// <summary>The longitude</summary>
            public double  Longitude { get; set; }
        }

        /// <summary>List of coordinates to show on map</summary>
        public List<Coordinate> GetCoordinates()
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            foreach (Weather weather in Apsim.FindAll(this, typeof(Weather)))
            {
                weather.OpenDataFile();
                double latitude = weather.Latitude;
                double longitude = weather.Longitude;
                weather.CloseDataFile();
                if (latitude != 0 && longitude != 0)
                {
                    Coordinate coordinate = new Coordinate();
                    coordinate.Latitude = latitude;
                    coordinate.Longitude = longitude;
                    coordinates.Add(coordinate);
                }
            }

            return coordinates;
        }

        /// <summary>
        /// Coordinate of the center of the map
        /// </summary>
        public Coordinate Center = new Coordinate() { Latitude = 0.0, Longitude = 0.0 };

        /// <summary>
        /// Zoom factor for the map
        /// </summary>
        public Double Zoom = 1.4;

        /// <summary>Writes documentation for this function by adding to the list of documentation tags.</summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public override void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            tags.Add(this);
        }


    }
}
