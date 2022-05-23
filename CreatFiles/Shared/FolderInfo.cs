using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Shared
{
    public class FolderInfo
    {
        public string Root { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Origin { get; set; }
        public string Command { get; set; }
        public string Example { get; set; }
        public string Met { get; set; }
        public string Obs { get; set; }
        public string Cultivar { get; set; }
        public string FileName_OL { get; set; }
        public string FileName_DA { get; set; }
        public FolderInfo(string root)
        {
            //For EnKF.sln to purturb met and para.
            Root = root;
            Root = Path.GetFullPath(Root);
            Root = Root.Replace('\\', '/');
            Console.WriteLine("Current directory: [" + Root + "]");

            FileName_OL = "OpenLoop";
            FileName_DA = "EnKF";

            Output = Root + "/Output";
            Origin = Root + "/Origin";
            Met = Root + "/Met";
            Obs = Root + "/Obs";
            Cultivar = Root + "/Cultivar";
        }
    }
}
