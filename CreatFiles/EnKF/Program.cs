using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shared;
using System.Threading;

namespace EnKF
{
    class Program
    {
        static int Main(string[] args)
        {
            //string root = " D:\\Dropbox\\Summary\\DA_50_9_EnKF";
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

                ApsimXEdit.ApsimXFile(folder, control);

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
    
