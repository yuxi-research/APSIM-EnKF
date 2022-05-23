using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using System.IO;

namespace XMLEdit
{
    class XMLEdit
    {

        /// <summary>
        /// Generate simulation nodes from an original APSIMX file.
        /// Save to FimeName and OpenLoop.
        /// The OpenLoop and EnKF use exactly the same set of input files.
        /// </summary>
        /// <param name="ensembleSize"></param>
        /// <param name="rootPath"></param>
        public static void ObsEdit(string inputfile, string outputfile, string obs_name, string obs_error, string obs_options)
        {
            // Create backup files.


            XmlDocument doc = new XmlDocument();
            doc.Load(@inputfile);

            //Select the node.
            XmlNodeList aNodes = doc.SelectNodes("Simulations/Simulation");

            for ( int i = 0; i < aNodes.Count; i++)
            {
                XmlNodeList bNodes = aNodes[i].SelectNodes("Control/Observations/StateNamesObs/string");
                int index = -1;
                for (int j=0; j < bNodes.Count; j++)
                {
                    if (obs_name == bNodes[j].InnerText)
                    {
                        index = j;
                        break;
                    }
                }
                aNodes[i].SelectNodes("Control/Observations/ObsError/double")[index].InnerText=obs_error;
                aNodes[i].SelectNodes("Control/Observations/ObsErrorOption/int")[index].InnerText = obs_options;
            }
            doc.Save(outputfile);
            Console.WriteLine("[" + outputfile + "]" + " Created!");

        }
    }

}
