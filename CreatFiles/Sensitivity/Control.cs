using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data;

namespace Sensitivity
{
    public class Control
    {
        public DataTable ReadControl(FolderStructure folder, string dataType = "Data")
        {
            string fileName = folder.Origin + "/Control.csv";
            DataTable dt = new DataTable();
            FileStream fs1 = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr1 = new StreamReader(fs1, Encoding.UTF8);

            //string[] option = new string[] { "", "" };
            string strLine = "";
            string[] tableWithHead = null;
            List<string> columnNames = new List<string>();
            List<double[]> values = new List<double[]>();

            int rowCount = 0;

            while ((strLine = sr1.ReadLine()) != null)
            {
                if (fileName.Contains(".csv")) { tableWithHead = strLine.Split(','); }
                else if (fileName.Contains(".txt")) { tableWithHead = strLine.Split('\t'); }
                else { throw new Exception("Wrong input file format."); }
                if (tableWithHead[0] != "<" + dataType + ">") { continue; }
                else { break; }
            }
            while ((strLine = sr1.ReadLine()) != null)
            {
                if (fileName.Contains(".csv")) { tableWithHead = strLine.Split(','); }
                else if (fileName.Contains(".txt")) { tableWithHead = strLine.Split('\t'); }
                else { throw new Exception("Wrong input file format."); }
                rowCount = tableWithHead.Length - 1;

                if (tableWithHead[0].Contains("<") && !tableWithHead[0].Contains("</" + dataType + ">") || tableWithHead[0].Contains("//")) { continue; }
                //else if (tableWithHead[0] == "<Option>")
                //{
                //    int i = 1;
                //    while (tableWithHead[i] != "" && i < 5)
                //    {
                //        option[i - 1] = tableWithHead[i];
                //        i++;
                //    }
                //    continue;
                //}
                else if (tableWithHead[0].Contains("</" + dataType + ">")) { break; }
                else
                {
                    columnNames.Add(tableWithHead[0]);
                    double[] value = new double[rowCount];
                    for (int j = 0; j < rowCount; j++)
                    {
                        value[j] = Convert.ToDouble(tableWithHead[j + 1]);
                    }
                    values.Add(value);
                }
            }

            //Add columns.
            for (int i = 0; i < columnNames.Count; i++)
            {
                DataColumn dc = new DataColumn(columnNames[i], typeof(double));
                dt.Columns.Add(dc);
            }
            sr1.Close();
            fs1.Close();
            for (int j = 0; j < rowCount; j++)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < values.Count; i++)
                {
                    dr[i] = values[i][j];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// Select columns from original datatable based on column names or column index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public DataTable SelectControl<T>(DataTable table, T[] variables)
        {
            if (variables != null)
            {
                DataTable dt = new DataTable();
                List<double> values = new List<double>();
                for (int i = 0; i < variables.Length; i++)
                {
                    if (variables.GetType() == typeof(string[]))
                    {
                        DataColumn dc = new DataColumn(variables[i] as string, typeof(double));
                        dt.Columns.Add(dc);
                    }
                    else if (variables.GetType() == typeof(int[]))
                    {
                        int index = Convert.ToInt16(variables[i]);
                        DataColumn dc = new DataColumn(table.Columns[index].ToString(), typeof(double));
                        dt.Columns.Add(dc);
                    }
                    else { throw new Exception("Wrong data type!"); }
                }
                for (int j = 0; j < table.Rows.Count; j++) //Continue work...
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dr[i] = table.Rows[j][dt.Columns[i].ToString()];
                    }
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            else
            {
                return table;
            }

        } 
    }
}