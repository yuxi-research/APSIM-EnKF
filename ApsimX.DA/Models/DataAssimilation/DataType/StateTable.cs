using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Serialization;
using Models.Core;
using System.Threading;

namespace Models.DataAssimilation.DataType
{
    /// <summary> A Table to store states. </summary>
    [Serializable]
    public class StateTable : Model
    {
        /// <summary> The table Name. </summary>
        [XmlIgnore]
        public string TableName { get; set; }

        /// <summary>
        /// A DataTable to store states.
        /// </summary>
        [XmlIgnore]
        public DataTable Table { get; set; }

        /// <summary>
        /// The column names.
        /// </summary>
        [XmlIgnore]
        public List<string> ColumnNames { get; set; }   //May not necessary.

        /// <summary> Constructor. </summary>
        public StateTable() { }

        /// <summary> Constructor. </summary>
        /// <param name="tableName"></param>
        /// <param name="ensembleSize"></param>
        public StateTable(string tableName, int ensembleSize)
        {
            TableName = tableName;
            Table = new DataTable();
            ColumnNames = new List<string>();

            DataColumn dc;

            //Add ID, Date and DayOfYear.
            dc = new DataColumn("ID", typeof(int));     //Set this as primary key.
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            dc = new DataColumn("Date", typeof(DateTime));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            dc = new DataColumn("DOY", typeof(int));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            //Add truth and openloop.
            dc = new DataColumn("Truth", typeof(double));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            dc = new DataColumn("PriorOpenLoop", typeof(double));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            //Add prior ensembles.
            for (int i = 0; i < ensembleSize; i++)
            {
                dc = new DataColumn("PriorEnsemble" + i.ToString(), typeof(double));
                Table.Columns.Add(dc);
                ColumnNames.Add(dc.ColumnName);
            }

            //Prior ensemble mean.
            dc = new DataColumn("PriorMean", typeof(double));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            //Posterior Open Loop.
            dc = new DataColumn("PosteriorOpenLoop", typeof(double));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            //Add posterior ensembles.
            for (int i = 0; i < ensembleSize; i++)
            {
                dc = new DataColumn("PosteriorEnsemble" + i.ToString(), typeof(double));
                Table.Columns.Add(dc);
                ColumnNames.Add(dc.ColumnName);
            }

            //Posterior ensemble mean.
            dc = new DataColumn("PosteriorMean", typeof(double));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);

            //Add Observation ensembles.
            for (int i = 0; i < ensembleSize; i++)
            {
                dc = new DataColumn("ObsEnsemble" + i.ToString(), typeof(double));
                Table.Columns.Add(dc);
                ColumnNames.Add(dc.ColumnName);
            }

            //Observation.
            dc = new DataColumn("Obs", typeof(double));
            Table.Columns.Add(dc);
            ColumnNames.Add(dc.ColumnName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        public void NewRow(int id, DateTime date)
        {
            //Insert a new row on the begining of the day.
            DataRow dr = Table.NewRow();
            Table.Rows.Add(dr);
            Table.Rows[id]["ID"] = id;
            Table.Rows[id]["Date"] = date;
            Table.Rows[id]["DOY"] = date.DayOfYear;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ensembleSize"></param>
        /// <returns></returns>
        public double[] ReadPrior(int id, int ensembleSize)
        {
            string rowStr;
            double[] row = new double[ensembleSize];
            for (int i = 0; i < row.Count(); i++)
            {
                rowStr = Table.Rows[id]["PriorEnsemble" + i.ToString()].ToString();
                row[i] = Convert.ToDouble(rowStr);
            }
            return row;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double ReadOpenLoop(int id)
        {
            string rowStr;
            double row;
            rowStr = Table.Rows[id]["PriorOpenLoop"].ToString();
            row = Convert.ToDouble(rowStr);
            return row;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="states"></param>
        /// <param name="DAOption"></param>
        /// <param name="tableIndex"></param>
        /// <param name="ensembleSize"></param>
        public void InsertPosterior(int rowIndex, StatesOfTheDay states, string DAOption, int tableIndex, int ensembleSize)
        {
            if (Table.Rows[rowIndex]["ID"] != null)
            {
                Table.Rows[rowIndex]["PriorMean"] = states.PriorMean[tableIndex];
                Table.Rows[rowIndex]["PosteriorOpenLoop"] = states.PosteriorOL[tableIndex];
                Table.Rows[rowIndex]["PosteriorMean"] = states.PosteriorMean[tableIndex];
                Table.Rows[rowIndex]["Obs"] = states.Obs[tableIndex];

                for (int i = 0; i < ensembleSize; i++)
                {
                    Table.Rows[rowIndex]["PosteriorEnsemble" + i.ToString()] = states.Posterior[tableIndex][i];
                    if (DAOption == "EnKF")
                        Table.Rows[rowIndex]["ObsEnsemble" + i.ToString()] = states.ObsPerturb[tableIndex][i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="states"></param>
        /// <param name="DAOption"></param>
        /// <param name="tableIndex"></param>
        /// <param name="ensembleSize"></param>
        public void InsertOutputPosterior(int rowIndex, StatesOfTheDay states, string DAOption, int tableIndex, int ensembleSize)
        {
            if (Table.Rows[rowIndex]["ID"] != null)
            {
                    Table.Rows[rowIndex]["Obs"] = states.Obs[tableIndex];

                for (int i = 0; i < ensembleSize; i++)
                {
                    if (DAOption == "EnKF")
                        Table.Rows[rowIndex]["ObsEnsemble" + i.ToString()] = states.ObsPerturb[tableIndex][i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rowIndex"> The row index corresponding to ID. </param>
        /// <param name="columnName"></param>
        public void InsertSingle(double value, int rowIndex, string columnName)
        {
            if (Table.Rows[rowIndex]["ID"] != null)
            {
                Table.Rows[rowIndex][columnName] = value;
            }
            else
            {
                throw new Exception("Table row does not exist!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public double GetSingle(int rowIndex, string columnName)
        {
            string value;
            if (Table.Rows[rowIndex]["ID"] != null)
            {
                value = Table.Rows[rowIndex][columnName].ToString();
                return Convert.ToDouble(value);
            }
            else
            {
                throw new Exception("Table row does not exist!");
            }
        }
    }
}