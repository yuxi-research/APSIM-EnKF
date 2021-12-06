using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataAssimilation
{
    [Serializable]
    public class Test
    {
        public static void Read()
        {
            FolderStructure folder = new FolderStructure(1);
            string str = folder.Obs + "/" + "LAI" + "_Obs.csv";
            string str1 = str.Replace("/", "\\");
            bool exist = File.Exists(str);
            if (File.Exists(str)) 
            {
                Console.WriteLine("Exist.");
            }
            else
            {
                Console.WriteLine("Not Exist!");
            }
            Console.ReadLine();
        }
    }
}
