using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Models.Core;

namespace Models.DataAssimilation
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FolderInfo : Model
    {

        /// <summary>  </summary>
        [Description("Select Folder to store data.")]
        public string FolderName { get; set; }
        /// <summary>  </summary>
        public string Root { get; set; }
        /// <summary>  </summary>
        public string Input { get; set; }
        /// <summary>  </summary>
        public string Output { get; set; }
        /// <summary>  </summary>
        public string Origin { get; set; }
        /// <summary>  </summary>
        public string Example { get; set; }
        /// <summary>  </summary>
        public string Resources { get; set; }
        /// <summary>  </summary>
        public string Met { get; set; }
        /// <summary>  </summary>
        public string Obs { get; set; }
        /// <summary>  </summary>
        public string FileName { get; set; }
        /// <summary>  </summary>
        public string SQLite { get; set; }
        /// <summary>  </summary>
        public string SQLiteOutput { get; set; }
        /// <summary>  </summary>
        public FolderInfo()
        {
            //For APSIM.DA.
            FolderName = "D:\\Dropbox\\Dropbox\\Case_Study_1\\Case1_50_EnKF"; // Default directory: run from VS.

            FileInfo info = new FileInfo(".");      //Run from batch

            if (info.FullName.Contains("ApsimX.DA\\Bin"))
            {
                info = new FileInfo(FolderName);   //Run from VS
            }

            Root = info.ToString();
            Root = Path.GetFullPath(Root);
            Root = Root.Replace('\\', '/');

            Output = Root + "/Output";
            Origin = Root + "/Origin";
            Example = Root + "/Example";
            Met = Root + "/Met";
            Obs = Root + "/Obs";
            SQLite = Output + "/States.sqlite";
            SQLiteOutput = Output + "/StatesExtra.sqlite";
        }
    }
}
