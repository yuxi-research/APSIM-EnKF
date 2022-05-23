using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sensitivity
{
    class Test
    {
        public static void NewFolderTest()
        {
            FileInfo info = new FileInfo("../../../../Summary/" + "ExampleSens");    //    ../Debug../bin../ProjectFolder../SolutionFolder
            FolderStructure folder = new FolderStructure(info);
        }

        public static void ReadTest()
        {
            FileInfo info = new FileInfo("../../../../Summary/" + "ExampleSens");    //    ../Debug../bin../ProjectFolder../SolutionFolder
            FolderStructure folder = new FolderStructure(info);
            //folder = General.CreateFolder(folder, 2);
            //Console.WriteLine("Please place original files to Origin folder");
            //Edit.EditAllInput(folder);
        }
    }
}
