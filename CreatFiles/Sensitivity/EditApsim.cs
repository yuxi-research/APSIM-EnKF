using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Data;

namespace Sensitivity
{
    class EditApsim
    {
        public static void Weather(FolderStructure folder, DataTable table)
        {
            string originPath = Directory.GetFiles(folder.Origin, "*.apsimx")[0];
            string originName = Path.GetFileNameWithoutExtension(@originPath);
            string absolutePath = Path.GetFullPath(folder.Met);
            absolutePath = absolutePath.Replace('\\', '/');

            //string absolutePath = Path.GetFileName(@folder.Met);
            for (int j = 0; j < table.Columns.Count; j++)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@originPath);
                XmlElement root = doc.DocumentElement;
                //Show node Name and FirstChild.InnerText.
                //Select the node.
                XmlNode aNode, cNode;
                XmlNode[] bNodes = new XmlNode[table.Rows.Count];
                aNode = doc.SelectSingleNode("Simulations/Simulation");
                cNode = doc.SelectSingleNode("Simulations/ExplorerWidth");
                if (aNode.ChildNodes[0].InnerText == "Truth")
                {
                    string columnName = table.Columns[j].ToString();
                    if (columnName == "maxt")
                    {
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            bNodes[i] = aNode.CloneNode(true);
                            bNodes[i].ChildNodes[0].InnerText = "temp" + i.ToString();
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = absolutePath + "/temp" + i.ToString() + ".met";
                            root.InsertBefore(bNodes[i], cNode);
                            doc.Save(folder.Input + "/Weather_temp" + ".apsimx");
                        }
                    }
                    else if (columnName == "mint") { continue; }
                    else
                    {
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            bNodes[i] = aNode.CloneNode(true);
                            bNodes[i].ChildNodes[0].InnerText = columnName + i.ToString();
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = absolutePath + "/" + columnName + i.ToString() + ".met";
                            root.InsertBefore(bNodes[i], cNode);
                            doc.Save(folder.Input + "/Weather_" + columnName + ".apsimx");
                        }
                    }
                }
                else
                {
                    throw new Exception("Cannot find node!");
                }
            }
        }
        public static void Parameter(FolderStructure folder, DataTable table)
        {
            string originPath = Directory.GetFiles(folder.Origin, "*.apsimx")[0];
            string originName = Path.GetFileNameWithoutExtension(@originPath);
            for (int j = 0; j < table.Columns.Count; j++)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@originPath);
                XmlElement root = doc.DocumentElement;
                //Show node Name and FirstChild.InnerText.
                //Select the node.
                XmlNode aNode, cNode;
                XmlNode[] bNodes = new XmlNode[table.Rows.Count];
                aNode = doc.SelectSingleNode("Simulations/Simulation");
                cNode = doc.SelectSingleNode("Simulations/ExplorerWidth");
                if (aNode.ChildNodes[0].InnerText == "Truth")
                {
                    string columnName = table.Columns[j].ToString();
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        bNodes[i] = aNode.CloneNode(true);
                        bNodes[i].ChildNodes[0].InnerText = "Custom_para" + j.ToString() + "_" + i.ToString();

                        if (bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Name").InnerText == "SowingRule") 
                        {
                            bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "Custom_para" + j.ToString() + "_" + i.ToString();
                        }
                        else { throw new Exception("Cannot find node!"); }

                        XmlNodeList nodeList = bNodes[i].SelectNodes("Zone/Manager");
                        for (int index = 0; index < nodeList.Count; index++)
                        {
                            if (nodeList[0].SelectSingleNode("Name").InnerText == "SowingRule")
                            {                                
                                break;
                            }
                        }
                        root.InsertBefore(bNodes[i], cNode);
                        doc.Save(folder.Input + "/Cultivar_" + columnName + ".apsimx");
                    }
                }
                else
                {
                    throw new Exception("Cannot find node!");
                }
            }
        }
    }
}