using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace Shared
{
    public class MyXml
    {
        public static void Serialize(string out_dir)
        {
            PerturbControl control = new PerturbControl();
            // Make sure Perturb_Soil was set to 'true'.
            control.Perturb_Soil = true;
            control.SetSoil();
            XmlSerializer mySerializer = new XmlSerializer(typeof(PerturbControl));
            StreamWriter myWriter = new StreamWriter(out_dir + "/Perturb_Control.xml");
            mySerializer.Serialize(myWriter, control);
            myWriter.Close();
            Console.WriteLine("[" + out_dir + "/Perturb_Control.xml] created.");
        }
        public static PerturbControl Deserialize(string in_dir = "./Origin")
        {
            PerturbControl control;
            if (File.Exists(in_dir + "/Perturb_Control.xml"))
            {
                StreamReader myReader = new StreamReader(in_dir + "/Perturb_Control.xml");
                XmlSerializer mySerializer = new XmlSerializer(typeof(PerturbControl));
                control = mySerializer.Deserialize(myReader) as PerturbControl;
                control.UpdateSoil();
            }
            else
            {
                Console.WriteLine("Warning: Xml file not found. Default control setting applied.");
                control = new PerturbControl();
            }
            return control;

        }
    }
}
