using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.IO;
using Shared;

namespace Weather
{
    /// <summary>
    /// Perturb weather data with addtive or multiplicative methods.
    /// Equivelent to PerturbMet.
    /// Old perturb met using excel.
    /// </summary>
    public class PerturbMet2
    {
        /// <summary>
        /// purtrub MET file and save as MET\*xlsx.
        /// </summary>
        /// <param name="metSD"></param>
        /// <param name="ensembleSize"></param>
        /// <param name="rootPath"></param>
        /// <param name="index"></param>
        /// <param name="sR"></param>
        /// <param name="sC"></param>
        public static void MetFile(double[] metSD, FolderInfo folder, PerturbControl control, int index = 1, int sR = 21, int sC = 3) //读取第index个sheet的数据
        {
            string originMetFile = folder.Origin + "/Weather.xlsx";
            string targetMetFile = folder.Met + "/Weather_Ensemble";
            //Start Excel application.
            Application xls = new Application();
            //Open spreadsheet.
            _Workbook book = xls.Workbooks.Open(originMetFile, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            _Worksheet sheet;//定义sheet变量
            xls.Visible = false;//设置Excel后台运行
            xls.DisplayAlerts = false;//设置不显示确认修改提示

            try
            {
                sheet = (_Worksheet)book.Worksheets.get_Item(index);//获得第index个sheet，准备读取
            }
            catch (Exception ex)//不存在就退出
            {
                Console.WriteLine(ex.Message);
            }

            sheet = (_Worksheet)book.Worksheets.get_Item(index);//获得第index个sheet，准备读取
            Console.WriteLine("SheetName = {0}", sheet.Name);
            int row = sheet.UsedRange.Rows.Count;//获取不为空的行数
            int col = sheet.UsedRange.Columns.Count;//获取不为空的列数
            Console.WriteLine("row = {0}, col = {1}.", row, col);
            string inputName = sheet.Range[sheet.Cells[sR - 2, sC], sheet.Cells[sR - 2, sC]].Cells.Value2; //获得区域数据赋值给Array数组，方便读取
            object[,] value = (object[,])sheet.Range[sheet.Cells[sR, sC], sheet.Cells[row, col - 1]].Cells.Value2; //获得区域数据赋值给Array数组，方便读取

            for (int size = 0; size < control.EnsembleSize; size++)
            {
                Console.WriteLine("Writting File {0} ...", size.ToString());
                double normRand = 0;
                //For radn.
                for (int r = 1; r <= row - sR + 1; r++) //row - sR + 1
                {
                    string valueString = Convert.ToString(value[r, 1]);
                    double valueDouble = Convert.ToDouble(valueString);
                    valueDouble = valueDouble + metSD[1 - 1] * Distribution.NormalRand();      //Replace with perturb methods.
                    valueDouble = Math.Max(0, valueDouble);
                    sheet.Cells[sR + r - 1, sC + 1 - 1] = Convert.ToString(valueDouble);
                }

                //For maxt and mint.
                for (int r = 1; r <= row - sR + 1; r++) //row - sR + 1
                {
                    normRand = Distribution.NormalRand();
                    for (int c = 2; c <= 3; c++)  //The last row is "code";  = col - sC + 1 - 2
                    {
                        string valueString = Convert.ToString(value[r, c]);
                        double valueDouble = Convert.ToDouble(valueString);
                        valueDouble = valueDouble + normRand * metSD[c - 1];
                        sheet.Cells[sR + r - 1, sC + c - 1] = Convert.ToString(valueDouble);
                    }
                }
                //For rain. (%)
                for (int r = 1; r <= row - sR + 1; r++)
                {
                    normRand = Distribution.NormalRand();
                    string valueString = Convert.ToString(value[r, 4]);
                    double valueDouble = Convert.ToDouble(valueString);
                    if (valueDouble != 0)
                    {
                        valueDouble = valueDouble + valueDouble * normRand * metSD[4 - 1];
                        valueDouble = Math.Max(0, valueDouble);
                    }
                    sheet.Cells[sR + r - 1, sC + 4 - 1] = Convert.ToString(valueDouble);
                }

                //For pan evap. (%)
                for (int r = 1; r <= row - sR + 1; r++)
                {
                    normRand = Distribution.NormalRand();
                    string valueString = Convert.ToString(value[r, 5]);
                    double valueDouble = Convert.ToDouble(valueString);
                    if (valueDouble != 0)
                    {
                        valueDouble = valueDouble + valueDouble * normRand * metSD[5 - 1];
                        valueDouble = Math.Max(0, valueDouble);
                    }
                    sheet.Cells[sR + r - 1, sC + 5 - 1] = Convert.ToString(valueDouble);
                }

                //For vp.
                for (int r = 1; r <= row - sR + 1; r++)
                {
                    normRand = Distribution.NormalRand();
                    string valueString = Convert.ToString(value[r, 6]);
                    double valueDouble = Convert.ToDouble(valueString);
                    if (valueDouble != 0)
                    {
                        valueDouble = valueDouble + normRand * metSD[6 - 1];
                        valueDouble = Math.Max(0, valueDouble);
                    }
                    sheet.Cells[sR + r - 1, sC + 6 - 1] = Convert.ToString(valueDouble);
                }

                ////For evap and vp
                //for (int r = 1; r <= row - sR + 1; r++) //row - sR + 1
                //{
                //    for (int c = 5; c <= 6; c++)  //The last row is "code";
                //    {
                //        string valueString = Convert.ToString(value[r, c]);
                //        double valueDouble = Convert.ToDouble(valueString);
                //        valueDouble = valueDouble + normRand * metSD[c - 1];
                //        valueDouble = Math.Max(0, valueDouble);
                //        sheet.Cells[sR + r - 1, sC + c - 1] = Convert.ToString(valueDouble);
                //    }
                //}

                sheet.Cells[18, 1] = "! Ensemble" + size.ToString();
                sheet.Cells[18, 2] = "! SD" + size.ToString();
                string temp = targetMetFile + (size).ToString() + ".xlsx";
                temp = temp.Replace("/", "\\");
                Console.WriteLine(temp + " saved!");
                book.SaveAs(temp);
            }

            book.Close(false, Missing.Value, Missing.Value);//关闭打开的表
            xls.Quit();//Excel程序退出
                       //          sheet,book,xls设置为null，防止内存泄露
            sheet = null;
            book = null;
            xls = null;
            GC.Collect();//系统回收资源
        }
        /// <summary>
        /// Transfer Excel files in MET folder to *.met files.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="extension"></param>
        /// <param name="index"></param>
        public static void ExcelToMet(FolderInfo folder, string extension = ".met", int index = 1)//读取第index个sheet的数据
        {
            string[] files = Directory.GetFiles(folder.Met, "*.xlsx");
            for (int i = 0; i < files.Length; i++)
            {
                //启动Excel应用程序
                Application xls = new Application();

                string metFile = folder.Met + "/" + Path.GetFileNameWithoutExtension(files[i]) + extension;
                //打开filename表
                _Workbook book = xls.Workbooks.Open(files[i], Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                _Worksheet sheet;//定义sheet变量
                xls.Visible = false;//设置Excel后台运行
                xls.DisplayAlerts = false;//设置不显示确认修改提示

                try
                {
                    sheet = (_Worksheet)book.Worksheets.get_Item(index);//获得第index个sheet，准备读取
                }
                catch (Exception ex)//不存在就退出
                {
                    Console.WriteLine(ex.Message);
                }

                sheet = (_Worksheet)book.Worksheets.get_Item(index);//获得第index个sheet，准备读取
                                                                    //            Console.WriteLine(sheet.Name);
                int row = sheet.UsedRange.Rows.Count;//获取不为空的行数
                int col = sheet.UsedRange.Columns.Count;//获取不为空的列数
                                                        //            Console.WriteLine("row={0},col={1}", row, col);
                object[,] value = (object[,])sheet.Range[sheet.Cells[1, 1], sheet.Cells[row, col]].Cells.Value2; //获得区域数据赋值给Array数组，方便读取

                FileStream fs = new FileStream(metFile, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                for (int r = 1; r <= row; r++) //row - sR + 1
                {
                    for (int c = 1; c <= col; c++)
                    {
                        string valueString = Convert.ToString(value[r, c]);
                        sw.Write(valueString + " ");
                    }
                    sw.Write("\n");
                }
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();

                Console.WriteLine(metFile + " saved!");
                book.Close(false, Missing.Value, Missing.Value);//关闭打开的表

                xls.Quit();//Excel程序退出
                sheet = null;            //          sheet,book,xls设置为null，防止内存泄露
                book = null;
                xls = null;
                GC.Collect();//系统回收资源
            }
        }

        static void Display(XmlDocument doc)
        {
            XmlNode root = doc.DocumentElement;
            XmlNodeList rootChild = root.ChildNodes;
            Console.WriteLine("Document information: [1] root.ChildNodes.Name and [2] root.ChildNodes.ChildNodes[1].Name ");
            foreach (XmlNode n in rootChild)
            {
                Console.WriteLine(n.Name);
                Console.WriteLine("\t{0}", n.ChildNodes[0].InnerText);
                Console.WriteLine();
            }
        }
    }
}
