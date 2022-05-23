using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EnsembleKalmanFilter;
using System.Data;


namespace Sensitivity
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo info = new FileInfo("../../../../Summary" + "/ExampleSens");    //    ../Debug../bin../ProjectFolder../SolutionFolder
            FolderStructure folder = new FolderStructure(info);
            //folder = General.CreateFolder(folder, 10);

            EditAllInput(folder);
            Console.WriteLine("Press ENTER to contine...");
            Console.ReadLine();
        }

        public static void EditAllTest()
        {
            FileInfo info = new FileInfo("../../../../Summary" + "/ExampleSens");    //    ../Debug../bin../ProjectFolder../SolutionFolder
            FolderStructure folder = new FolderStructure(info);
            //folder = General.CreateFolder(folder, 2);
            //Console.WriteLine("Please place original files to Origin folder");
            EditAllInput(folder);
            Console.WriteLine("Press ENTER to contine...");
        }

        public static void EditAllInput(FolderStructure folder)
        {
            //Control            
            Control ctrl = new Control();

            //// Edit Weather
            System.Data.DataTable tableWeather = ctrl.ReadControl(folder, "Weather");
            // Edit Met1
            string[] metNames = new string[] { "radn", "rain", "evap", "vp" };
            DataTable selectTable1 = ctrl.SelectControl(tableWeather, metNames);
            Weather.EditMet(folder, selectTable1);
            //EditApsim.Weather(folder, selectTable1);

            //Edit Met2
            string[] tempNames = new string[] { "maxt", "mint" };
            DataTable selectTable2 = ctrl.SelectControl(tableWeather, tempNames);
            //Weather.EditMet2(folder, selectTable2, tempNames);
            //EditApsim.Weather(folder, selectTable2);

            ////Edit CultivarParameter
            //System.Data.DataTable tablePara = ctrl.ReadControl(folder, "Cultivar");
            ////Parameter.EditPara(folder, tablePara);
            //EditApsim.Parameter(folder, tablePara);
        }
    }
}
