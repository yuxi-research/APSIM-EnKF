using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sensitivity
{
    public class FolderStructure
    {
        public string Root { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Origin { get; set; }
        public string Command { get; set; }
        public string Example { get; set; }
        public string Resources { get; set; }
        public string ResourcesForOpenLoop { get; set; }
        public string Met { get; set; }
        public string Obs { get; set; }
        public string Cultivar { get; set; }
        public string Data { get; set; }
        public string Fig { get; set; }
        public string FileName { get; set; }
        public int EnsembleSize { get; set; }

        /// <summary>
        /// The summary folder path.
        /// </summary>
        /// <param name="info"></param>
        public FolderStructure(FileInfo info, int size = 3)
        {
            Root = info.ToString();
            Root = Path.GetFullPath(Root);
            Root = Root.Replace('\\', '/');
            Input = Root + "/Input";
            Output = Root + "/Output";
            Origin = Root + "/Origin";
            Example = Root + "/Example";
            Command = Root + "/Command";
            Met = Root + "/Input/Met";
            Obs = Root + "/Input/Obs";
            Cultivar = Root + "/Input/Cultivar";
            Data = Root + "/Output/Data";
            Fig = Root + "/Output/Fig";
            Resources = Root + "../../../DABranch1/ApsimX.DA/Models/Resources";
            EnsembleSize = size;
        }
        public FolderStructure(int pathOption, int size)
        {
            switch (pathOption)
            {
                //case 0:
                //    {
                //        //For APSIM.DA.
                //        FileInfo info = new FileInfo("../../../Summary" + "/ExampleDA");
                //        Root = info.ToString();
                //        Root = Path.GetFullPath(Root);
                //        Root = Root.Replace('\\', '/');
                //        EnsembleSize = size;
                //        Resources = Root + "../../../DABranch1/ApsimX.DA/Models/Resources";
                //        break;
                //    }
                case 1:
                    {
                        //For EnKF.sln to purturb met and para.
                        FileInfo info = new FileInfo("../../../../Summary" + "/ExampleDA1");
                        Root = info.ToString();
                        Root = info.ToString();
                        Root = Path.GetFullPath(Root);
                        Root = Root.Replace('\\', '/');
                        EnsembleSize = size;
                        Resources = Root + "../../../DABranch1/ApsimX.DA/Models/Resources";
                        ResourcesForOpenLoop = Root + "../../../SensitivityBranch0/ApsimX.Sens/Models/Resources";
                        break;
                    }
                case 2:
                    {
                        //For Sensitivity.
                        FileInfo info = new FileInfo("../../Summary" + "/ExampleDA");
                        Root = info.ToString();
                        EnsembleSize = size;
                        Resources = Root + "../../../SensitivityBranch1/ApsimX.Sens/Models/Resources";
                        break;
                    }
                default:
                    {
                        //For nothing.
                        FileInfo info = new FileInfo("../../Summary" + "/ExampleDA");
                        Root = info.ToString();
                        Root = Path.GetFullPath(Root);
                        Root = Root.Replace('\\', '/');
                        EnsembleSize = size;
                        Resources = Root + "../../../DABranch1/ApsimX.DA/Models/Resources";
                        Resources = Path.GetFullPath(Resources);
                        Resources = Resources.Replace('\\', '/');
                        break;
                    }
            }
            Input = Root + "/Input";
            Output = Root + "/Output";
            Origin = Root + "/Origin";
            Example = Root + "/Example";
            Command = Root + "/Command";
            Met = Root + "/Input/Met";
            Obs = Root + "/Input/Obs";
            Cultivar = Root + "/Input/Cultivar";
            Data = Root + "/Output/Data";
            Fig = Root + "/Output/Fig";
        }
    }
    public class CountThread
    {
        private static int count = -1;
        public int Count { get { return count; } }
        public void Add()
        {
            count++;
            Console.WriteLine(Count);
        }
    }
}
