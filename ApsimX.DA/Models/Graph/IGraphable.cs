﻿// -----------------------------------------------------------------------
// <copyright file="IGraphable.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using System.Collections;
    using System.Data;
    using System.Xml.Serialization;
    /// <summary>
    /// An interface for a model that can graph itself.
    /// </summary>
    public interface IGraphable
    {
        /// <summary>Called by the graph presenter to get a list of all actual series to put on the graph.</summary>
        /// <param name="definitions">A list of definitions to add to.</param>
        void GetSeriesToPutOnGraph(List<SeriesDefinition> definitions);

        /// <summary>Called by the graph presenter to get a list of all annotations to put on the graph.</summary>
        /// <param name="annotations">A list of annotations to add to.</param>
        void GetAnnotationsToPutOnGraph(List<Annotation> annotations);
    }

    /// <summary>An enumeration for the different types of graph series</summary>
    public enum SeriesType 
    {
        /// <summary>A bar series</summary>
        Bar,

        /// <summary>A scatter series</summary>
        Scatter,

        /// <summary>An area series</summary>
        Area 
    }

    /// <summary>An enumeration for the different types of markers</summary>
    public enum MarkerType 
    {
        /// <summary>A filled circle marker</summary>
        FilledCircle,

        /// <summary>A filled diamond marker</summary>
        FilledDiamond,

        /// <summary>A filled square marker</summary>
        FilledSquare,

        /// <summary>A filled triangle marker</summary>
        FilledTriangle,

        /// <summary>A circle marker</summary>
        Circle,

        /// <summary>A diamond marker</summary>
        Diamond,

        /// <summary>A square marker</summary>
        Square,

        /// <summary>A triangle marker</summary>
        Triangle,

        /// <summary>A cross marker</summary>
        Cross,

        /// <summary>A plus marker</summary>
        Plus,

        /// <summary>A star marker</summary>
        Star,

        /// <summary>No marker should be display</summary>
        None
    }

    /// <summary>An enumeration for the different sizes of markers</summary>
    public enum MarkerSizeType
    {
        /// <summary>Normal size markers.</summary>
        Normal,

        /// <summary>Small markers</summary>
        Small
    }

    /// <summary>An enumeration representing the different types of lines</summary>
    public enum LineType 
    {
        /// <summary>A solid line</summary>
        Solid,

        /// <summary>A dashed line</summary>
        Dash,

        /// <summary>A dotted line</summary>
        Dot,

        /// <summary>A dash dot line</summary>
        DashDot,

        /// <summary>No line</summary>
        None 
    }

    /// <summary>An enumeration for the different thicknesses of lines.</summary>
    public enum LineThicknessType
    {
        /// <summary>Normal line thickness</summary>
        Normal,

        /// <summary>Thin line thickess</summary>
        Thin
    }

    /// <summary>
    /// A class for defining a graph series. A list of these is given to graph when graph is drawing itself.
    /// </summary>
    public class SeriesDefinition
    {
        /// <summary>A list of simulation names.</summary>
        public string[] SimulationNames = null;

        /// <summary>Series definition filter.</summary>
        public string Filter = null;

        /// <summary>Gets the series type</summary>
        public SeriesType type;

        /// <summary>Gets the marker to show</summary>
        public MarkerType marker;

        /// <summary>Gets the line type to show</summary>
        public LineType line;

        /// <summary>Gets the marker size</summary>
        public MarkerSizeType markerSize;

        /// <summary>Gets the line thickness</summary>
        public LineThicknessType lineThickness;

        /// <summary>Gets the colour.</summary>
        public Color colour;

        /// <summary>Gets the associated x axis</summary>
        public Axis.AxisType xAxis = Axis.AxisType.Bottom;

        /// <summary>Gets the associated y axis</summary>
        public Axis.AxisType yAxis = Axis.AxisType.Left;

        /// <summary>Gets the x field name.</summary>
        public string xFieldName;

        /// <summary>Gets the t field name.</summary>
        public string yFieldName;

        /// <summary>Gets a value indicating whether this series should be shown in the level.</summary>
        public bool showInLegend;

        /// <summary>Gets the title of the series</summary>
        public string title;

        /// <summary>Gets the dataview</summary>
        [XmlIgnore]
        public DataTable data;

        /// <summary>Gets the x values</summary>
        public IEnumerable x;

        /// <summary>Gets the y values</summary>
        public IEnumerable y;

        /// <summary>Gets the x2 values</summary>
        public IEnumerable x2;

        /// <summary>Gets the y2 values</summary>
        public IEnumerable y2;

        /// <summary>The simulation names for each point.</summary>
        public IEnumerable<string> simulationNamesForEachPoint;
    }

    /// <summary>Base interface for all annotations</summary>
    public interface Annotation
    {
    }

    /// <summary>
    /// A class for defining a text annotation
    /// </summary>
    public class TextAnnotation : Annotation
    {
        /// <summary>X position - can be double.MinValue for autocalculated</summary>
        public object x;

        /// <summary>Y position - can be double.MinValue for autocalculated</summary>
        public object y;

        /// <summary>A text annotation.</summary>
        public string text;

        /// <summary>The colour of the text</summary>
        public Color colour;

        /// <summary>Left align the text?</summary>
        public bool leftAlign;

        /// <summary>Text rotation angle</summary>
        public double textRotation;
    }

    /// <summary>
    /// A class for defining a line annotation
    /// </summary>
    public class LineAnnotation : Annotation
    {
        /// <summary>X1 position - can be double.MinValue for autocalculated</summary>
        public object x1;

        /// <summary>Y1 position - can be double.MinValue for autocalculated</summary>
        public object y1;

        /// <summary>X2 position - can be double.MinValue for autocalculated</summary>
        public object x2;

        /// <summary>Y2 position - can be double.MinValue for autocalculated</summary>
        public object y2;

        /// <summary>The colour of the text</summary>
        public Color colour;

        /// <summary>Gets the line type to show</summary>
        public LineType type;

        /// <summary>Gets the line thickness</summary>
        public LineThicknessType thickness;
    }
}
