// -----------------------------------------------------------------------
// <copyright file="SQLite.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace APSIM.Shared.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.InteropServices;

    /// <summary>A class representing an exception thrown by this library.</summary>
    public class SQLiteException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="SQLiteException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public SQLiteException(string message) :
            base(message)
        {

        }
    }

    /// <summary>A class for accessing an SQLite database.</summary>
    [Serializable]
    public class SQLite
    {
        /// <summary>The sqlit e_ ok</summary>
        private const int SQLITE_OK = 0;
        /// <summary>The sqlit e_ row</summary>
        private const int SQLITE_ROW = 100;
        /// <summary>The sqlit e_ done</summary>
        private const int SQLITE_DONE = 101;
        /// <summary>The sqlit e_ integer</summary>
        private const int SQLITE_INTEGER = 1;
        /// <summary>The sqlit e_ float</summary>
        private const int SQLITE_FLOAT = 2;
        /// <summary>The sqlit e_ text</summary>
        private const int SQLITE_TEXT = 3;
        /// <summary>The sqlit e_ BLOB</summary>
        private const int SQLITE_BLOB = 4;
        /// <summary>The sqlit e_ null</summary>
        private const int SQLITE_NULL = 5;

        /// <summary>The sqlit e_ ope n_ readonly</summary>
        private const int SQLITE_OPEN_READONLY       =   0x00000001;  /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ readwrite</summary>
        private const int SQLITE_OPEN_READWRITE      =   0x00000002; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ create</summary>
        private const int SQLITE_OPEN_CREATE         =   0x00000004; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ deleteonclose</summary>
        private const int SQLITE_OPEN_DELETEONCLOSE  =   0x00000008; /* VFS only */
        /// <summary>The sqlit e_ ope n_ exclusive</summary>
        private const int SQLITE_OPEN_EXCLUSIVE      =   0x00000010; /* VFS only */
        /// <summary>The sqlit e_ ope n_ autoproxy</summary>
        private const int SQLITE_OPEN_AUTOPROXY      =   0x00000020; /* VFS only */
        /// <summary>The sqlit e_ ope n_ URI</summary>
        private const int SQLITE_OPEN_URI            =   0x00000040; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ memory</summary>
        private const int SQLITE_OPEN_MEMORY         =   0x00000080; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ mai n_ database</summary>
        private const int SQLITE_OPEN_MAIN_DB        =   0x00000100; /* VFS only */
        /// <summary>The sqlit e_ ope n_ tem p_ database</summary>
        private const int SQLITE_OPEN_TEMP_DB        =   0x00000200; /* VFS only */
        /// <summary>The sqlit e_ ope n_ transien t_ database</summary>
        private const int SQLITE_OPEN_TRANSIENT_DB   =   0x00000400; /* VFS only */
        /// <summary>The sqlit e_ ope n_ mai n_ journal</summary>
        private const int SQLITE_OPEN_MAIN_JOURNAL   =   0x00000800; /* VFS only */
        /// <summary>The sqlit e_ ope n_ tem p_ journal</summary>
        private const int SQLITE_OPEN_TEMP_JOURNAL   =   0x00001000; /* VFS only */
        /// <summary>The sqlit e_ ope n_ subjournal</summary>
        private const int SQLITE_OPEN_SUBJOURNAL     =   0x00002000; /* VFS only */
        /// <summary>The sqlit e_ ope n_ maste r_ journal</summary>
        private const int SQLITE_OPEN_MASTER_JOURNAL =   0x00004000; /* VFS only */
        /// <summary>The sqlit e_ ope n_ nomutex</summary>
        private const int SQLITE_OPEN_NOMUTEX        =   0x00008000; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ fullmutex</summary>
        private const int SQLITE_OPEN_FULLMUTEX      =   0x00010000; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ sharedcache</summary>
        private const int SQLITE_OPEN_SHAREDCACHE    =   0x00020000; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ privatecache</summary>
        private const int SQLITE_OPEN_PRIVATECACHE   =   0x00040000; /* Ok for sqlite3_open_v2() */
        /// <summary>The sqlit e_ ope n_ wal</summary>
        private const int SQLITE_OPEN_WAL            =   0x00080000; /* VFS only */

        #region Externals
        //When using sqlite3 without .dll the platforms are intelligent enough to add the OS specific details.
        //On Linux-Mono the lib .so artifacts appear to be accounted for.
        /// <summary>Sqlite3_opens the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_open", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_open(string filename, out IntPtr db);

        /// <summary>Sqlite3_open_v2s the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="db">The database.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="zVfs">The z VFS.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_open_v2(string filename, out IntPtr db, int flags, string zVfs);

        /// <summary>Sqlite3_closes the specified database.</summary>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_close", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_close(IntPtr db);

        /// <summary>Sqlite3_prepare_v2s the specified database.</summary>
        /// <param name="db">The database.</param>
        /// <param name="zSql">The z SQL.</param>
        /// <param name="nByte">The n byte.</param>
        /// <param name="ppStmpt">The pp STMPT.</param>
        /// <param name="pzTail">The pz tail.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_prepare_v2(IntPtr db, string zSql,
            int nByte, out IntPtr ppStmpt, IntPtr pzTail);

        /// <summary>Sqlite3_steps the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_step(IntPtr stmHandle);

        /// <summary>Sqlite3_finalizes the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_finalize(IntPtr stmHandle);

        /// <summary>Sqlite3_errmsgs the specified database.</summary>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_errmsg", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_errmsg(IntPtr db);

        /// <summary>Sqlite3_column_counts the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_count(IntPtr stmHandle);

        /// <summary>Sqlite3_column_names the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <param name="iCol">The i col.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_column_name", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_column_name(IntPtr stmHandle, int iCol);

        /// <summary>Sqlite3_column_types the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <param name="iCol">The i col.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_type(IntPtr stmHandle, int iCol);

        /// <summary>Sqlite3_column_ints the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <param name="iCol">The i col.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_column_int", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_int(IntPtr stmHandle, int iCol);

        /// <summary>Sqlite3_column_texts the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <param name="iCol">The i col.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_column_text(IntPtr stmHandle, int iCol);

        /// <summary>Sqlite3_column_doubles the specified STM handle.</summary>
        /// <param name="stmHandle">The STM handle.</param>
        /// <param name="iCol">The i col.</param>
        /// <returns></returns>
        [DllImport("sqlite3", EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
        static extern double sqlite3_column_double(IntPtr stmHandle, int iCol);

        /// <summary>Sqlite3_bind_doubles the specified query.</summary>
        /// <param name="Query">The query.</param>
        /// <param name="ParameterNumber">The parameter number.</param>
        /// <param name="Value">The value.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern string sqlite3_bind_double(IntPtr Query, int ParameterNumber, double Value);

        /// <summary>Sqlite3_bind_ints the specified query.</summary>
        /// <param name="Query">The query.</param>
        /// <param name="ParameterNumber">The parameter number.</param>
        /// <param name="Value">The value.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern string sqlite3_bind_int(IntPtr Query, int ParameterNumber, int Value);

        /// <summary>Sqlite3_bind_nulls the specified query.</summary>
        /// <param name="Query">The query.</param>
        /// <param name="ParameterNumber">The parameter number.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern string sqlite3_bind_null(IntPtr Query, int ParameterNumber);

        /// <summary>Sqlite3_bind_texts the specified query.</summary>
        /// <param name="Query">The query.</param>
        /// <param name="ParameterNumber">The parameter number.</param>
        /// <param name="Value">The value.</param>
        /// <param name="n">The n.</param>
        /// <param name="CallBack">The call back.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_bind_text(IntPtr Query, int ParameterNumber, string Value, int n, IntPtr CallBack);

        /// <summary>Sqlite3_resets the specified query.</summary>
        /// <param name="Query">The query.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_reset(IntPtr Query);

        /// <summary>Sqlite3_threadsafes this instance.</summary>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_threadsafe();

        /// <summary>Sqlite3_busy_timeouts the specified database.</summary>
        /// <param name="db">The database.</param>
        /// <param name="ms">The ms.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_busy_timeout(IntPtr db, int ms);

        /// <summary>Sqlite3_db_mutexes the specified database.</summary>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_db_mutex(IntPtr db);

        /// <summary>Sqlite3_mutex_enters the specified sqlite3_mutex.</summary>
        /// <param name="sqlite3_mutex">The sqlite3_mutex.</param>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sqlite3_mutex_enter(IntPtr sqlite3_mutex);

        /// <summary>Sqlite3_mutex_leaves the specified sqlite3_mutex.</summary>
        /// <param name="sqlite3_mutex">The sqlite3_mutex.</param>
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sqlite3_mutex_leave(IntPtr sqlite3_mutex);
        #endregion

        /// <summary>The _DB</summary>
        [NonSerialized]
        private IntPtr _db; //pointer to SQLite database
        /// <summary>The _open</summary>
        [NonSerialized]
        private bool _open; //whether or not the database is open

        /// <summary>Property to return true if the database is open.</summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        public bool IsOpen { get { return _open; } }

        /// <summary>Opens or creates SQLite database with the specified path</summary>
        /// <param name="path">Path to SQLite database</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        /// <exception cref="SQLiteException"></exception>
        public void OpenDatabase(string path, bool readOnly)
        {
            int id;
            if (readOnly)
                id = sqlite3_open_v2(path, out _db, SQLITE_OPEN_READONLY | SQLITE_OPEN_NOMUTEX, null);
            else
                id = sqlite3_open_v2(path, out _db, SQLITE_OPEN_READWRITE | SQLITE_OPEN_NOMUTEX | SQLITE_OPEN_CREATE, null);

            if (id != SQLITE_OK)
            {
                string errorMessage = Marshal.PtrToStringAnsi(sqlite3_errmsg(_db));
                throw new SQLiteException(errorMessage);
            }

            _open = true;
            sqlite3_busy_timeout(_db, 40000);
        }

        /// <summary>Closes the SQLite database</summary>
        public void CloseDatabase()
        {
            if (_open)
                sqlite3_close(_db);

            _open = false;
        }

        /// <summary>Executes a query that returns no results</summary>
        /// <param name="query">SQL query to execute</param>
        public void ExecuteNonQuery(string query)
        {
            if (!_open)
                throw new SQLiteException("SQLite database is not open.");

            //prepare the statement
            IntPtr stmHandle = Prepare(query);

            int code = sqlite3_step(stmHandle);
            if (code != SQLITE_DONE)
            {
                string errorMessage = Marshal.PtrToStringAnsi(sqlite3_errmsg(_db));
                throw new SQLiteException(errorMessage);
            }

            Finalize(stmHandle);
        }

        private class Column
        {
            public string name;
            public Type dataType;
            public List<object> values = new List<object>();

            public void addIntValue(int value)
            {
                if (dataType == null)
                    dataType = typeof(int);
                values.Add(value);
            }

            public void addDoubleValue(double value)
            {
                if (dataType == null || dataType == typeof(int))
                    dataType = typeof(double);
                values.Add(value);
            }
            public void addTextValue(string value)
            {
                DateTime date;
                if (DateTime.TryParseExact(value, "yyyy-MM-dd hh:mm:ss", null, System.Globalization.DateTimeStyles.None, out date))
                {
                    if (dataType == null)
                        dataType = typeof(DateTime);
                    values.Add(date);
                }
                else
                {
                    dataType = typeof(string);
                    values.Add(value);
                }
            }
            public void addNull()
            {
                values.Add(null);
            }

            internal object GetValue(int rowIndex)
            {
                if (rowIndex >= values.Count)
                    throw new Exception("Not enough values found when creating DataTable from SQLITE query.");
                if (values[rowIndex] == null)
                    return DBNull.Value;
                else if (dataType == typeof(int))
                    return Convert.ToInt32(values[rowIndex]);
                else if (dataType == typeof(double))
                    return Convert.ToDouble(values[rowIndex]);
                else if (dataType == typeof(DateTime))
                    return Convert.ToDateTime(values[rowIndex]);
                else
                {
                    if (values[rowIndex].GetType() == typeof(DateTime))
                        return Convert.ToDateTime(values[rowIndex]).ToString("yyyy-MM-dd hh:mm:ss");
                    return values[rowIndex].ToString();
                }

            }
        }


        /// <summary>
        /// Executes a query and stores the results in
        /// a DataTable
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <returns>DataTable of results</returns>
        public System.Data.DataTable ExecuteQuery(string query)
        {
            if (!_open)
                throw new SQLiteException("SQLite database is not open.");

            //prepare the statement
            IntPtr stmHandle = Prepare(query);

            //get the number of returned columns
            int columnCount = sqlite3_column_count(stmHandle);

            // Create a datatable that may have column of type object. This occurs
            // when the first row of a table has null values.
            List<Column> columns = new List<Column>();
            while (sqlite3_step(stmHandle) == SQLITE_ROW)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    if (i >= columns.Count)
                    {
                        // add a new column
                        string columnName = Marshal.PtrToStringAnsi(sqlite3_column_name(stmHandle, i));
                        columns.Add(new Column() { name = columnName });
                    }

                    int sqliteType = sqlite3_column_type(stmHandle, i);

                    if (sqliteType == SQLITE_INTEGER)
                        columns[i].addIntValue(sqlite3_column_int(stmHandle, i));
                    else if (sqliteType == SQLITE_FLOAT)
                        columns[i].addDoubleValue(sqlite3_column_double(stmHandle, i));
                    else if (sqliteType == SQLITE_TEXT)
                    {
                        IntPtr iptr = sqlite3_column_text(stmHandle, i);
                        columns[i].addTextValue(Marshal.PtrToStringAnsi(iptr));
                    }
                    else
                        columns[i].addNull();
                }
            }

            Finalize(stmHandle);

            // At this point we have a list of columns, each with values for each row.
            // Need to convert this to a DataTable.

            DataTable table = new DataTable();
            if (columns.Count > 0)
            {
                foreach (Column column in columns)
                {
                    if (column.dataType == null)
                        table.Columns.Add(column.name, typeof(object));
                    else
                        table.Columns.Add(column.name, column.dataType);
                }

                for (int row = 0; row != columns[0].values.Count; row++)
                {
                    DataRow newRow = table.NewRow();
                    for (int col = 0; col != columns.Count; col++)
                        newRow[col] = columns[col].GetValue(row);
                    table.Rows.Add(newRow);
                }
            }
            return table;
        }

        /// <summary>
        /// Executes a query and return a single integer value to caller. Returns -1 if not found.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ColumnNumber">The column number.</param>
        /// <returns></returns>
        public int ExecuteQueryReturnInt(string query, int ColumnNumber)
        {
            if (!_open)
                throw new SQLiteException("SQLite database is not open.");

            //prepare the statement
            IntPtr stmHandle = Prepare(query);

            //get the number of returned columns
            int columnCount = sqlite3_column_count(stmHandle);

            int ReturnValue = -1;
            if (sqlite3_step(stmHandle) == SQLITE_ROW && ColumnNumber < columnCount)
                ReturnValue = sqlite3_column_int(stmHandle, ColumnNumber);

            Finalize(stmHandle);
            return ReturnValue;
        }

        /// <summary>Prepares a SQL statement for execution</summary>
        /// <param name="query">SQL query</param>
        /// <returns>Pointer to SQLite prepared statement</returns>
        public IntPtr Prepare(string query)
        {
            IntPtr stmHandle;

            if (sqlite3_prepare_v2(_db, query, query.Length,
                  out stmHandle, IntPtr.Zero) != SQLITE_OK)
                throw new SQLiteException( Marshal.PtrToStringAnsi(sqlite3_errmsg(_db)));

            return stmHandle;
        }

        /// <summary>Finalizes a SQLite statement</summary>
        /// <param name="stmHandle">Pointer to SQLite prepared statement</param>
        public void Finalize(IntPtr stmHandle)
        {
            int code = sqlite3_finalize(stmHandle);
            if (code != SQLITE_OK)
            {
                string errorMessage = Marshal.PtrToStringAnsi(sqlite3_errmsg(_db));
                throw new SQLiteException(errorMessage);
            }
        }

        /// <summary>Bind all parameters values to the specified query and execute the query.</summary>
        /// <param name="query">The query.</param>
        /// <param name="values">The values.</param>
        public void BindParametersAndRunQuery(IntPtr query, object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (Convert.IsDBNull(values[i]) || values[i] == null)
                {
                    sqlite3_bind_null(query, i+1);
                }
                else if (values[i].GetType().ToString() == "System.DateTime")
                {
                    DateTime d = (DateTime)values[i];
                    sqlite3_bind_text(query, i + 1, d.ToString("yyyy-MM-dd hh:mm:ss"), -1, new IntPtr(-1));
                }
                else if (values[i].GetType().ToString() == "System.Int32")
                {
                    int integer = (int)values[i];
                    sqlite3_bind_int(query, i + 1, integer);
                }
                else if (values[i].GetType().ToString() == "System.Single")
                {
                    float f = (float)values[i];
                    sqlite3_bind_double(query, i + 1, f);
                }
                else if (values[i].GetType().ToString() == "System.Double")
                {
                    double d = (double)values[i];
                    sqlite3_bind_double(query, i + 1, d);
                }
                else
                {
                    sqlite3_bind_text(query, i + 1, values[i] as string, -1, new IntPtr(-1));

                }

            }

            if (sqlite3_step(query) != SQLITE_DONE)
            {
                string errorMessage = Marshal.PtrToStringAnsi(sqlite3_errmsg(_db));
                throw new SQLiteException(errorMessage);
            }
            sqlite3_reset(query);
        }

        /// <summary>Return a list of column names.</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public List<string> GetColumnNames(string tableName)
        {
            string sql = "select * from "+tableName+" LIMIT 0";

            //prepare the statement
            IntPtr stmHandle = Prepare(sql);

            //get the number of returned columns
            int columnCount = sqlite3_column_count(stmHandle);

            List<string> columnNames = new List<string>();
            for(int i = 0; i < columnCount; i++)
            {
                string columnName = Marshal.PtrToStringAnsi(sqlite3_column_name(stmHandle, i));
                columnNames.Add(columnName);    
            }

            Finalize(stmHandle);
            return columnNames;
        }

        /// <summary>Lock the mutex</summary>
        public void MutexEnter()
        {
            sqlite3_mutex_enter(sqlite3_db_mutex(_db));
        }

        /// <summary>Unlock the mutex</summary>
        public void MutexLeave()
        {
            sqlite3_mutex_leave(sqlite3_db_mutex(_db));
        }
    }
}