using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shared;

namespace Weather
{
    /// <summary>
    /// Perturb weather data using Turner's method (Turner, 2008).
    /// </summary>
    class PerturbMet3
    {
        public static void EditMet(FolderInfo folder, PerturbControl control, int start_row = 1, int end_row = 1000)
        {
            string OrignFile = folder.Origin + "/Weather.met";
            string truthMet = folder.Met + "/Weather.met";
            string OpenLoopMet = folder.Met + "/OpenLoop.met";
            if (!control.FixedOL)
            {
                //Copy Origin/Weather file to Met/Weather and create openloop file.
                File.Copy(OrignFile, truthMet, true);
                EditMultiMet(truthMet, OpenLoopMet, control, start_row, end_row);
                Console.WriteLine("Met file: [/OpenLoop.met]is saved!");
            }

            // Create ensembles from openloop.
            for (int num = 0; num < control.EnsembleSize; num++)
            {
                string targetFile = folder.Met + "/Weather_Ensemble" + (num).ToString() + ".met";
                EditMultiMet(OpenLoopMet, targetFile, control, start_row, end_row);
                Console.WriteLine("Met file: [/Weather_Ensemble{0}.met]is saved!", num);
            }
        }

        public static void EditMultiMet(string metFile, string targetFile, PerturbControl control, int start_row, int end_row)
        {
            double[] offsetRand = new double[control.weatherNames.Count()];
            for (int i = 0; i < offsetRand.Count(); i++) { offsetRand[i] = Distribution.NormalRand(); }

            string[] variables = control.weatherNames;
            StreamReader sr = new StreamReader(metFile);
            StreamWriter sw = new StreamWriter(targetFile); //Create new targetFile.
            sw.Close();
            string strLine = "";
            string[] row = null;
            string[] columnNames = null;
            string sep = " ";

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


            int this_row = 0;

            while ((strLine = sr.ReadLine()) != null)
            {
                this_row++;

                StreamWriter sw1 = new StreamWriter(targetFile, true);
                if (strLine.Contains('(') && strLine.Contains(')'))
                {
                    this_row--;

                    sw1.WriteLine(strLine);
                    sw1.Close();
                }
                else if (this_row < start_row || this_row > end_row)
                {
                    sw1.WriteLine(strLine);
                    sw1.Close();
                }
                else
                {
                    row = strLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    double[] newValues = new double[control.weatherNames.Count()];
                    double theta1, theta2, zeta, beta;
                    for (int i = 0; i < control.weatherNames.Count(); i++)
                    {
                        newValues[i] = Convert.ToDouble(row[i + 2]);
                        if (control.WeatherPerturbOption[i] == 0)
                        {
                        }   //No perturbation.
                        else if (control.WeatherPerturbOption[i] == 1)  //Unrestricted.
                        {
                            theta1 = control.Xi[i];
                            theta2 = control.Chi[i];
                            zeta = theta1 * Distribution.NormalRand();
                            beta = theta2 * offsetRand[i];
                            newValues[i] = newValues[i] + zeta + beta;
                        }
                        else if (control.WeatherPerturbOption[i] == 2)  //Semi-restricted: lower-bounded.
                        {
                            theta1 = (newValues[i] - control.WeatherLowerBound[i]) * control.Xi[i];
                            theta2 = (newValues[i] - control.WeatherLowerBound[i]) * control.Chi[i];
                            zeta = theta1 * Distribution.NormalRand();
                            beta = theta2 * offsetRand[i];
                            newValues[i] = newValues[i] + zeta + beta;
                        }
                        else if (control.WeatherPerturbOption[i] == 3)  //Semi-restricted: upper-bounded.
                        {
                            theta1 = (control.WeatherUpperBound[i] - newValues[i]) * control.Xi[i];
                            theta2 = (control.WeatherUpperBound[i] - newValues[i]) * control.Chi[i];
                            zeta = theta1 * Distribution.NormalRand();
                            beta = theta2 * offsetRand[i];
                            newValues[i] = newValues[i] + zeta + beta;
                        }
                        else if (control.WeatherPerturbOption[i] == 4)  //Restricted: both end.
                        {
                        }
                        newValues[i] = Math.Max(control.WeatherLowerBound[i], newValues[i]);
                        newValues[i] = Math.Min(control.WeatherUpperBound[i], newValues[i]);
                    }
                    if (newValues[1] < newValues[2])
                    {
                        double temp2 = newValues[1];
                        newValues[1] = newValues[2];
                        newValues[2] = temp2;
                    }
                    for (int i = 0; i < newValues.Count(); i++)
                    {
                        row[i + 2] = newValues[i].ToString();
                    }

                    sw1.Write(row[0]);
                    for (int i = 1; i < row.Count(); i++)
                    {
                        sw1.Write(sep + row[i]);
                    }
                    sw1.WriteLine();
                }
                sw1.Close();
            }
            sr.Close();
        }
    }
}
