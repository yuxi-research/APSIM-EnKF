using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Data;

namespace Sensitivity
{
    public class Weather
    {
        // For everything except rainfall
        public static System.Data.DataTable EditMet(FolderStructure folder, DataTable table)
        {
            string controlFile = folder.Origin + "/Control.csv";
            string metFile = folder.Origin + "/Weather.met";
            for (int file = 0; file < table.Columns.Count; file++)
            {
                for (int num = 0; num < table.Rows.Count; num++)
                {
                    string targetFile = folder.Met + "/" + table.Columns[file].ToString() + num.ToString() + ".met";
                    string variable = table.Columns[file].ToString();
                    double value = Convert.ToDouble(table.Rows[num][variable]);
                    EditSingleMet(metFile, targetFile, variable, value);
                }
            }
            return table;
        }

        //For temperature
        public static void EditMet2(FolderStructure folder, DataTable table, string[] variables)
        {
            string controlFile = folder.Origin + "/Control.csv";
            string metFile = folder.Origin + "/Weather.met";
            for (int num = 0; num < table.Rows.Count; num++)
            {
                string targetFile = folder.Met + "/" + "temp" + num.ToString() + ".met";
                double[] values = new double[table.Columns.Count];
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    values[i] = Convert.ToDouble(table.Rows[num][variables[i]]);
                }
                //double value = Convert.ToDouble(table.Rows[num][variable]);
                EditMultiMet(metFile, targetFile, variables, values);
            }
        }

        public static void EditSingleMet(string metFile, string targetFile, string variable, double value)
        {
            StreamReader sr = new StreamReader(metFile);
            StreamWriter sw = new StreamWriter(targetFile);
            sw.Close();
            string strLine = "";
            string[] row = null;
            string[] columnNames = null;

            while ((strLine = sr.ReadLine()) != null)
            {
                StreamWriter sw1 = new StreamWriter(targetFile, true);

                if (strLine.Contains('[') && strLine.Contains(']') || strLine.IndexOf('!') == 0 || strLine.IndexOf('(') == 0)
                {
                    sw1.WriteLine(strLine);
                }
                else if (strLine.Contains("tav"))
                {
                    sw1.WriteLine(strLine);     //Change this later.
                }
                else if (strLine.Contains("amp"))
                {
                    sw1.WriteLine(strLine);     //Change this later.
                }
                else if (strLine.Contains("year"))
                {
                    sw1.WriteLine(strLine);
                    columnNames = strLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    sw1.Close();
                    break;
                }
                else
                {
                    sw1.WriteLine(strLine);
                }
                sw1.Close();
            }

            int index = Array.IndexOf(columnNames, variable);

            while ((strLine = sr.ReadLine()) != null)
            {
                StreamWriter sw1 = new StreamWriter(targetFile, true);
                if (strLine.Contains('(') && strLine.Contains('('))
                {
                    sw1.WriteLine(strLine);
                    sw1.Close();
                }
                else
                {
                    row = strLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double newValue = Convert.ToDouble(row[index]) + value;
                    if (columnNames[index] == "rain" || columnNames[index] == "radn" || columnNames[index] == "evap" || columnNames[index] == "vp")
                    {
                        newValue = Math.Max(0, newValue);
                    }
                    row[index] = newValue.ToString();
                    foreach (string str in row)
                    {
                        sw1.Write(str + "\t");
                    }
                    sw1.WriteLine();
                }
                sw1.Close();
            }
            sr.Close();
        }

        public static void EditMultiMet(string metFile, string targetFile, string[] variables, double[] value)
        {
            StreamReader sr = new StreamReader(metFile);
            StreamWriter sw = new StreamWriter(targetFile);
            sw.Close();
            string strLine = "";
            string[] row = null;
            string[] columnNames = null;

            while ((strLine = sr.ReadLine()) != null)
            {
                StreamWriter sw1 = new StreamWriter(targetFile, true);

                if (strLine.Contains('[') && strLine.Contains(']') || strLine.IndexOf('!') == 0 || strLine.IndexOf('(') == 0)
                {
                    sw1.WriteLine(strLine);
                }
                else if (strLine.Contains("tav"))
                {
                    sw1.WriteLine(strLine);     //Change this later.
                }
                else if (strLine.Contains("amp"))
                {
                    sw1.WriteLine(strLine);     //Change this later.
                }
                else if (strLine.Contains("year"))
                {
                    sw1.WriteLine(strLine);
                    columnNames = strLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    sw1.Close();
                    break;
                }
                else
                {
                    sw1.WriteLine(strLine);
                }
                sw1.Close();
            }

            int[] indices = new int[variables.Count()];
            for (int i = 0; i < variables.Count(); i++)
            {
                indices[i] = Array.IndexOf(columnNames, variables[i]);
            }

            while ((strLine = sr.ReadLine()) != null)
            {
                StreamWriter sw1 = new StreamWriter(targetFile, true);
                if (strLine.Contains('(') && strLine.Contains('('))
                {
                    sw1.WriteLine(strLine);
                    sw1.Close();
                }
                else
                {
                    row = strLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double[] newValues = new double[indices.Count()];
                    for (int i = 0; i < indices.Count(); i++)
                    {
                        newValues[i] = Convert.ToDouble(row[indices[i]]) + value[i];

                        if (columnNames[indices[i]] == "rain")
                        {
                            newValues[i] = Math.Max(0, newValues[i]);
                        }
                        row[indices[i]] = newValues[i].ToString();
                    }

                    foreach (string str in row)
                    {
                        sw1.Write(str + "\t");
                    }
                    sw1.WriteLine();
                }
                sw1.Close();
            }
            sr.Close();
        }

        public static System.Data.DataTable ReadMet(FolderStructure folder)
        {
            string controlFile = folder.Origin + "/Control.csv";
            string metFile = folder.Origin + "/Weather.met";

            System.Data.DataTable dt = new System.Data.DataTable();
            FileStream fs1 = new FileStream(metFile, FileMode.Open, FileAccess.Read);
            StreamReader sr1 = new StreamReader(fs1, Encoding.UTF8);

            string strLine = "";
            string[] row = null;
            string[] columnNames = null;
            string[] units = null;
            //List<double[]> values = new List<double[]>();

            while ((strLine = sr1.ReadLine()) != null)
            {
                if (strLine.Contains('[')) { continue; }
                if (strLine.Contains('!'))
                {
                    int end = strLine.IndexOf('!');
                    strLine = strLine.Substring(0, end);
                }
                if (strLine.Contains('='))
                {
                    row = strLine.Split(new char[] { ' ', '=', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                }
                if (strLine.Contains("year"))
                {
                    columnNames = strLine.Split('\t');

                    strLine = sr1.ReadLine();
                    units = strLine.Split('\t');
                    //foreach (string u in unit)    //No need to remove "()".
                    //{
                    //    u.Replace('(', ' ');
                    //    u.Replace(')', ' ');
                    //}
                    break;
                }
                for (int i = 0; i < columnNames.Count(); i++)
                {
                    System.Data.DataColumn dc = new System.Data.DataColumn(columnNames[i], typeof(double));
                    dt.Columns.Add(dc);
                }
            }
            while ((strLine = sr1.ReadLine()) != null)
            {
                row = strLine.Split('\t');
                System.Data.DataRow dr = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dr[i] = Convert.ToDouble(row[i]);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}