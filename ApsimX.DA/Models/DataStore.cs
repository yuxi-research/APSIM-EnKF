﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Models.Core;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using APSIM.Shared.Utilities;
using Models.Report;

namespace Models
{
    /// <summary>
    /// A data storage model
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.DataStoreView")]
    [PresenterName("UserInterface.Presenters.DataStorePresenter")]
    [ValidParent(ParentType = typeof(Simulations))]
    public class DataStore : Model
    {
        /// <summary>A SQLite connection shared between all instances of this DataStore.</summary>
        [NonSerialized]
        private SQLite Connection = null;

        /// <summary>The filename of the SQLite .db</summary>
        /// <value>The filename.</value>
        [XmlIgnore]
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data store should export to text files
        /// automatically when all simulations finish.
        /// </summary>
        /// <value><c>true</c> if [automatic export]; otherwise, <c>false</c>.</value>
        public bool AutoExport { get; set; }

        /// <summary>The maximum number of results to display per page.</summary>
        public int MaximumResultsPerPage { get; set; }


        /// <summary>A collection of datatables that need writing.</summary>
        public static List<ReportTable> TablesToWrite = new List<ReportTable>();


        /// <summary>
        /// This class encapsulates a simple lock mechanism. It is used by DataStore to
        /// apply file level locking.
        /// </summary>
        private class DbMutex
        {
            /// <summary>The locked</summary>
            private bool Locked = false;

            /// <summary>Aquire a lock. If already locked then wait a bit.</summary>
            public void Aquire()
            {
                lock (this)
                {
                    while (Locked)
                        Thread.Sleep(100);
                    Locked = true;
                }
            }

            /// <summary>Release a lock.</summary>
            public void Release()
            {
                Locked = false;
            }
        }

        /// <summary>A static dictionary of locks, one for each filename.</summary>
        private static Dictionary<string, DbMutex> Locks = new Dictionary<string, DbMutex>();

        /// <summary>Is the .db file open for writing?</summary>
        private bool ForWriting = false;

        /// <summary>
        /// An enum that is used to indicate message severity when writing messages to the .db
        /// </summary>
        public enum ErrorLevel 
        {
            /// <summary>Information</summary>
            Information,

            /// <summary>Warning</summary>
            Warning,

            /// <summary>Error</summary>
            Error 
        };

        /// <summary>
        /// A parameterless constructor purely for the XML serialiser. Other models
        /// shouldn't use this contructor.
        /// </summary>
        public DataStore()
        {
        }

        /// <summary>A constructor that needs to know the calling model.</summary>
        /// <param name="ownerModel">The owner model.</param>
        /// <param name="baseline">if set to <c>true</c> [baseline].</param>
        public DataStore(Model ownerModel, bool baseline = false)
        {
            Simulation simulation = Apsim.Parent(ownerModel, typeof(Simulation)) as Simulation;
            if (simulation == null)
            {
                Simulations simulations = Apsim.Parent(ownerModel, typeof(Simulations)) as Simulations;
                if (simulations != null)
                    Filename = Path.ChangeExtension(simulations.FileName, ".db");
            }
            else
                Filename = Path.ChangeExtension(simulation.FileName, ".db");

            if (Filename != null && baseline)
                Filename += ".baseline";
        }

        /// <summary>Destructor. Close our DB connection.</summary>
        ~DataStore()
        {
            Disconnect();
        }

        /// <summary>Disconnect from the SQLite database.</summary>
        public void Disconnect()
        {
            if (Connection != null)
            {
                Connection.CloseDatabase();
                Connection = null;
            }
        }

        /// <summary>All simulations have run - write all tables</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("AllCompleted")]
        private void OnAllSimulationsCompleted(object sender, EventArgs e)
        {
            // Open the .db for writing.
            Open(forWriting: true);

            // Get a list of tables for our .db file.
            List<ReportTable> tablesForOurFile = new List<ReportTable>();
            lock (TablesToWrite)
            {
                foreach (ReportTable table in TablesToWrite)
                    if (table.FileName == Filename)
                        tablesForOurFile.Add(table);
            }

            IEnumerable<string> distinctTableNames = tablesForOurFile.Select(t => t.TableName).Distinct();

            // loop through all our tables and write them to the .db
            foreach (string tableName in distinctTableNames)
            {
                // Get a list of tables that have the same name as 'tableName'
                List<ReportTable> tables = new List<ReportTable>();
                foreach (ReportTable table in tablesForOurFile)
                    if (table.TableName == tableName)
                        tables.Add(table);

                WriteTables(tables.ToArray());

            }

            lock (TablesToWrite)
            {
                foreach (ReportTable table in tablesForOurFile)
                    TablesToWrite.Remove(table);
            }

            // Call each of the child post simulation tools allowing them to run
            RunPostProcessingTools();

            if (AutoExport)
            {
                WriteOutputToTextFiles();
                WriteSummaryToTextFiles();
            }

            // Disconnect.
            Disconnect();
        }

        /// <summary>Remove all simulations from the database that don't exist in 'simulationsToKeep'</summary>
        /// <param name="simulationsToKeep">The simulations to keep.</param>
        /// <param name="simulationNamesToBeRun">The simulation names about to be run.</param>
        public void RemoveUnwantedSimulations(Simulations simulationsToKeep, List<string> simulationNamesToBeRun)
        {
            Open(forWriting: true);

            string[] simulationNamesToKeep = simulationsToKeep.FindAllSimulationNames();


            // Tell SQLite that we're beginning a transaction.
            Connection.ExecuteNonQuery("BEGIN");

            try
            {
                // Make sure that the list of simulations in 'simulationsToKeep' are in the 
                // Simulations table.
                string[] simulationNames = this.SimulationNames;
                string sql = string.Empty;
                foreach (string simulationNameToKeep in simulationNamesToKeep)
                {
                    if (!StringUtilities.Contains(simulationNames, simulationNameToKeep))
                    {
                        if (sql != string.Empty)
                            sql += "),(";
                        sql += "'" + simulationNameToKeep + "'";
                    }
                }

                if (sql != string.Empty)
                    RunQueryWithNoReturnData("INSERT INTO [Simulations] (Name) VALUES (" + sql + ")");

                // Get a list of simulation IDs that we are to delete.
                List<int> idsToDelete = new List<int>();
                foreach (string simulationNameInDB in SimulationNames)
                    if (!simulationNamesToKeep.Contains(simulationNameInDB))
                    {
                        idsToDelete.Add(GetSimulationID(simulationNameInDB));
                    }

                // create an SQL WHERE clause with all IDs
                string idString = "";
                for (int i = 0; i < idsToDelete.Count; i++)
                {
                    if (i > 0)
                        idString += " OR ";
                    idString += "ID = " + idsToDelete[i].ToString();
                }

                if (idString != string.Empty)
                    RunQueryWithNoReturnData("DELETE FROM Simulations WHERE " + idString);

                // Now add to IDs to delete the simulations IDs of the simulations we are
                // about to run i.e. remove the rows that we are about to regenerate.
                idsToDelete.Clear();
                foreach (string simulationNameToBeRun in simulationNamesToBeRun)
                    idsToDelete.Add(GetSimulationID(simulationNameToBeRun));

                idString = "";
                for (int i = 0; i < idsToDelete.Count; i++)
                {
                    if (i > 0)
                        idString += " OR ";
                    idString += "SimulationID = " + idsToDelete[i].ToString();
                }

                foreach (string tableName in TableNames)
                {
                    // delete this simulation
                    RunQueryWithNoReturnData("DELETE FROM " + tableName + " WHERE " + idString);
                }
            }
            finally
            {
                // Tell SQLite that we're ending a transaction.
                Connection.ExecuteNonQuery("END");
            }

        }

        /// <summary>Delete all tables</summary>
        /// <param name="cleanSlate">If true, all tables are deleted; otherwise Simulations and Messages tables are retained</param>
        public void DeleteAllTables(bool cleanSlate = false)
        {
            foreach (string tableName in this.TableNames)
                if (cleanSlate || (tableName != "Simulations" && tableName != "Messages"))
                    DeleteTable(tableName);
        }

        /// <summary>Determine whether a table exists in the database</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public bool TableExists(string tableName)
        {
            Open(forWriting: false);
            
            return (Connection != null) && Connection.ExecuteQueryReturnInt("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='" + 
                                                    tableName + "'", 0) > 0;
        }

        /// <summary>Delete the specified table.</summary>
        /// <param name="tableName">Name of the table.</param>
        public void DeleteTable(string tableName)
        {
            Open(forWriting: true);
            if (TableExists(tableName))
            {
                string cmd = "DROP TABLE " + tableName;
                RunQueryWithNoReturnData(cmd);
            }
        }

        /// <summary>Create a table in the database based on the specified one.</summary>
        /// <param name="table">The table.</param>
        public void WriteTable(ReportTable table)
        {
            lock (TablesToWrite)
                TablesToWrite.Add(table);
        }

        /// <summary>Write a message to the DataStore.</summary>
        /// <param name="simulationName">Name of the simulation.</param>
        /// <param name="date">The date.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        public void WriteMessage(string simulationName, DateTime date, string componentName, string message, ErrorLevel type)
        {
            Open(forWriting: true);
            string[] names = new string[] { "ComponentName", "Date", "Message", "MessageType" };

            string sql = string.Format("INSERT INTO Messages (SimulationID, ComponentName, Date, Message, MessageType) " +
                                       "VALUES ({0}, {1}, {2}, {3}, {4})",
                                       new object[] { GetSimulationID(simulationName),
                                                      "\"" + componentName + "\"",
                                                      date.ToString("yyyy-MM-dd"),
                                                      "\"" + message + "\"",
                                                      Convert.ToInt32(type, System.Globalization.CultureInfo.InvariantCulture)});

            RunQueryWithNoReturnData(sql);
        }

        /// <summary>Return a list of simulations names or empty string[]. Never returns null.</summary>
        /// <value>The simulation names.</value>
        public string[] SimulationNames
        {
            get
            {
                if (!TableExists("Simulations"))
                    return new string[0];

                try
                {
                    DataTable table = Connection.ExecuteQuery("SELECT Name FROM Simulations ORDER BY Name");
                    if (table == null)
                        return new string[0];
                    return DataTableUtilities.GetColumnAsStrings(table, "Name");
                }
                catch (SQLiteException )
                {
                    return new string[0];
                }
            }
        }

        /// <summary>Return a list of table names or empty string[]. Never returns null.</summary>
        /// <value>The table names.</value>
        public string[] TableNames
        {
            get
            {
                try
                {
                    Open(forWriting: false);
                    if (Connection != null)
                    {
                        DataTable table = Connection.ExecuteQuery("SELECT * FROM sqlite_master");
                        List<string> tables = new List<string>();
                        if (table != null)
                        {
                            tables.AddRange(DataTableUtilities.GetColumnAsStrings(table, "Name"));

                            // remove the simulations table
                            int simulationsI = tables.IndexOf("Simulations");
                            if (simulationsI != -1)
                                tables.RemoveAt(simulationsI);
                        }
                        return tables.ToArray();
                    }
                    return new string[0];
                }
                catch (SQLiteException )
                {
                    return new string[0];
                }

            }
        }

        /// <summary>Get a list of column names for table.</summary>
        public string[] ColumnNames(string tableName)
        {
            string sql = "SELECT * FROM " + tableName + " LIMIT 1";
            DataTable data = RunQuery(sql);
            return DataTableUtilities.GetColumnNames(data);
        }

        /// <summary>
        /// Return all data from the specified simulation and table name. If simulationName = "*"
        /// the all simulation data will be returned.
        /// </summary>
        /// <param name="simulationName">Name of the simulation.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="includeSimulationName">if set to <c>true</c> [include simulation name].</param>
        /// <param name="from">Only used when 'count' specified. The record number to offset.</param>
        /// <param name="count">The number of records to return or all if 0.</param>
        /// <returns></returns>
        public DataTable GetData(string simulationName, string tableName, bool includeSimulationName = false, int from = 0, int count = 0)
        {
            Open(forWriting: false);
            if (Connection == null || !TableExists("Simulations") || tableName == null || !TableExists(tableName))
                return null;
            try
            {
                string sql;

                if (simulationName == null || simulationName == "*")
                {
                    sql = "SELECT S.Name as SimName, T.* FROM " + tableName + " T" + ", Simulations S " +
                          "WHERE SimulationID = ID";
                }
                else
                {
                    sql = "SELECT * FROM " + tableName;
                    int simulationID = GetSimulationID(simulationName);
                    sql += " WHERE SimulationID = " + simulationID.ToString();
                }

                if (count > 0)
                    sql += " LIMIT " + count + " OFFSET " + from;

                return Connection.ExecuteQuery(sql);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Return all data from the specified simulation and table name. If simulationName = "*"
        /// the all simulation data will be returned.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="from">Only used when 'count' specified. The record number to offset.</param>
        /// <param name="count">The number of records to return or all if 0.</param>
        /// <returns></returns>
        public DataTable GetFilteredData(string tableName, string filter, int from = 0, int count = 0)
        {
            return GetFilteredData(tableName, new string[] { "*" }, filter, from, count);
        }

        /// <summary>
        /// Return all data from the specified simulation and table name. If simulationName = "*"
        /// the all simulation data will be returned.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldNames">Field names to get data for.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="from">Only used when 'count' specified. The record number to offset.</param>
        /// <param name="count">The number of records to return or all if 0.</param>
        /// <returns></returns>
        public DataTable GetFilteredData(string tableName, string[] fieldNames, string filter, int from = 0, int count = 0)
        {
            Open(forWriting: false);
            if (Connection == null || !TableExists("Simulations") || tableName == null)
                return null;
            try
            {
                string sql;

                // Create a string of comma separated field names.
                string fieldNameString = string.Empty;
                foreach (string fieldName in fieldNames)
                {
                    if (fieldNameString != string.Empty)
                        fieldNameString += ",";
                    fieldNameString += "[" + fieldName + "]";
                }

                if (fieldNameString == "[*]")
                    fieldNameString = "*";

                sql = "SELECT S.Name as SimulationName, " + fieldNameString + " FROM " + tableName + " T" + ", Simulations S ";
                sql += "WHERE ID = SimulationID";
                if (filter != null)
                {
                    sql += " AND " + filter;
                }

                if (count > 0)
                    sql += " LIMIT " + count + " OFFSET " + from;

                return Connection.ExecuteQuery(sql);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Return all data from the specified simulation and table name.</summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public DataTable RunQuery(string sql)
        {
            Open(forWriting: false);

            try
            {
                return Connection.ExecuteQuery(sql);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Return all data from the specified simulation and table name.</summary>
        /// <param name="sql">The SQL.</param>
        public void RunQueryWithNoReturnData(string sql)
        {
            Open(forWriting: true);

            Locks[Filename].Aquire();
            try
            {

                Connection.ExecuteNonQuery(sql);

            }
            finally
            {
                Locks[Filename].Release();
            }
        }

        /// <summary>Remove all rows from the specified table for the specified simulation</summary>
        /// <param name="simulationName">Name of the simulation.</param>
        /// <param name="tableName">Name of the table.</param>
        public void DeleteOldContentInTable(string simulationName, string tableName)
        {
            if (TableExists(tableName))
            {
                Open(forWriting: true);
                int id = GetSimulationID(simulationName);
                string sql = "DELETE FROM " + tableName + " WHERE SimulationID = " + id.ToString();
                RunQueryWithNoReturnData(sql);
            }
        }

        /// <summary>Write all outputs to a text file (.csv)</summary>
        public void WriteOutputToTextFiles()
        {
            try
            {
                // Write the output CSV file.
                Open(forWriting: false);
                WriteAllTables(this, Filename + ".csv");
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>Write all summary to a text file (.sum)</summary>
        public void WriteSummaryToTextFiles()
        {
            try
            {
                // Write the summary file.
                Open(forWriting: false);
                WriteSummaryFile(this, Filename + ".sum");
            }
            finally
            {
                Disconnect();
            }
        }


        /// <summary>Clear all tables to be written.</summary>
        public static void ClearTablesToWritten()
        {
            TablesToWrite.Clear();
        }

        /// <summary>Store the list of factor names and values for the specified simulation.</summary>
        public void StoreFactors(string experimentName, string simulationName, string folderName, List<string> names, List<string> values)
        {
            ReportTable table = new ReportTable();
            table.FileName = Filename;
            table.TableName = "Factors";
            table.SimulationName = simulationName;
            table.Columns.Add(new ReportColumnConstantValue("ExperimentName", experimentName));
            table.Columns.Add(new ReportColumnConstantValue("SimulationName", simulationName));
            table.Columns.Add(new ReportColumnConstantValue("FolderName", folderName));
            table.Columns.Add(new ReportColumnWithValues("FactorName", names.ToArray()));
            table.Columns.Add(new ReportColumnWithValues("FactorValue", values.ToArray()));
            WriteTable(table);
        }

        /// <summary>Write a single summary file.</summary>
        /// <param name="dataStore">The data store containing the data</param>
        /// <param name="fileName">The file name to create</param>
        private static void WriteSummaryFile(DataStore dataStore, string fileName)
        {
            StreamWriter report = new StreamWriter(fileName);
            foreach (string simulationName in dataStore.SimulationNames)
            {
                Summary.WriteReport(dataStore, simulationName, report, null, outtype: Summary.OutputType.html);
                report.WriteLine();
                report.WriteLine();
                report.WriteLine("############################################################################");
            }
            report.Close();
        }

        /// <summary>Run all post processing tools.</summary>
        public void RunPostProcessingTools()
        {
            // Open the .db for writing.
            Open(forWriting: true);

            foreach (IPostSimulationTool tool in Apsim.Children(this, typeof(IPostSimulationTool)))
                tool.Run(this);
        }

        #region Privates

        /// <summary>Connect to the SQLite database.</summary>
        /// <param name="forWriting">if set to <c>true</c> [for writing].</param>
        /// <exception cref="Models.Core.ApsimXException">Cannot find name of .db file</exception>
        private void Open(bool forWriting)
        {
            lock (Locks)
            {
                if (Filename == null)
                {
                    Simulations simulations = Apsim.Parent(this, typeof(Simulations)) as Simulations;
                    if (simulations != null)
                        Filename = Path.ChangeExtension(simulations.FileName, ".db");
                }

                if (Filename != null && 
                    (Connection == null || 
                    (ForWriting == false && forWriting == true)))
                {
                    if (Filename == null)
                        throw new ApsimXException(this, "Cannot find name of .db file");

                    Disconnect();

                    ForWriting = forWriting;

                    if (!Locks.ContainsKey(Filename))
                        Locks.Add(Filename, new DbMutex());

                    Locks[Filename].Aquire();
                    try
                    {
                        if (!File.Exists(Filename))
                        {
                            Connection = new SQLite();
                            Connection.OpenDatabase(Filename, readOnly: false);
                            Connection.ExecuteNonQuery("CREATE TABLE Simulations (ID INTEGER PRIMARY KEY ASC, Name TEXT COLLATE NOCASE)");
                            Connection.ExecuteNonQuery("CREATE TABLE Messages (SimulationID INTEGER, ComponentName TEXT, Date TEXT, Message TEXT, MessageType INTEGER)");

                            if (!forWriting)
                            {
                                Connection.CloseDatabase();
                                Connection.OpenDatabase(Filename, readOnly: !forWriting);
                            }
                        }
                        else
                        {
                            Connection = new SQLite();
                            Connection.OpenDatabase(Filename, readOnly: !forWriting);
                        }

                    }
                    finally
                    {
                        Locks[Filename].Release();
                    }


                }
            }
        }

        /// <summary>Go create a table in the DataStore with the specified field names and types.</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="names">The names.</param>
        /// <param name="types">The types.</param>
        private void CreateTable(string tableName, string[] names, Type[] types)
        {
            Open(forWriting: true);

            string cmd = "CREATE TABLE " + tableName + "(";

            for (int i = 0; i < names.Length; i++)
            {
                string columnType = null;
                columnType = GetSQLColumnType(types[i]);

                if (i != 0)
                    cmd += ",";
                cmd += "[" + names[i] + "] " + columnType;
            }
            cmd += ")";
            Locks[Filename].Aquire();

            try
            {
                if (!TableExists(tableName))
                    Connection.ExecuteNonQuery(cmd);
                else
                    AddMissingColumnsToTable(Connection, tableName, names, types);
            }
            finally
            {
                Locks[Filename].Release();
            }
        }

        /// <summary>Create a table in the database based on the specified one.</summary>
        /// <param name="simulationName">Name of the simulation.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="table">The table.</param>
        public void WriteTable(string simulationName, string tableName, DataTable table)
        {
            if (table != null)
            {
                // Open the .db for writing.
                Open(forWriting: true);

                // Get a list of all names and datatypes for each field in this table.
                List<string> names = new List<string>();
                List<Type> types = new List<Type>();
                names.Add("SimulationID");
                types.Add(typeof(int));

                int simulationID = int.MaxValue;

                // If the table has a simulationname then go find its ID for later
                if (table.Columns.Contains("SimulationID"))
                {
                    // do nothing.
                }
                else if (simulationName != null)
                    simulationID = GetSimulationID(simulationName);
                else if (table.Columns.Contains("SimulationName"))
                    AddSimulationIDColumnToTable(table);

                // Go through all columns for this table and add to 'names' and 'types'
                foreach (DataColumn column in table.Columns)
                {
                    if (!names.Contains(column.ColumnName) && column.ColumnName != "SimulationName")
                    {
                        names.Add(column.ColumnName);
                        types.Add(column.DataType);
                    }
                }

                // Create the table.
                CreateTable(tableName, names.ToArray(), types.ToArray());

                // Prepare the insert query sql
                IntPtr query = PrepareInsertIntoTable(Connection, tableName, names.ToArray());

                // Tell SQLite that we're beginning a transaction.
                Connection.ExecuteNonQuery("BEGIN");

                try
                {
                    // Write each row to the .db
                    if (table != null)
                    {
                        object[] values = new object[names.Count];
                        foreach (DataRow row in table.Rows)
                        {
                            for (int i = 0; i < names.Count; i++)
                            {
                                if (names[i] == "SimulationID" && simulationID != int.MaxValue)
                                    values[i] = simulationID;
                                else if (table.Columns.Contains(names[i]))
                                    values[i] = row[names[i]];
                            }

                            // Write the row to the .db
                            Connection.BindParametersAndRunQuery(query, values);
                        }
                    }
                }
                finally
                {
                    // tell SQLite we're ending our transaction.
                    Connection.ExecuteNonQuery("END");

                    // finalise our query.
                    Connection.Finalize(query);
                }
            }
        }

        /// <summary>
        /// Write the specified tables to a single table in the DB. i.e. merge
        /// all columns and rows in all specified tables into a single table.
        /// </summary>
        /// <param name="tables">The tables.</param>
        private void WriteTables(ReportTable[] tables)
        {
            // Insert simulationID column into all tables.
            foreach (ReportTable table in tables)
            {
                int simulationID = GetSimulationID(table.SimulationName);
                table.Columns.Insert(0, new ReportColumnConstantValue("SimulationID", simulationID));
            }

            // Get a list of all names and datatypes for each field in this table.
            List<string> names = new List<string>();
            List<Type> types = new List<Type>();
            foreach (ReportTable table in tables)
                foreach (IReportColumn column in table.Columns)
                {
                    if (!names.Contains(column.Name))
                    {
                        object firstNonBlankValue = column.Values.Find(value => value != null);
                        if (firstNonBlankValue != null)
                        {
                            names.Add(column.Name);
                            types.Add(firstNonBlankValue.GetType());
                        }
                    }
                }

            // Open the .db for writing.
            Open(forWriting: true);

            // Create the table.
            string tableName = tables[0].TableName;
            CreateTable(tableName, names.ToArray(), types.ToArray());

            // Prepare the insert query sql
            IntPtr query = PrepareInsertIntoTable(Connection, tableName, names.ToArray());

            // Tell SQLite that we're beginning a transaction.
            Connection.ExecuteNonQuery("BEGIN");

            try
            {
                // Write each row to the .db
                foreach (ReportTable table in tables)
                {
                    int numRows = 0;

                    // Create an array of value indexes for column.
                    int[] valueIndexes = new int[table.Columns.Count];
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        numRows = Math.Max(numRows, table.Columns[i].Values.Count);
                        valueIndexes[i] = names.IndexOf(table.Columns[i].Name);
                    }

                    object[] values = new object[names.Count];
                    for (int rowIndex = 0; rowIndex < numRows; rowIndex++)
                    {
                        Array.Clear(values, 0, values.Length);
                        for (int colIndex = 0; colIndex < table.Columns.Count; colIndex++)
                        {
                            int valueIndex = valueIndexes[colIndex];
                            if (valueIndex != -1)
                            {
                                if (table.Columns[colIndex] is ReportColumnConstantValue)
                                    values[valueIndex] = table.Columns[colIndex].Values[0];
                                else if (rowIndex < table.Columns[colIndex].Values.Count)
                                    values[valueIndex] = table.Columns[colIndex].Values[rowIndex];
                            }
                        }

                        // Write the row to the .db
                        Connection.BindParametersAndRunQuery(query, values.ToArray());
                    }
                }
            }
            finally
            {
                // tell SQLite we're ending our transaction.
                Connection.ExecuteNonQuery("END");

                // finalise our query.
                Connection.Finalize(query);
            }
        }

        /// <summary>
        /// Return the simulation id (from the simulations table) for the specified name.
        /// If this name doesn't exist in the table then append a new row to the table and
        /// returns its id.
        /// </summary>
        /// <param name="simulationName">Name of the simulation.</param>
        /// <returns></returns>
        public int GetSimulationID(string simulationName)
        {
            if (!TableExists("Simulations"))
                return -1;

            string selectSQL = "SELECT ID FROM Simulations WHERE Name = '" + simulationName + "'";
            int ID = Connection.ExecuteQueryReturnInt(selectSQL, 0);
            if (ID == -1)
            {
                Locks[Filename].Aquire();
                ID = Connection.ExecuteQueryReturnInt(selectSQL, 0);
                if (ID == -1)
                {
                    if (ForWriting == false)
                    {
                        Disconnect();
                        Connection = new SQLite();
                        Connection.OpenDatabase(Filename, readOnly: false);
                        ForWriting = true;
                    }
                    Connection.ExecuteNonQuery("INSERT INTO [Simulations] (Name) VALUES ('" + simulationName + "')");
                    ID = Connection.ExecuteQueryReturnInt("SELECT ID FROM Simulations WHERE Name = '" + simulationName + "'", 0);
                }

                Locks[Filename].Release();
            }
            return ID;
        }

        /// <summary>Create a text report from tables in this data store.</summary>
        /// <param name="dataStore">The data store.</param>
        /// <param name="fileName">Name of the file.</param>
        private static void WriteAllTables(DataStore dataStore, string fileName)
        {
            
            // Write out each table for this simulation.
            foreach (string tableName in dataStore.TableNames)
            {
                if (tableName != "Messages" && tableName != "InitialConditions")
                {
                    DataTable firstRowOfTable = dataStore.RunQuery("SELECT * FROM " + tableName + " LIMIT 1");
                    if (firstRowOfTable != null)
                    {
                        string fieldNamesString = "";
                        for (int i = 1; i < firstRowOfTable.Columns.Count; i++)
                        {
                            if (i > 1)
                                fieldNamesString += ", ";
                            fieldNamesString += "[" + firstRowOfTable.Columns[i].ColumnName + "]";
                        }

                        string sql = String.Format("SELECT Name, {0} FROM Simulations, {1} " +
                                                   "WHERE Simulations.ID = {1}.SimulationID " +
                                                   "ORDER BY Name",
                                                   fieldNamesString, tableName);
                        DataTable data = dataStore.RunQuery(sql);
                        if (data != null && data.Rows.Count > 0)
                        {
                            StreamWriter report = new StreamWriter(Path.ChangeExtension(fileName, "." + tableName + ".csv"));
                            DataTableUtilities.DataTableToText(data, 0, ",", true, report);
                            report.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Go through the specified names and add them to the specified table if they are not
        /// already there.
        /// </summary>
        /// <param name="Connection">The connection.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="names">The names.</param>
        /// <param name="types">The types.</param>
        private static void AddMissingColumnsToTable(SQLite Connection, string tableName, string[] names, Type[] types)
        {
            List<string> columnNames = Connection.GetColumnNames(tableName);

            for (int i = 0; i < names.Length; i++)
            {
                if (!columnNames.Contains(names[i], StringComparer.OrdinalIgnoreCase))
                {
                    string sql = "ALTER TABLE " + tableName + " ADD COLUMN [";
                    sql += names[i] + "] " + GetSQLColumnType(types[i]);
                    Connection.ExecuteNonQuery(sql);    
                }
            }
        }

        /// <summary>Go prepare an insert into query and return the query.</summary>
        /// <param name="Connection">The connection.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="names">The names.</param>
        /// <returns></returns>
        private static IntPtr PrepareInsertIntoTable(SQLite Connection, string tableName, string[] names)
        {
            string Cmd = "INSERT INTO " + tableName + "(";

            for (int i = 0; i < names.Length; i++)
            {
                if (i > 0)
                    Cmd += ",";
                Cmd += "[" + names[i] + "]";
            }
            Cmd += ") VALUES (";

            for (int i = 0; i < names.Length; i++)
            {
                if (i > 0)
                    Cmd += ",";
                Cmd += "?";
            }
            Cmd += ")";
            return Connection.Prepare(Cmd);
        }

        /// <summary>
        /// Using the SimulationName column in the specified 'table', add a
        /// SimulationID column.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <exception cref="Models.Core.ApsimXException">Cannot find Simulations table</exception>
        private void AddSimulationIDColumnToTable(DataTable table)
        {
            // Get a list of simulations that are in the DB
            DataTable DB = Connection.ExecuteQuery("SELECT * FROM Simulations");
            List<string> simulationNamesInDB = DataTableUtilities.GetColumnAsStrings(DB, "Name").ToList();

            // Tell SQLite that we're beginning a transaction.
            Connection.ExecuteNonQuery("BEGIN");

            try
            {
                // For those simulations in 'table' that aren't in the DB, add them
                // to the simulations table
                List<string> simulationNamesInTable = DataTableUtilities.GetDistinctValues(table, "SimulationName");
                foreach (string simulationNameInTable in simulationNamesInTable)
                {
                    if (!StringUtilities.Contains(simulationNamesInDB, simulationNameInTable))
                        RunQueryWithNoReturnData("INSERT INTO [Simulations] (Name) VALUES ('" + simulationNameInTable + "')");
                }
            }
            finally
            {
                // Tell SQLite that we're ending a transaction.
                Connection.ExecuteNonQuery("END");
            }

            // Get a list of simulation names and IDs from DB
            DB = Connection.ExecuteQuery("SELECT * FROM Simulations");
            List<double> ids = DataTableUtilities.GetColumnAsDoubles(DB, "ID").ToList();
            simulationNamesInDB = DataTableUtilities.GetColumnAsStrings(DB, "Name").ToList();

            table.Columns.Add("SimulationID", typeof(int)).SetOrdinal(0);
            foreach (DataRow row in table.Rows)
            {
                string simulationName = row["SimulationName"].ToString();
                if (simulationName != null)
                {
                    int index = StringUtilities.IndexOfCaseInsensitive(simulationNamesInDB, simulationName);
                    if (index == -1)
                        throw new Exception("Cannot find simulation name: " + simulationName);
                    else
                        row["SimulationID"] = ids[index];
                }
            }
        }

        /// <summary>Convert the specified type to a SQL type.</summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static string GetSQLColumnType(Type type)
        {
            if (type == null)
                return "integer";
            else if (type.ToString() == "System.DateTime")
                return "date";
            else if (type.ToString() == "System.Int32")
                return "integer";
            else if (type.ToString() == "System.Single")
                return "real";
            else if (type.ToString() == "System.Double")
                return "real";
            else
                return "char(50)";
        }
        #endregion
    }
}

