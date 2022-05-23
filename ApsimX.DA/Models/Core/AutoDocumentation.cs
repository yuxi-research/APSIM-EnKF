﻿// -----------------------------------------------------------------------
// <copyright file="AutoDocumentation.cs" company="APSIM Initiative">
// Copyright APSIM Initiative.
// </copyright>
// -----------------------------------------------------------------------
namespace Models.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using APSIM.Shared.Utilities;
    using System.Xml;
    using System.IO;
    using Models.PMF.Functions;
    using System.Data;

    /// <summary>
    /// A class of auto-documentation methods and HTML building widgets.
    /// </summary>
    public class AutoDocumentation
    {
        private static XmlDocument doc = null;

        /// <summary>Gets the units from a declaraion.</summary>
        /// <param name="model">The model containing the declaration field.</param>
        /// <param name="fieldName">The declaration field name.</param>
        /// <returns>The units (no brackets) or any empty string.</returns>
        public static string GetUnits(IModel model, string fieldName)
        {
            FieldInfo field = model.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                UnitsAttribute unitsAttribute = ReflectionUtilities.GetAttribute(field, typeof(UnitsAttribute), false) as UnitsAttribute;
                if (unitsAttribute != null)
                    return unitsAttribute.ToString();
            }

            return string.Empty;
        }

        /// <summary>Gets the description from a declaraion.</summary>
        /// <param name="model">The model containing the declaration field.</param>
        /// <param name="fieldName">The declaration field name.</param>
        /// <returns>The description or any empty string.</returns>
        public static string GetDescription(IModel model, string fieldName)
        {
            FieldInfo field = model.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                DescriptionAttribute descriptionAttribute = ReflectionUtilities.GetAttribute(field, typeof(DescriptionAttribute), false) as DescriptionAttribute;
                if (descriptionAttribute != null)
                    return descriptionAttribute.ToString();
            }

            return string.Empty;
        }


        /// <summary>Writes the description of a class to the tags.</summary>
        /// <param name="model">The model to get documentation for.</param>
        /// <param name="tags">The tags to add to.</param>
        /// <param name="indent">The indentation level.</param>
        public static void GetClassDescription(object model, List<ITag> tags, int indent)
        {
            if (doc == null)
            {
                string fileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".xml");
                doc = new XmlDocument();
                doc.Load(fileName);
            }

            XmlNode summaryNode = XmlUtilities.Find(doc.DocumentElement, "members/T:" + model.GetType().FullName + "/summary");
            if (summaryNode != null)
            {
                int numSpacesStartOfLine = -1;
                string paragraphSoFar = string.Empty;
                string st = summaryNode.InnerXml;
                if (st.StartsWith("\r\n"))
                    st = st.Remove(0, 2);
                StringReader reader = new StringReader(st);
                string line = reader.ReadLine();
                while (line != null && line.Trim() != string.Empty)
                {
                    if (numSpacesStartOfLine == -1)
                    {
                        int preLineLength = line.Length;
                        line = line.TrimStart();
                        numSpacesStartOfLine = preLineLength - line.Length - 1;
                    }
                    else
                        line = line.Remove(0, numSpacesStartOfLine);

                    string heading;
                    int headingLevel;
                    if (GetHeadingFromLine(line, out heading, out headingLevel))
                    {
                        if (paragraphSoFar != string.Empty)
                        {
                            tags.Add(new Paragraph(paragraphSoFar, indent));
                            paragraphSoFar = string.Empty;
                        }
                        tags.Add(new Heading(heading, headingLevel));
                    }
                    else
                        paragraphSoFar += line + "\r\n";

                    line = reader.ReadLine();
                }

                if (paragraphSoFar != string.Empty)
                {
                    tags.Add(new Paragraph(paragraphSoFar, indent));
                    paragraphSoFar = string.Empty;
                }
            }
        }

        /// <summary>Look at a string and return true if it is a heading.</summary>
        /// <param name="st">The string to look at.</param>
        /// <param name="heading">The returned heading.</param>
        /// <param name="headingLevel">The returned heading level.</param>
        /// <returns></returns>
        private static bool GetHeadingFromLine(string st, out string heading, out int headingLevel)
        {
            st = st.Trim();
            heading = st.Replace("#", string.Empty);
            headingLevel = 0;
            if (st.StartsWith("####"))
            {
                headingLevel = 4;
                return true;
            }
            if (st.StartsWith("###"))
            {
                headingLevel = 3;
                return true;
            }
            if (st.StartsWith("#"))
            {
                headingLevel = 2;
                return true;
            }
            if (st.StartsWith("#"))
            {
                headingLevel = 1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Describes an interface for a auto-doc command.
        /// </summary>
        public interface ITag
        {
        }

        /// <summary>
        /// Describes an auto-doc heading command.
        /// </summary>
        public class Heading : ITag
        {
            /// <summary>The heading text</summary>
            public string text;

            /// <summary>The heading level</summary>
            public int headingLevel;

            /// <summary>
            /// Initializes a new instance of the <see cref="Heading"/> class.
            /// </summary>
            /// <param name="text">The heading text.</param>
            /// <param name="headingLevel">The heading level.</param>
            public Heading(string text, int headingLevel)
            {
                this.text = text;
                this.headingLevel = headingLevel;
            }
        }

        /// <summary>
        /// Describes an auto-doc paragraph command.
        /// </summary>
        public class Paragraph : ITag
        {
            /// <summary>The paragraph text.</summary>
            public string text;

            /// <summary>The indent level.</summary>
            public int indent;

            /// <summary>The bookmark name (optional)</summary>
            public string bookmarkName;

            /// <summary>Should the paragraph indent all lines except the first?</summary>
            public bool handingIndent;

            /// <summary>
            /// Initializes a new instance of the <see cref="Paragraph"/> class.
            /// </summary>
            /// <param name="text">The paragraph text.</param>
            /// <param name="indent">The paragraph indent.</param>
            public Paragraph(string text, int indent)
            {
                this.text = text;
                this.indent = indent;
            }
        }

        /// <summary>Describes an auto-doc graph and table command.</summary>
        public class GraphAndTable : ITag
        {
            /// <summary>The data to show in graph and table.</summary>
            public XYPairs xyPairs;

            /// <summary>The graph title</summary>
            public string title;

            /// <summary>The x axis title.</summary>
            public string xName;

            /// <summary>The y axis title</summary>
            public string yName;

            /// <summary>The indent level.</summary>
            public int indent;

            /// <summary>
            /// Initializes a new instance of the <see cref="GraphAndTable"/> class.
            /// </summary>
            /// <param name="xyPairs">The xy pairs.</param>
            /// <param name="title">Graph title.</param>
            /// <param name="xName">The x axis title.</param>
            /// <param name="yName">The y axis title.</param>
            /// <param name="indent">The indentation.</param>
            public GraphAndTable(XYPairs xyPairs, string title, string xName, string yName, int indent)
            {
                this.title = title;
                this.xyPairs = xyPairs;
                this.xName = xName;
                this.yName = yName;
                this.indent = indent;
            }
        }

        /// <summary>Describes an auto-doc table command.</summary>
        public class Table : ITag
        {
            /// <summary>The data to show in the table.</summary>
            public List<List<string>> data;

            /// <summary>The indent level.</summary>
            public int indent;

            /// <summary>
            /// Initializes a new instance of the <see cref="Table"/> class.
            /// </summary>
            /// <param name="data">The column / row data.</param>
            /// <param name="indent">The indentation.</param>
            public Table(List<List<string>> data, int indent)
            {
                this.data = data;
                this.indent = indent;
            }
        }

        /// <summary>Descibes an image for the tags system.</summary>
        public class Image : ITag
        {
            /// <summary>The image to put into the doc.</summary>
            public System.Drawing.Image image;

            /// <summary>Unique name for image. Used to save image to temp folder.</summary>
            public string name;
        }


        /// <summary>Describes a new page for the tags system.</summary>
        public class NewPage : ITag
        {

        }
    }
}
