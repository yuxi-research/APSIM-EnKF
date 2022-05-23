using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shared;
using System.Threading;

namespace Weather
{
    class Program
    {
        static int Main(string[] args)
        {
            string root = ".";
            int exitCode = 0;
            try
            {
                if (args.Count() > 0)
                {
                    FileInfo info = new FileInfo(args[0]);
                    root = info.ToString();
                }
                FolderInfo folder = new FolderInfo(root);
                PerturbControl control = MyXml.Deserialize(folder.Origin);

                if (args.Count() == 3)
                {
                    int start_row = Convert.ToInt16(args[1]);
                    int end_row = Convert.ToInt16(args[2]);
                    PerturbMet3.EditMet(folder, control, start_row, end_row);

                }
                else
                {
                    PerturbMet3.EditMet(folder, control);
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
