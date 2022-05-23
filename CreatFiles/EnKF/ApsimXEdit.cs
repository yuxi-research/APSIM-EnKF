using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using System.IO;
using Shared;

namespace EnKF
{
    class ApsimXEdit
    {

        /// <summary>
        /// Generate simulation nodes from an original APSIMX file.
        /// Save to FimeName and OpenLoop.
        /// The OpenLoop and EnKF use exactly the same set of input files.
        /// </summary>
        /// <param name="ensembleSize"></param>
        /// <param name="rootPath"></param>
        public static void ApsimXFile(FolderInfo folder, PerturbControl control)
        {
            string fileName = folder.FileName_OL;

            string MetFile = "../Met" + "/Weather_Ensemble";    //Relative path.

            string originfile = folder.Origin + "/" + fileName + ".apsimx";
            string outfile = folder.Output + "/" + fileName + ".apsimx";
            string outfile2 = folder.Output + "/" + folder.FileName_DA + ".apsimx";

            XmlDocument doc = new XmlDocument();
            doc.Load(@originfile);
            string nodeName = "Control";

            XmlElement root = doc.DocumentElement;

            //Select the node.
            XmlNode aNode, cNode;
            XmlNode[] bNodes = new XmlNode[control.EnsembleSize + 1];
            aNode = doc.SelectSingleNode("Simulations/Simulation");
            cNode = doc.SelectSingleNode("Simulations/ExplorerWidth");

            if (aNode.ChildNodes[0].InnerText == "Truth")
            {
                //Step 1: Add truth. Using relative path below.
                aNode.SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/Weather.met";
                aNode.SelectSingleNode("Control/EnsembleSize").InnerText = control.EnsembleSize.ToString();
                if (control.Fix_Pheno)
                    aNode.SelectSingleNode("Control/FixPhenology").InnerText = "true";
                else
                    aNode.SelectSingleNode("Control/FixPhenology").InnerText = "false";

                for (int j = 0; j < control.InitSW.Count(); j++)
                {
                    aNode.SelectSingleNode(nodeName + "/InitialConditions/InitialSW").ChildNodes[j].InnerText = control.InitSW[j].ToString();
                    aNode.SelectSingleNode(nodeName + "/InitialConditions/InitialSWError").ChildNodes[j].InnerText = control.InitSWError[j].ToString();
                    aNode.SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = control.DUL[j].ToString();
                    aNode.SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = control.WheatLL[j].ToString();
                    aNode.SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = control.LL15[j].ToString();
                    aNode.SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(control.DUL[j] + control.SAT_DUL, 1).ToString();
                    aNode.SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(control.LL15[j] + control.AirDry_LL, 0).ToString();
                }

                //Step 2: Generate OpenLoop parameters.

                double[] OpenLoopInitSW = new double[control.InitSW.Count()];
                double[] OpenLoopDUL = new double[control.DUL.Count()];
                double[] OpenLoopWheatLL = new double[control.WheatLL.Count()];
                double[] OpenLoopLL15 = new double[control.LL15.Count()];

                if (control.FixedOL)
                {
                    for (int i = 0; i < OpenLoopDUL.Count(); i++)
                    {
                        OpenLoopInitSW[i] = control.InitSWOpenLoop[i];
                        OpenLoopDUL[i] = control.DULOpenLoop[i];
                        OpenLoopWheatLL[i] = control.WheatLLOpenLoop[i];
                        OpenLoopLL15[i] = OpenLoopWheatLL[i];
                    }
                }
                else
                {
                    for (int i = 0; i < OpenLoopDUL.Count(); i++)
                    {
                        OpenLoopInitSW[i] = control.InitSW[i] + control.InitSWError[i] * Distribution.NormalRand();
                        OpenLoopDUL[i] = control.DUL[i] + control.DULError[i] * Distribution.NormalRand();
                        OpenLoopWheatLL[i] = control.WheatLL[i] + control.WheatLLError[i] * Distribution.NormalRand();
                        OpenLoopLL15[i] = OpenLoopWheatLL[i];
                    }
                }


                Shared.DataType.Matrix SWError = Distribution.MultiNormalRand(control.InitSWError, control.SW_Corr, control.EnsembleSize);

                //Step 3: Insert Ensemble nodes.
                for (int i = 0; i < control.EnsembleSize + 1; i++)
                {
                    double[] tempDUL = new double[OpenLoopDUL.Count()];
                    double[] tempLL = new double[OpenLoopWheatLL.Count()];
                    for (int j = 0; j < control.InitSW.Count(); j++)
                    {
                        tempDUL[j] = OpenLoopDUL[j] + control.DULError[j] * Distribution.NormalRand();
                        tempLL[j] = OpenLoopWheatLL[j] + control.WheatLLError[j] * Distribution.NormalRand();
                    }

                    bNodes[i] = aNode.CloneNode(true);

                    if (i != control.EnsembleSize)
                    {   //Ensembles.
                        //Weather file.
                        bNodes[i].ChildNodes[0].InnerText = "Ensemble" + i.ToString();

                        if (control.Perturb_Weather)
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = MetFile + i.ToString() + ".met";
                        else
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/Weather.met";

                        //Cultivar.
                        bNodes[i].SelectSingleNode("Zone/Manager/Script/CultivarName").InnerText = "Truth";
                        if (bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Name").InnerText == "SowingRule")
                        {
                            if (control.Perturb_Cultivar)
                                bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "Custom" + i.ToString();
                            else
                                bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "Truth";
                        }

                        //Change soil parameter values.

                        double tempInitSW;
                        for (int j = 0; j < control.InitSW.Count(); j++)
                        {
                            tempInitSW = OpenLoopInitSW[j] + SWError.Arr[j, i];
                            tempInitSW = Distribution.Constrain(tempInitSW, tempLL[j] + control.AirDry_LL, tempDUL[j] + control.SAT_DUL);
                            //tempInitSW = OpenLoopInitSW[j];   //Test

                            bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSW").ChildNodes[j].InnerText = tempInitSW.ToString();
                            bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSWError").ChildNodes[j].InnerText = control.InitSWError[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = tempDUL[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = tempLL[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = tempLL[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(tempDUL[j] + control.SAT_DUL, 1).ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(tempLL[j] + control.AirDry_LL, 0).ToString();
                        }
                        #region OpenLoop test
                        ////Temp: set soil parameter ensembles to openloop.
                        //for (int j = 0; j < control.InitSW.Count(); j++)
                        //{
                        //    bNodes[i].SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = OpenLoopDUL[j].ToString();
                        //    bNodes[i].SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = OpenLoopWheatLL[j].ToString();
                        //    bNodes[i].SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = OpenLoopLL15[j].ToString();
                        //    bNodes[i].SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(OpenLoopDUL[j] + control.SAT_DUL, 1).ToString();
                        //    bNodes[i].SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(OpenLoopLL15[j] + control.AirDry_LL, 0).ToString();
                        //}
                        #endregion
                    }

                    //Step 4: Change openloop node.
                    else
                    {
                        //OpenLoop
                        //Weather file.
                        bNodes[i].ChildNodes[0].InnerText = "OpenLoop";
                        if (control.Perturb_Weather)
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/OpenLoop.met"; //Relative path.
                        else
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/Weather.met";
                        //Cultivar.
                        bNodes[i].SelectSingleNode("Zone/Manager/Script/CultivarName").InnerText = "Truth";
                        if (bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Name").InnerText == "SowingRule")
                        {
                            if (control.Perturb_Cultivar)
                                bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "OpenLoop";
                            else
                                bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "Truth";
                        }

                        //Change soil parameter values.
                        for (int j = 0; j < control.InitSW.Count(); j++)
                        {
                            bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSW").ChildNodes[j].InnerText = OpenLoopInitSW[j].ToString();
                            bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSWError").ChildNodes[j].InnerText = control.InitSWError[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = OpenLoopDUL[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = OpenLoopWheatLL[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = OpenLoopLL15[j].ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(OpenLoopDUL[j] + control.SAT_DUL, 1).ToString();
                            bNodes[i].SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(OpenLoopLL15[j] + control.AirDry_LL, 0).ToString();
                        }
                    }
                    // Insert node.
                    root.InsertBefore(bNodes[i], cNode);
                }


                doc.Save(outfile);
                Console.WriteLine("[" + outfile + "]" + " Created!");

            }
            else
            {
                throw new Exception("Cannot find node!");
            }

            XmlDocument doc2 = new XmlDocument();
            doc2.Load(@outfile);

            XmlElement root2 = doc2.DocumentElement;
            XmlNodeList currentNode = root2.SelectNodes("Simulation");
            XmlElement enkfNode = doc2.CreateElement("EnKF");
            enkfNode.InnerXml = "<Name>EnKF</Name>";

            foreach (XmlNode node in currentNode)
            {
                node.SelectSingleNode("Control/DAOption").InnerText = "EnKF";
                node.SelectSingleNode("Control/EnsembleSize").InnerText = control.EnsembleSize.ToString();
                node.SelectSingleNode("Control").ReplaceChild(enkfNode.Clone(), node.SelectSingleNode("Control/OpenLoop"));
            }

            doc2.Save(outfile2);
            Console.WriteLine("[" + outfile2 + "]" + " Created!");

        }


        #region Discarded old method.
        /// <summary>
        /// Generate simulation nodes from an original APSIMX file.
        /// Save to FimeName and OpenLoop.
        /// Discarded.
        /// </summary>
        /// <param name="ensembleSize"></param>
        /// <param name="rootPath"></param>
        public static void ApsimXFileOld(FolderInfo folder, PerturbControl control)
        {
            string[] OriginFileNames = new string[] {
                folder.FileName_OL,
                folder.FileName_DA};

            string MetFile = "../Met" + "/Weather_Ensemble";    //Relative path.

            foreach (string fileName in OriginFileNames)
            {
                string originfile = folder.Origin + "/" + fileName + ".apsimx";
                XmlDocument doc = new XmlDocument();
                doc.Load(@originfile);
                string nodeName = fileName;
                if (nodeName != "EnKF" && nodeName != "OpenLoop")
                {
                    throw new Exception("Node " + nodeName + " does not exist!");
                }
                XmlElement root = doc.DocumentElement;

                //Select the node.
                XmlNode aNode, cNode;
                XmlNode[] bNodes = new XmlNode[control.EnsembleSize + 1];
                aNode = doc.SelectSingleNode("Simulations/Simulation");
                cNode = doc.SelectSingleNode("Simulations/ExplorerWidth");

                if (aNode.ChildNodes[0].InnerText == "Truth")
                {
                    //Step 1: Add truth. Using relative path below.
                    aNode.SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/Weather.met";
                    for (int j = 0; j < control.InitSW.Count(); j++)
                    {
                        aNode.SelectSingleNode(nodeName + "/InitialConditions/InitialSW").ChildNodes[j].InnerText = control.InitSW[j].ToString();
                        aNode.SelectSingleNode(nodeName + "/InitialConditions/InitialSWError").ChildNodes[j].InnerText = control.InitSWError[j].ToString();
                        aNode.SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = control.DUL[j].ToString();
                        aNode.SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = control.WheatLL[j].ToString();
                        aNode.SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = control.LL15[j].ToString();
                        aNode.SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(control.DUL[j] + control.SAT_DUL, 1).ToString();
                        aNode.SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(control.LL15[j] + control.AirDry_LL, 0).ToString();
                    }

                    //Step 2: Generate OpenLoop parameters.

                    double[] OpenLoopInitSW = new double[control.InitSW.Count()];
                    double[] OpenLoopDUL = new double[control.DUL.Count()];
                    double[] OpenLoopWheatLL = new double[control.WheatLL.Count()];
                    double[] OpenLoopLL15 = new double[control.LL15.Count()];

                    if (control.FixedOL)
                    {
                        for (int i = 0; i < OpenLoopDUL.Count(); i++)
                        {
                            OpenLoopInitSW[i] = control.InitSWOpenLoop[i];
                            OpenLoopDUL[i] = control.DULOpenLoop[i];
                            OpenLoopWheatLL[i] = control.WheatLLOpenLoop[i];
                            OpenLoopLL15[i] = OpenLoopWheatLL[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < OpenLoopDUL.Count(); i++)
                        {
                            OpenLoopInitSW[i] = control.InitSW[i] + control.InitSWError[i] * Distribution.NormalRand();
                            OpenLoopDUL[i] = control.DUL[i] + control.DULError[i] * Distribution.NormalRand();
                            OpenLoopWheatLL[i] = control.WheatLL[i] + control.WheatLLError[i] * Distribution.NormalRand();
                            OpenLoopLL15[i] = OpenLoopWheatLL[i];
                        }
                    }

                    Shared.DataType.Matrix SWError = Distribution.MultiNormalRand(control.InitSWError, control.SW_Corr, control.EnsembleSize);

                    #region OpenLoopTest
                    //// Temp: set OpenLoop to truth
                    //for (int i = 0; i < OpenLoopDUL.Count(); i++)
                    //{
                    //    OpenLoopInitSW[i] = control.InitSW[i];
                    //    OpenLoopDUL[i] = control.DUL[i];
                    //    OpenLoopWheatLL[i] = control.WheatLL[i];
                    //    OpenLoopLL15[i] = OpenLoopWheatLL[i];
                    //}
                    #endregion

                    //Step 3: Insert Ensemble nodes.
                    for (int i = 0; i < control.EnsembleSize + 1; i++)
                    {
                        double[] tempDUL = new double[OpenLoopDUL.Count()];
                        double[] tempLL = new double[OpenLoopWheatLL.Count()];
                        for (int j = 0; j < control.InitSW.Count(); j++)
                        {
                            tempDUL[j] = OpenLoopDUL[j] + control.DULError[j] * Distribution.NormalRand();
                            tempLL[j] = OpenLoopWheatLL[j] + control.WheatLLError[j] * Distribution.NormalRand();
                        }

                        bNodes[i] = aNode.CloneNode(true);

                        if (i != control.EnsembleSize)
                        {   //Ensembles.
                            //Weather file.
                            bNodes[i].ChildNodes[0].InnerText = "Ensemble" + i.ToString();
                            //bNodes[i].SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/Weather.met";    //Temp.
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = MetFile + i.ToString() + ".met";

                            //Cultivar.
                            bNodes[i].SelectSingleNode("Zone/Manager/Script/CultivarName").InnerText = "Truth";
                            if (bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Name").InnerText == "SowingRule")
                            {
                                bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "Custom" + i.ToString();   //"Truth";// 
                            }

                            //Change soil parameter values.

                            double tempInitSW;

                            for (int j = 0; j < control.InitSW.Count(); j++)
                            {
                                tempInitSW = OpenLoopInitSW[j] + SWError.Arr[j, i];
                                tempInitSW = Distribution.Constrain(tempInitSW, tempLL[j] + control.AirDry_LL, tempDUL[j] + control.SAT_DUL);

                                bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSW").ChildNodes[j].InnerText = tempInitSW.ToString();
                                bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSWError").ChildNodes[j].InnerText = control.InitSWError[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = tempDUL[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = tempLL[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = tempLL[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(tempDUL[j] + control.SAT_DUL, 1).ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(tempLL[j] + control.AirDry_LL, 0).ToString();
                            }

                            #region OpenLoop test
                            ////Temp: set soil parameter ensembles to openloop.
                            //for (int j = 0; j < control.InitSW.Count(); j++)
                            //{
                            //    bNodes[i].SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = OpenLoopDUL[j].ToString();
                            //    bNodes[i].SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = OpenLoopWheatLL[j].ToString();
                            //    bNodes[i].SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = OpenLoopLL15[j].ToString();
                            //    bNodes[i].SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(OpenLoopDUL[j] + control.SAT_DUL, 1).ToString();
                            //    bNodes[i].SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(OpenLoopLL15[j] + control.AirDry_LL, 0).ToString();
                            //}
                            #endregion
                        }
                        else
                        {
                            //OpenLoop
                            //Weather file.
                            bNodes[i].ChildNodes[0].InnerText = "OpenLoop";
                            bNodes[i].SelectSingleNode("Weather/FileName").InnerText = "../Met" + "/OpenLoop.met"; //Relative path.

                            //Cultivar.
                            bNodes[i].SelectSingleNode("Zone/Manager/Script/CultivarName").InnerText = "Truth";
                            if (bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Name").InnerText == "SowingRule")
                            {
                                bNodes[i].SelectSingleNode("Zone/Manager").NextSibling.SelectSingleNode("Script/CultivarName").InnerText = "OpenLoop";
                            }

                            //Change soil parameter values.
                            for (int j = 0; j < control.InitSW.Count(); j++)
                            {
                                bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSW").ChildNodes[j].InnerText = OpenLoopInitSW[j].ToString();
                                bNodes[i].SelectSingleNode(nodeName + "/InitialConditions/InitialSWError").ChildNodes[j].InnerText = control.InitSWError[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/DUL").ChildNodes[j].InnerText = OpenLoopDUL[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/SoilCrop/LL").ChildNodes[j].InnerText = OpenLoopWheatLL[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/LL15").ChildNodes[j].InnerText = OpenLoopLL15[j].ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/SAT").ChildNodes[j].InnerText = Math.Min(OpenLoopDUL[j] + control.SAT_DUL, 1).ToString();
                                bNodes[i].SelectSingleNode("Zone/Soil/Water/AirDry").ChildNodes[j].InnerText = Math.Max(OpenLoopLL15[j] + control.AirDry_LL, 0).ToString();
                            }
                        }

                        // Insert node.
                        root.InsertBefore(bNodes[i], cNode);
                    }
                    doc.Save(folder.Output + "/" + fileName + ".apsimx");
                }
                else
                {
                    throw new Exception("Cannot find node!");
                }
            }
        }
        #endregion
    }

}
