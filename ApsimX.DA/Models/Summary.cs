﻿// -----------------------------------------------------------------------
// <copyright file="Summary.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using APSIM.Shared.Utilities;
    using MigraDoc.DocumentObjectModel;
    using MigraDoc.DocumentObjectModel.Tables;
    using MigraDoc.RtfRendering;
    using Models.Core;
    using Report;

    /// <summary>
    /// This model collects the simulation initial conditions and stores into the DataStore.
    /// It also provides an API for writing messages to the DataStore.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.SummaryView")]
    [PresenterName("UserInterface.Presenters.SummaryPresenter")]
    [ValidParent(ParentType=typeof(Simulation))]
    public class Summary : Model, ISummary
    {
        /// <summary>
        /// The messages data table.
        /// </summary>
        private ReportTable messagesTable = new Report.ReportTable();

        private ReportColumnWithValues componentNameColumn;
        private ReportColumnWithValues dateColumn;
        private ReportColumnWithValues messageColumn;
        private ReportColumnWithValues messageTypeColumn;

        /// <summary>
        /// A link to the clock in the simulation.
        /// </summary>
        [Link]
        private Clock clock = null;

        /// <summary>
        /// Enumeration used to indicate the format of the output string
        /// </summary>
        public enum OutputType
        {
            /// <summary>
            /// Plain ASCII text
            /// </summary>
            plain,

            /// <summary>
            /// HTML format
            /// </summary>
            html,

            /// <summary>
            /// RTF format
            /// </summary>
            rtf
        }

        /// <summary>
        /// Gets a link to the simulation.
        /// </summary>
        public Simulation Simulation
        {
            get
            {
                return Apsim.Parent(this, typeof(Simulation)) as Simulation;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Summary()
        {
            // Create our Messages table.
            componentNameColumn = new ReportColumnWithValues("ComponentName");
            dateColumn = new ReportColumnWithValues("Date");
            messageColumn = new ReportColumnWithValues("Message");
            messageTypeColumn = new ReportColumnWithValues("MessageType");

            messagesTable.Columns.Add(componentNameColumn);
            messagesTable.Columns.Add(dateColumn);
            messagesTable.Columns.Add(messageColumn);
            messagesTable.Columns.Add(messageTypeColumn);
        }

        /// <summary>
        /// Write the summary report to the specified writer.
        /// </summary>
        /// <param name="dataStore">The data store to write a summary report from</param>
        /// <param name="simulationName">The simulation name to produce a summary report for</param>
        /// <param name="writer">Text writer to write to</param>
        /// <param name="apsimSummaryImageFileName">The file name for the logo. Can be null</param>
        /// <param name="outtype">Indicates the format to be produced</param>
        public static void WriteReport(
            DataStore dataStore,
            string simulationName,
            TextWriter writer,
            string apsimSummaryImageFileName,
            OutputType outtype)
        {
            Document document = null;
            RtfDocumentRenderer renderer = null;

            if (outtype == OutputType.html)
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("<meta content='text/html; charset=UTF-8; http-equiv='content-type'>");
                writer.WriteLine("<style>");
                writer.WriteLine("h2 { color:darkblue; } ");
                writer.WriteLine("h3 { color:darkblue; } ");
                writer.WriteLine("table { border:1px solid black; border-collapse:collapse; width:100%; table-layout:fixed; text-align:left; }");
                writer.WriteLine("table.headered {text-align:right; }");
                writer.WriteLine("tr.total { color:darkorange; font-weight:bold; }");
                writer.WriteLine("table.headered td.col1 { text-align:left; font-weight:bold; }");
                writer.WriteLine("td { border:1px solid; }");
                writer.WriteLine("th { border:1px solid; text-align:right; background-color: palegoldenrod}");
                writer.WriteLine("th.col1 { text-align:left; }");
                writer.WriteLine("</style>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
            }
            else if (outtype == OutputType.rtf)
            {
                document = new Document();
                renderer = new RtfDocumentRenderer();

                // Get the predefined style Normal.
                Style style = document.Styles["Normal"];

                // Because all styles are derived from Normal, the next line changes the 
                // font of the whole document. Or, more exactly, it changes the font of
                // all styles and paragraphs that do not redefine the font.
                style.Font.Name = "Arial";

                // Heading1 to Heading9 are predefined styles with an outline level. An outline level
                // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) 
                // in PDF.
                style = document.Styles["Heading2"];
                style.Font.Size = 14;
                style.Font.Bold = true;
                style.Font.Color = Colors.DarkBlue;
                style.ParagraphFormat.PageBreakBefore = false;
                style.ParagraphFormat.SpaceAfter = 3;
                style.ParagraphFormat.SpaceBefore = 16;

                style = document.Styles["Heading3"];
                style.Font.Size = 12;
                style.Font.Bold = true;
                style.Font.Color = Colors.DarkBlue;
                style.ParagraphFormat.SpaceBefore = 10;
                style.ParagraphFormat.SpaceAfter = 2;

                // Create a new style called Monospace based on style Normal
                style = document.Styles.AddStyle("Monospace", "Normal");
                System.Drawing.FontFamily monoFamily = new System.Drawing.FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace);
                style.Font.Name = monoFamily.Name;
                Section section = document.AddSection();
            }

            // Get the initial conditions table.            
            DataTable initialConditionsTable = dataStore.GetData(simulationName, "InitialConditions");
            if (initialConditionsTable != null)
            {
                // Convert the 'InitialConditions' table in the DataStore to a series of
                // DataTables for each model.
                List<DataTable> tables = new List<DataTable>();
                ConvertInitialConditionsToTables(initialConditionsTable, tables);

                // Now write all tables to our report.
                for (int i = 0; i < tables.Count; i += 2)
                {
                    // Only write something to the summary file if we have something to write.
                    if (tables[i].Rows.Count > 0 || tables[i + 1].Rows.Count > 0)
                    {
                        string heading = tables[i].TableName;
                        WriteHeading(writer, heading, outtype, document);

                        // Write the manager script.
                        if (tables[i].Rows.Count == 1 && tables[i].Rows[0][0].ToString() == "Script code: ")
                        {
                            WriteScript(writer, tables[i].Rows[0], outtype, document);
                        }
                        else
                        {
                            // Write the properties table if we have any properties.
                            if (tables[i].Rows.Count > 0)
                            {
                                WriteTable(writer, tables[i], outtype, "PropertyTable", document);
                            }

                            // Write the general data table if we have any data.
                            if (tables[i + 1].Rows.Count > 0)
                            {
                                WriteTable(writer, tables[i + 1], outtype, "ApsimTable", document);
                            }
                        }

                        if (outtype == OutputType.html)
                            writer.WriteLine("<br/>");
                    }
                }
            }

            // Write out all messages.
            WriteHeading(writer, "Simulation log:", outtype, document);
            DataTable messageTable = GetMessageTable(dataStore, simulationName);
            WriteMessageTable(writer, messageTable, outtype, false, "MessageTable", document);

            if (outtype == OutputType.html)
            {
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
            else if (outtype == OutputType.rtf)
            {
                string rtf = renderer.RenderToString(document, Path.GetTempPath());
                writer.Write(rtf);
            }
        }

        /// <summary>
        /// All simulations have been completed. 
        /// </summary>
        /// <param name="sender">The sending Object</param>
        /// <param name="e">Arguments associated with the event</param>
        [EventSubscribe("Completed")]
        private void OnSimulationCompleted(object sender, EventArgs e)
        {
            DataStore dataStore = new DataStore(this);
            dataStore.WriteTable(this.messagesTable);
            dataStore.Disconnect();
        }

        /// <summary>
        /// Write a message to the summary
        /// </summary>
        /// <param name="model">The model writing the message</param>
        /// <param name="message">The message to write</param>
        public void WriteMessage(IModel model, string message)
        {
            string modelFullPath = Apsim.FullPath(model);
            string relativeModelPath = modelFullPath.Replace(Apsim.FullPath(Simulation) + ".", string.Empty).TrimStart(".".ToCharArray());

            componentNameColumn.Add(relativeModelPath);
            dateColumn.Add(clock.Today);
            messageColumn.Add(message);
            messageTypeColumn.Add(Convert.ToInt32(DataStore.ErrorLevel.Information));
        }

        /// <summary>
        /// Write a warning message to the summary
        /// </summary>
        /// <param name="model">The model writing the message</param>
        /// <param name="message">The warning message to write</param>
        public void WriteWarning(IModel model, string message)
        {
            if (this.messagesTable != null)
            {
                componentNameColumn.Add(Apsim.FullPath(model));
                dateColumn.Add(clock.Today);
                messageColumn.Add(message);
                messageTypeColumn.Add(Convert.ToInt32(DataStore.ErrorLevel.Warning));
            }
        }

        #region Private static summary report generation

        /// <summary>
        /// Create a message table ready for writing.
        /// </summary>
        /// <param name="dataStore">The data store to read the message table from</param>
        /// <param name="simulationName">The simulation name to get messages for</param>
        /// <returns>The filled message table</returns>
        private static DataTable GetMessageTable(DataStore dataStore, string simulationName)
        {
            DataTable messageTable = new DataTable();
            DataTable messages = dataStore.GetData(simulationName, "Messages");
            if (messages != null && messages.Rows.Count > 0)
            {
                messageTable.Columns.Add("Date", typeof(string));
                messageTable.Columns.Add("Message", typeof(string));
                string previousCol1Text = null;
                string previousMessage = null;
                foreach (DataRow row in messages.Rows)
                {
                    // Work out the column 1 text.
                    string modelName = (string)row[1];

                    string col1Text;
                    if (row[2].GetType() == typeof(DateTime))
                    {
                        DateTime date = (DateTime)row[2];
                        col1Text = date.ToString("yyyy-MM-dd") + " " + modelName;
                    }
                    else
                        col1Text = row[2].ToString();

                    // If the date and model name have changed then write a row.
                    if (col1Text != previousCol1Text)
                    {
                        if (previousCol1Text != null)
                        {
                            messageTable.Rows.Add(new object[] { previousCol1Text, previousMessage });
                        }

                        previousMessage = string.Empty;
                        previousCol1Text = col1Text;
                    }
                    else
                    {
                        col1Text = null;
                    }

                    string message = (string)row[3];
                    Models.DataStore.ErrorLevel errorLevel = (Models.DataStore.ErrorLevel)Enum.Parse(typeof(Models.DataStore.ErrorLevel), row[4].ToString());

                    if (errorLevel == DataStore.ErrorLevel.Error)
                    {
                        previousMessage += "FATAL ERROR: " + message;
                    }
                    else if (errorLevel == DataStore.ErrorLevel.Warning)
                    {
                        previousMessage += "WARNING: " + message;
                    }
                    else
                    {
                        previousMessage += message;
                    }

                    previousMessage += "\r\n";
                }
                if (previousMessage != null)
                {
                    messageTable.Rows.Add(new object[] { previousCol1Text, previousMessage });
                }
            }

            return messageTable;
        }

        /// <summary>
        /// Write the specified heading to the TextWriter.
        /// </summary>
        /// <param name="writer">Text writer to write to</param>
        /// <param name="heading">The heading to write</param>
        /// <param name="outtype">Indicates the format to be produced</param>
        /// <param name="document">Document object if using MigraDoc to generate output, null otherwise </param>
        private static void WriteHeading(TextWriter writer, string heading, OutputType outtype, Document document)
        {
            if (outtype == OutputType.html)
            {
                writer.WriteLine("<h2>" + heading + "</h2>");
            }
            else if (outtype == OutputType.rtf)
            {
                Section section = document.LastSection;
                Paragraph paragraph = section.AddParagraph(heading, "Heading2");
            }
            else
            {
                writer.WriteLine(heading.ToUpper());
                writer.WriteLine(new string('-', heading.Length));
            }
        }

        /// <summary>
        /// Write out manager script
        /// </summary>
        /// <param name="writer">Text writer to write to</param>
        /// <param name="row">The data table row containing the script</param>
        /// <param name="outtype">Indicates the format to be produced</param>
        /// <param name="document">Document object if using MigraDoc to generate output, null otherwise </param>
        private static void WriteScript(TextWriter writer, DataRow row, OutputType outtype, Document document)
        {
            string st = row[1].ToString();
            st = st.Replace("\t", "    ");
            if (outtype == OutputType.html)
            {
                writer.WriteLine("<pre>");
                st = st.Replace("&", "&amp;");
                st = st.Replace("<", "&lt;");
                st = st.Replace(">", "&gt;");
                writer.WriteLine(st);
                writer.WriteLine("</pre>");
            }
            else if (outtype == OutputType.rtf)
            {
                Paragraph paragraph = document.LastSection.AddParagraph(st, "Monospace");
            }
            else
            {
                writer.WriteLine(st);
            }
        }

        /// <summary>
        /// Write the specified table to the TextWriter.
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="table">The table to write</param>
        /// <param name="outtype">Indicates the format to be produced</param>
        /// <param name="className">The class name of the generated html table</param>
        /// <param name="document">Document object if using MigraDoc to generate output, null otherwise </param>
        private static void WriteTable(TextWriter writer, DataTable table, OutputType outtype, string className, Document document)
        {
            bool showHeadings = className != "PropertyTable";
            if (outtype == OutputType.html)
            {
                if (showHeadings)
                {
                    writer.WriteLine("<table class='headered'>");
                    writer.Write("<tr>");
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        writer.Write("<th");
                        if (i == 0)
                            writer.Write(" class='col1'");
                        writer.Write(">" + table.Columns[i].ColumnName + "</th>");
                    }
                }
                else
                    writer.WriteLine("<table>");

                foreach (DataRow row in table.Rows)
                {
                    bool titleRow = Convert.IsDBNull(row[0]);
                    if (titleRow)
                        writer.Write("<tr class='total'>");
                    else
                        writer.Write("<tr>");

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        string st;
                        if (titleRow && i == 0)
                            st = "Total";
                        else
                            st = row[i].ToString();
                        
                        writer.Write("<td");
                        if (i == 0)
                            writer.Write(" class='col1'");
                        writer.Write(">");
                        writer.Write(st);
                        writer.Write("</td>");
                    }
                    writer.WriteLine("</tr>");
                }
                writer.WriteLine("</table><br/>");
            }
            else if (outtype == OutputType.rtf)
            {
                Table tabl = new Table();
                tabl.Borders.Width = 0.75;

                foreach (DataColumn col in table.Columns)
                {
                    Column column = tabl.AddColumn(Unit.FromCentimeter(18.0 / table.Columns.Count));
                }

                if (showHeadings)
                {
                    Row row = tabl.AddRow();
                    row.Shading.Color = Colors.PaleGoldenrod;
                    tabl.Shading.Color = new Color(245, 245, 255);
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        Cell cell = row.Cells[i];
                        Paragraph paragraph = cell.AddParagraph();
                        if (i == 0)
                            paragraph.Format.Alignment = ParagraphAlignment.Left;
                        else
                            paragraph.Format.Alignment = ParagraphAlignment.Right;
                        paragraph.AddText(table.Columns[i].ColumnName);
                    }
                }

                foreach (DataRow row in table.Rows)
                {
                    bool titleRow = Convert.IsDBNull(row[0]);
                    string st;
                    Row newRow = tabl.AddRow();

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (titleRow && i == 0)
                        {
                            st = "Total";
                            newRow.Format.Font.Color = Colors.DarkOrange;
                            newRow.Format.Font.Bold = true;
                        }
                        else
                            st = row[i].ToString();

                        Cell cell = newRow.Cells[i];
                        Paragraph paragraph = cell.AddParagraph();
                        if (!showHeadings)
                        {
                            cell.Borders.Style = BorderStyle.None;
                            paragraph.Format.Alignment = ParagraphAlignment.Left;
                        }
                        else if (i == 0)
                            paragraph.Format.Alignment = ParagraphAlignment.Left;
                        else
                            paragraph.Format.Alignment = ParagraphAlignment.Right;

                        if (showHeadings && i == 0)
                            paragraph.AddFormattedText(st, TextFormat.Bold);
                        else
                            paragraph.AddText(st);
                    }
                }

                document.LastSection.Add(tabl);
                document.LastSection.AddParagraph(); // Just to give a bit of spacing
            }
            else
            {
                DataTableUtilities.DataTableToText(table, 0, "  ", showHeadings, writer);
            }
        }

        /// <summary>
        /// Write the specified table to the TextWriter.
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="table">The table to write</param>
        /// <param name="outtype">Indicates the format to be produced</param>
        /// <param name="includeHeadings">Include headings in the html table produced?</param>
        /// <param name="className">The class name of the generated html table</param>
        /// <param name="document">Document object if using MigraDoc to generate output, null otherwise </param>
        private static void WriteMessageTable(TextWriter writer, DataTable table, OutputType outtype, bool includeHeadings, string className, Document document)
        {
            foreach (DataRow row in table.Rows)
            {
                if (outtype == OutputType.html)
                {
                    writer.WriteLine("<h3>" + row[0] + "</h3>");
                }
                else if (outtype == OutputType.rtf)
                {
                    Section section = document.LastSection;
                    Paragraph paragraph = section.AddParagraph(row[0].ToString(), "Heading3");
                }
                else
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine(row[0].ToString());
                }

                string st = row[1].ToString();
                st = st.Replace("\t", "    ");
                if (outtype == OutputType.html)
                {
                    writer.WriteLine("<pre>");
                    st = st.Replace("&", "&amp;");
                    st = st.Replace("<", "&lt;");
                    st = st.Replace(">", "&gt;");
                    writer.WriteLine(st);
                    writer.WriteLine("</pre>");
                }
                else if (outtype == OutputType.rtf)
                {
                    Section section = document.LastSection;
                    Paragraph paragraph = section.AddParagraph(st, "Monospace");
                    if (st.Contains("WARNING:"))
                        paragraph.Format.Font.Color = Colors.OrangeRed;
                    else if (st.Contains("ERROR:"))
                        paragraph.Format.Font.Color = Colors.Red;
                }
                else
                {
                    st = StringUtilities.IndentText(st, 4);
                    writer.WriteLine(st);
                }
            }
        }

        /// <summary>
        /// Find all properties from the model and fill this.properties.
        /// </summary>
        /// <param name="model">The model to search for properties</param>
        /// <param name="properties">The list of properties to fill</param>
        private static void FindAllProperties(Model model, List<VariableProperty> properties)
        {
            if (model != null)
            {
                foreach (PropertyInfo property in model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                {
                    // Properties must have a [Summary] attribute
                    bool includeProperty = property.IsDefined(typeof(SummaryAttribute), false);

                    if (includeProperty)
                    {
                        properties.Add(new VariableProperty(model, property));
                    }
                }
            }
        }

        /// <summary>
        /// Converts a flat 'InitialConditions' table (from the data store) to a series of data tables.
        /// </summary>
        /// <param name="initialConditionsTable">The table to read the rows from</param>
        /// <param name="tables">The list of tables to create</param>
        private static void ConvertInitialConditionsToTables(DataTable initialConditionsTable, List<DataTable> tables)
        {
            DataTable propertyDataTable = null;
            DataTable generalDataTable = null;
            string previousModel = null;
            foreach (DataRow row in initialConditionsTable.Rows)
            {
                string modelPath = row["ModelPath"].ToString();

                // If this is a new model then write a new section for it.
                if (modelPath != previousModel)
                {
                    // Add a new properties table for this model.
                    propertyDataTable = new DataTable(modelPath);
                    propertyDataTable.Columns.Add("Name", typeof(string));
                    propertyDataTable.Columns.Add("Value", typeof(string));
                    tables.Add(propertyDataTable);

                    // Add a new data table for this model.
                    generalDataTable = new DataTable("General " + modelPath);
                    tables.Add(generalDataTable);

                    previousModel = modelPath;
                }

                // Work out the property name.
                string propertyName = row["Description"].ToString();
                if (propertyName == string.Empty)
                    propertyName = row["Name"].ToString();
                string units = row["Units"].ToString();
                string displayFormat = row["DisplayFormat"].ToString();

                // If the data type is an array then write the general datatable.
                if (row["DataType"].ToString().Contains("[]"))
                {
                    if (units != null && units != string.Empty)
                    {
                        propertyName += " (" + units + ")";
                    }

                    bool showTotal = Convert.ToInt32(row["Total"]) == 1;
                    AddArrayToTable(propertyName, row["DataType"].ToString(), displayFormat, showTotal, row["Value"], generalDataTable);
                }
                else
                {
                    string value = FormatPropertyValue(row["DataType"].ToString(), row["Value"], displayFormat);
                    if (units != null && units != string.Empty)
                    {
                        value += " (" + units + ")";
                    }

                    propertyDataTable.Rows.Add(new object[]
                    {
                        propertyName + ": ",
                        value
                    });
                }
            }
        }

        /// <summary>
        /// Add a column to the specified table based on values in the 'value'
        /// </summary>
        /// <param name="heading">The new column heading</param>
        /// <param name="dataTypeName">The data type of the value</param>
        /// <param name="displayFormat">The display format to use when writing the column</param>
        /// <param name="showTotal">A value indicating whether a total should be added.</param>
        /// <param name="value">The values containing the array</param>
        /// <param name="table">The table where a column should be added to</param>
        private static void AddArrayToTable(string heading, string dataTypeName, string displayFormat, bool showTotal, object value, DataTable table)
        {
            if (displayFormat == null)
            {
                displayFormat = "N3";
            }

            string[] stringValues = value.ToString().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataTypeName == "Double[]")
            {
                List<double> values = new List<double>();
                values.AddRange(MathUtilities.StringsToDoubles(stringValues));
                if (showTotal)
                {
                    values.Add(MathUtilities.Sum(values));
                }

                stringValues = MathUtilities.DoublesToStrings(values, displayFormat);
            }
            else if (dataTypeName == "Int32[]")
            {
                List<double> values = new List<double>();
                values.AddRange(MathUtilities.StringsToDoubles(stringValues));
                if (showTotal)
                {
                    values.Add(MathUtilities.Sum(values));
                }

                stringValues = MathUtilities.DoublesToStrings(values, "N0");
            }
            else if (dataTypeName != "String[]")
            {
                // throw new ApsimXException(null, "Invalid property type: " + dataTypeName);
            }

            DataTableUtilities.AddColumn(table, heading, stringValues);
        }

        /// <summary>
        /// Format the specified value into a string and return the string.
        /// </summary>
        /// <param name="dataTypeName">The name of the data type</param>
        /// <param name="value">The value to format</param>
        /// <param name="format">The format to use for the value</param>
        /// <returns>The formatted value as a string</returns>
        private static string FormatPropertyValue(string dataTypeName, object value, string format)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (dataTypeName == "Double" || dataTypeName == "Single")
            {
                double doubleValue = Convert.ToDouble(value);
                if (format == null || format == string.Empty)
                {
                    return string.Format("{0:F3}", doubleValue);
                }
                else
                {
                    return string.Format("{0:" + format + "}", doubleValue);
                }
            }
            else if (dataTypeName == "DateTime")
            {
                DateTime date = DateTime.ParseExact(value.ToString(), "yyyy-MM-dd hh:mm:ss", null);
                return date.ToString("yyyy-MM-dd");
            }
            else
            {
                return value.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Simulation is commencing.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [EventSubscribe("DoInitialSummary")]
        private void OnDoInitialSummary(object sender, EventArgs e)
        {
            messagesTable.FileName = Path.ChangeExtension(Simulation.FileName, ".db");
            messagesTable.SimulationName = Simulation.Name;
            messagesTable.TableName = "Messages";

            // Create an initial conditions table in the DataStore.
            CreateInitialConditionsTable();
        }

        /// <summary>
        /// Create an initial conditions table in the DataStore.
        /// </summary>
        private void CreateInitialConditionsTable()
        {
            ReportColumnWithValues modelPath = new ReportColumnWithValues("ModelPath");
            ReportColumnWithValues name = new ReportColumnWithValues("Name");
            ReportColumnWithValues description = new ReportColumnWithValues("Description");
            ReportColumnWithValues dataType = new ReportColumnWithValues("DataType");
            ReportColumnWithValues units = new ReportColumnWithValues("Units");
            ReportColumnWithValues displayFormat = new ReportColumnWithValues("DisplayFormat");
            ReportColumnWithValues showTotal = new ReportColumnWithValues("Total");
            ReportColumnWithValues value = new ReportColumnWithValues("Value");
            ReportTable table = new Report.ReportTable();
            table.FileName = Path.ChangeExtension(Simulation.FileName, ".db");
            table.SimulationName = Simulation.Name;
            table.TableName = "InitialConditions";
            table.Columns.Add(modelPath);
            table.Columns.Add(name);
            table.Columns.Add(description);
            table.Columns.Add(dataType);
            table.Columns.Add(units);
            table.Columns.Add(displayFormat);
            table.Columns.Add(showTotal);
            table.Columns.Add(value);

            modelPath.Add(Apsim.FullPath(Simulation)); 
            name.Add("Simulation name");
            description.Add("Simulation name");
            dataType.Add("String");
            units.Add(string.Empty);
            displayFormat.Add(string.Empty);
            showTotal.Add(0);
            value.Add(Simulation.Name);

            modelPath.Add(Apsim.FullPath(Simulation));
            name.Add("APSIM version");
            description.Add("APSIM version");
            dataType.Add("String");
            units.Add(string.Empty);
            displayFormat.Add(string.Empty);
            showTotal.Add(0);
            value.Add(Simulation.ApsimVersion);

            modelPath.Add(Apsim.FullPath(Simulation));
            name.Add("Run on");
            description.Add("Run on");
            dataType.Add("String");
            units.Add(string.Empty);
            displayFormat.Add(string.Empty);
            showTotal.Add(0);
            value.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Get all model properties and store in 'initialConditionsTable'
            foreach (Model model in Apsim.FindAll(Simulation))
            {
                string relativeModelPath = Apsim.FullPath(model).Replace(Apsim.FullPath(Simulation) + ".", string.Empty);
                List<VariableProperty> properties = new List<VariableProperty>();

                FindAllProperties(model, properties);

                foreach (VariableProperty property in properties)
                {
                    string propertyValue = property.ValueWithArrayHandling.ToString();
                    if (propertyValue != string.Empty)
                    {
                        if (propertyValue != null && property.DataType == typeof(DateTime))
                        {
                            propertyValue = ((DateTime)property.Value).ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        modelPath.Add(relativeModelPath);
                        name.Add(property.Name);
                        description.Add(property.Description);
                        dataType.Add(property.DataType.Name);
                        units.Add(property.Units);
                        displayFormat.Add(property.Format);
                        if (double.IsNaN(property.Total))
                            showTotal.Add(0);
                        else
                            showTotal.Add(1);
                        value.Add(propertyValue);
                    }
                }
            }

            // Write to data store.
            DataStore dataStore = new DataStore(Simulation);
            dataStore.WriteTable(table);
            dataStore.Disconnect();
        }
    }
}
