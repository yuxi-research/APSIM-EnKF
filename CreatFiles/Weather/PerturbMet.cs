using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Diagnostics;
using Shared;

namespace Weather
{
    /// <summary>
    ///Perturb weather data with addtive or multiplicative methods.
    ///Not using.
    /// </summary>
    public class PerturbMet
    {
        public static void EditMet(FolderInfo folder, PerturbControl control)
        {
            //Copy file.
            string OrignFile = folder.Origin + "/Weather.met";
            string truthMet = folder.Met + "/Weather.met";
            File.Copy(OrignFile, truthMet, true);

            string OpenLoopMet = folder.Met + "/OpenLoop.met";
            //EditMultiMet(truthMet, OpenLoopMet, control, dis);
            //Console.WriteLine("Met file: [/OpenLoop.met]is saved!");

            string OpenLoopCopy = folder.Met + "/Weather_Ensemble" + control.EnsembleSize.ToString() + ".met";
            File.Copy(OpenLoopMet, OpenLoopCopy, true);

            for (int num = 0; num < control.EnsembleSize; num++)
            {
                string targetFile = folder.Met + "/Weather_Ensemble" + (num).ToString() + ".met";
                EditMultiMet(OpenLoopMet, targetFile, control);
                Console.WriteLine("Met file: [/Weather_Ensemble{0}.met]is saved!");
            }
        }

        public static void EditMultiMet(string metFile, string targetFile, PerturbControl control)
        {
            string[] variables = control.weatherNames;
            StreamReader sr = new StreamReader(metFile);
            StreamWriter sw = new StreamWriter(targetFile); //Create new targetFile.
            sw.Close();
            string strLine = "";
            string[] row = null;
            string[] columnNames = null;

            while ((strLine = sr.ReadLine()) != null)
            {
                StreamWriter sw1 = new StreamWriter(targetFile, true);  //Append new line to  targetFile.

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


            while ((strLine = sr.ReadLine()) != null)
            {
                StreamWriter sw1 = new StreamWriter(targetFile, true);
                if (strLine.Contains('(') && strLine.Contains(')'))
                {
                    sw1.WriteLine(strLine);
                    sw1.Close();
                }
                else
                {
                    row = strLine.Split(new char[] { ' ','\t' }, StringSplitOptions.RemoveEmptyEntries);
                    double newValue;
                    double newValue2;
                    double temp;
                    for (int i = 0; i < 1; i++)
                    {
                        newValue = Convert.ToDouble(row[i + 2]);

                        if (control.WeatherPerturbOption[i] == 0)
                        {
                            newValue = newValue + control.WeatherError[i] * Distribution.NormalRand();
                        }
                        else if (control.WeatherPerturbOption[i] == 1)
                        {
                            newValue = newValue + newValue * control.WeatherError[i] * Distribution.NormalRand();
                        }
                        else { throw new Exception(" Wrong Weather perturbation option!"); }

                        newValue = Math.Max(control.WeatherLowerBound[i], newValue);
                        row[i + 2] = newValue.ToString();
                    }

                    for (int i = 1; i < 2; i++)
                    {
                        temp = Distribution.NormalRand();
                        newValue = Convert.ToDouble(row[i + 2]);
                        newValue2 = Convert.ToDouble(row[i + 3]);
                        if (control.WeatherPerturbOption[i] == 0)
                        {
                            newValue = newValue + control.WeatherError[i] * temp;
                            newValue2 = newValue2 + control.WeatherError[i] * temp;
                        }
                        else if (control.WeatherPerturbOption[i] == 1)
                        {
                            newValue = newValue + newValue * control.WeatherError[i] * temp;
                            newValue2 = newValue2 + newValue2 * control.WeatherError[i] * temp;

                        }
                        else { throw new Exception(" Wrong Weather perturbation option!"); }

                        newValue = Math.Max(control.WeatherLowerBound[i], newValue);
                        newValue2 = Math.Max(control.WeatherLowerBound[i + 1], newValue2);
                        row[i + 2] = newValue.ToString();
                        row[i + 3] = newValue2.ToString();

                    }
                    for (int i = 3; i < 6; i++)
                    {
                        newValue = Convert.ToDouble(row[i + 2]);

                        if (control.WeatherPerturbOption[i] == 0)
                        {
                            newValue = newValue + control.WeatherError[i] * Distribution.NormalRand();
                        }
                        else if (control.WeatherPerturbOption[i] == 1)
                        {
                            newValue = newValue + newValue * control.WeatherError[i] * Distribution.NormalRand();
                        }
                        else { throw new Exception(" Wrong Weather perturbation option!"); }

                        newValue = Math.Max(control.WeatherLowerBound[i], newValue);
                        row[i + 2] = newValue.ToString();
                    }

                    //YUXI: Continue...

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
    }
}
