using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.IO;
using System.Threading;

namespace Cultivar
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
                PerturbPara.EditPara(folder, control, "Wheat");

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
