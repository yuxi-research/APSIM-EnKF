using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace XMLEdit
{
    class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;
            try
            {
                if (args.Count() == 0)
                {
                    string root = "D:/Users/Yuxi Zhang/Desktop/TEST";
                    string inputOL = root + "/OpenLoop_save.apsimx";
                    string inputDA = root + "/EnKF_save.apsimx";
                    string outputOL = root + "/OpenLoop.apsimx";
                    string outputDA = root + "/EnKF.apsimx";
                    XMLEdit.ObsEdit(inputOL, outputOL, "SW1", "0.153", "4");
                    XMLEdit.ObsEdit(inputDA, outputDA, "SW3", "0.163", "5");
                }

                else if (args.Count() == 3)
                {
                    string inputOL = "Output/apsimx/OpenLoop_save.apsimx";
                    string inputDA = "Output/apsimx/EnKF_save.apsimx";
                    string outputOL = "Output/OpenLoop.apsimx";
                    string outputDA = "Output/EnKF.apsimx";
                    XMLEdit.ObsEdit(inputOL, outputOL, args[0], args[1], args[2]);
                    XMLEdit.ObsEdit(inputDA, outputDA, args[0], args[1], args[2]);
                }

                Console.WriteLine("Finished!");
                Thread.Sleep(500);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                exitCode = 1;
            }

            if (exitCode != 0)
                Console.WriteLine("ERRORS FOUND!!");

            return exitCode;
        }
    }
}

