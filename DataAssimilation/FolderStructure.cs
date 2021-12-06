using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataAssimilation
{
    [Serializable]
    public class FolderStructure
    {
        public string Root { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Origin { get; set; }
        public string Command { get; set; }
        public string Example { get; set; }
        public string Resources { get; set; }
        public string Met { get; set; }
        public string Obs { get; set; }
        public string Cultivar { get; set; }
        public string InitialCondition { get; set; }
        public string FileName { get; set; }
        public string SQLite { get; set; }

        private DAControl control = new DAControl(0);

        public FolderStructure(int option = 0)
        {
            switch (option)
            {
                case 0:
                    {
                        //For APSIM.DA.
                        FileInfo info = new FileInfo("../../../Summary" + "/DA_New1");
                        Root = info.ToString();
                        Root = Path.GetFullPath(Root);
                        Root = Root.Replace('\\', '/');
                        Resources = Root + "../../../DABranch1/ApsimX.DA/Models/Resources";

                        if (control.OpenLoop)
                        {
                            FileName = "OpenLoop";
                        }
                        else
                        {
                            FileName = "DAExample";
                        }
                        break;
                    }
                case 1:
                    {
                        //For Test.
                        FileInfo info = new FileInfo("../../../../Summary" + "/DA_Lite2");
                        Root = info.ToString();
                        Root = Path.GetFullPath(Root);
                        Root = Root.Replace('\\', '/');
                        Resources = Root + "../../../DABranch1/ApsimX.DA/Models/Resources";
                        FileName = "DAExample";
                        break;
                    }

                default:
                    {
                        //For nothing.
                        break;
                    }
            }
            Input = Root + "/Input";
            Output = Root + "/Output";
            Origin = Root + "/Origin";
            Example = Root + "/Example";
            Command = Root + "/Command";
            Met = Input + "/Met";
            Obs = Input + "/Obs";
            Cultivar = Input + "/Cultivar";
            InitialCondition = Input + "/InitialCondition";
            SQLite = Output + "/States.sqlite";
        }
    }
}
