using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLNode
{
    class Program
    {
        static void Main(string[] args)
        {

            string root = "D:/Dropbox/Summary/DA_50_9_EnKF/Output/apsimx/";
            // NodeCopy_Obs(root, "EnKF_old");
            Set_Pheno(root, "EnKF", "true");
            Set_Pheno(root, "OpenLoop", "false");

        }


        public static void NodeCopy_Obs(string root, string inputname)
        {
            // Create backup files.
            string inputfile = root + inputname + ".apsimx";
            string outputfile = root + inputname + "_out.apsimx";
            XmlDocument doc = new XmlDocument();
            doc.Load(@inputfile);
            XmlElement rootElement = doc.DocumentElement;

            //Select the node.
            XmlNodeList aNodes = doc.SelectNodes("Simulations/Simulation");
            XmlNode newNode = aNodes[0].SelectSingleNode("Control/Observations").CloneNode(true);

            // Yuxi: Insert a clone of the new node. Otherwise the node will be demolished.
            for (int i = 1; i < aNodes.Count; i++)
            {
                XmlNode oldNode = aNodes[i].SelectSingleNode("Control/Observations");
                aNodes[i].SelectSingleNode("Control").ReplaceChild(newNode.CloneNode(true), oldNode);
            }
            doc.Save(outputfile);
            Console.WriteLine("[" + outputfile + "]" + " Created!");
        }


        public static void Set_Pheno(string root, string inputname, string fixPheno)
        {
            // Create backup files.
            string inputfile = root + inputname + "_save.apsimx";
            string outputfile = root + inputname +"_FixPheno_"+ fixPheno + ".apsimx";
            XmlDocument doc = new XmlDocument();
            doc.Load(@inputfile);
            XmlElement rootElement = doc.DocumentElement;

            //Select the node.
            XmlNodeList aNodes = doc.SelectNodes("Simulations/Simulation");

            // Yuxi: Insert a clone of the new node. Otherwise the node will be demolished.
            for (int i = 0; i < aNodes.Count; i++)
            {
                aNodes[i].SelectSingleNode("Control/FixPhenology").InnerText = fixPheno;
            }
            doc.Save(outputfile);
            Console.WriteLine("[" + outputfile + "]" + " Created!");
        }
    }
}
