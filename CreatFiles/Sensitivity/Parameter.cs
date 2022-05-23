using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.Data;
using System.Xml;
using System.IO;

namespace Sensitivity
{
    class Parameter
    {
        public static void EditPara(FolderStructure folder, DataTable table)
        {
            string sourceFile = folder.Origin + "/Wheat.xml";
            string targetFile = folder.Resources + "/Wheat.xml";
            string sumFile = folder.Cultivar + "/Wheat_Cultivar_Copy.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(@sourceFile);
            XmlNode root = doc.DocumentElement;
            XmlNodeList rootList = doc.ChildNodes;

            //"True" cultivar parameters.
            double[] para = new double[7] { 1.4, 1.8, 555.0, 20, 0.045, 580, 380 };
            string[] cmdText = new string[7] { "[Phenology].Vernalisation.VernSens",
                                            "[Phenology].Vernalisation.PhotopSens",
                                            "[Phenology].FloralInitiationToFlowering.Target.FixedValue",
                                            "[Grain].GrainsPerGramStem",
                                            "[Grain].MaxGrainSize",
                                            "[Phenology].StartGrainFillToEndGrainFill.Target.FixedValue",
                                            "[Phenology].EndOfJuvenileToFloralInitiation.Target.FixedValue"};
            //Define custom cultivar.
            XmlNode aNode = doc.CreateElement("Cultivar");
            XmlNode bNode = doc.CreateElement("Name");
            aNode.AppendChild(bNode);
            for (int i = 0; i < 7; i++)
            {
                bNode = doc.CreateElement("Command");
                aNode.AppendChild(bNode);
            }
            aNode.ChildNodes[0].InnerText = ("Custom");

            for (int i = 0; i < cmdText.Count(); i++)
            {
                aNode.ChildNodes[i + 1].InnerText = (cmdText[i] + " = " + para[i].ToString());
            }
            root.AppendChild(aNode);

            XmlNode[] cNodes = new XmlNode[table.Rows.Count];
            for(int paraIndex = 0; paraIndex < para.Count(); paraIndex++)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    cNodes[i] = aNode.CloneNode(true);
                    double paraRow = para[paraIndex] + Convert.ToDouble(table.Rows[i][paraIndex]);
                    //Use a loop.
                    cNodes[i].ChildNodes[0].InnerText = ("Custom_para" + paraIndex.ToString() + "_" + i.ToString());
                    for (int j = 0; j < para.Count(); j++)
                    {
                        if (j == paraIndex)
                        {
                            cNodes[i].ChildNodes[j + 1].InnerText = (cmdText[j] + " = " + paraRow.ToString());
                        }
                        else
                        {
                            cNodes[i].ChildNodes[j + 1].InnerText = (cmdText[j] + " = " + para[j].ToString());
                        }
                    }
                    root.AppendChild(cNodes[i]);
                }
            }
            doc.Save(targetFile);
            doc.Save(sumFile);
        }
    }
}
