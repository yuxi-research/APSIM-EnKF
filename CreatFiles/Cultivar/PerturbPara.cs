using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Shared;

namespace Cultivar
{
    class PerturbPara
    {
        /// <summary>
        /// Perturb parameters with addtive methods.
        /// Read baseline parameters and errors from control.
        /// Save [Summary/Inut/Cultivar] only.
        /// FixedOL = true when open-loop cultivar is pre-determined.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="control"></param>
        /// <param name="cropType"></param>
        public static void EditPara(FolderInfo folder, PerturbControl control, string cropType)
        {
            if (cropType != "Wheat" && cropType != "Maize") { throw new Exception("Crop type not defined."); }
            string sourceFile = folder.Origin + "/" + cropType + ".xml";
            string targetFile = folder.Cultivar + "/" + cropType + ".xml";
            string sumFile = folder.Cultivar + "/Parameters.txt";

            double[] para = control.ParaBaseline;
            double[] paraError = control.ParaError;

            XmlDocument doc = new XmlDocument();
            doc.Load(@sourceFile);
            XmlNode root = doc.DocumentElement;
            XmlNodeList rootList = doc.ChildNodes;

            string[] cmdText = new string[9] { "[Phenology].Vernalisation.VernSens",
                                               "[Phenology].Vernalisation.PhotopSens",
                                               "[Phenology].EndOfJuvenileToFloralInitiation.Target.FixedValue",
                                               "[Phenology].FloralInitiationToFlowering.Target.FixedValue",
                                               "[Phenology].FloweringToStartGrainFill.Target.FixedValue",
                                               "[Phenology].StartGrainFillToEndGrainFill.Target.FixedValue",
                                               "[Grain].PotentialGrainFillingRate",
                                               "[Grain].PotentialGrainGrowthRate",
                                               "[Grain].PotentialGrainNFillingRate",
            };

            #region Setp 1. Define "truth" cultivar.

            XmlNode aNode = doc.CreateElement("Cultivar");
            XmlNode bNode = doc.CreateElement("Name");
            aNode.AppendChild(bNode);
            for (int i = 0; i < cmdText.Count(); i++)
            {
                bNode = doc.CreateElement("Command");
                aNode.AppendChild(bNode);
            }

            aNode.ChildNodes[0].InnerText = ("Truth");
            for (int j = 0; j < cmdText.Count(); j++)
            {
                aNode.ChildNodes[j + 1].InnerText = (cmdText[j] + " = " + para[j].ToString());
            }
            root.AppendChild(aNode);

            //Write colume names and Truth to summary.
            FileStream fs = new FileStream(sumFile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write("Parameters\t");
            foreach (string value in cmdText)
            {
                sw.Write("{0}\t", value);
            }
            sw.Write("\n");
            sw.Write("Truth\t");
            foreach (double value in para)
            {

                sw.Write("{0}\t", value);
            }
            sw.Write("\n");

            #endregion

            #region Step 2. Define "OpenLoop" cultivar.

            XmlNode dNode = aNode.CloneNode(true);

            double[] para0 = new double[para.Count()];

            if (control.FixedOL)
            {
                for (int j = 0; j < para0.Count(); j++)
                {
                    para0[j] = control.ParaOpenLoop[j];
                }
            }
            else
            {
                for (int j = 0; j < para0.Count(); j++)
                {
                    para0[j] = para[j] + control.ParaError[j] * Distribution.NormalRand();
                }
            }

            dNode.ChildNodes[0].InnerText = ("OpenLoop");
            for (int j = 0; j < cmdText.Count(); j++)
            {
                dNode.ChildNodes[j + 1].InnerText = (cmdText[j] + " = " + para0[j].ToString());
            }
            root.AppendChild(dNode);

            //Write OpenLoop to a txt file
            sw.Write("OpenLoop\t");
            foreach (double value in para0)
            {
                sw.Write("{0}\t", value);
            }
            sw.Write("\n");

            #endregion

            #region Step 3. Perturb ensembles on the basis of open-loop values.

            XmlNode[] cNodes = new XmlNode[control.EnsembleSize];
            for (int i = 0; i < control.EnsembleSize; i++)
            {
                cNodes[i] = aNode.CloneNode(true);
                double[] para1 = new double[para0.Count()];
                for (int j = 0; j < para0.Count(); j++)
                {
                    para1[j] = para0[j] + control.ParaError[j] * Distribution.NormalRand();
                }

                cNodes[i].ChildNodes[0].InnerText = ("Custom" + i.ToString());
                for (int j = 0; j < cmdText.Count(); j++)
                {
                    cNodes[i].ChildNodes[j + 1].InnerText = (cmdText[j] + " = " + para1[j].ToString());
                }
                root.AppendChild(cNodes[i]);

                //Write Ensembles to a txt file
                sw.Write("Ensemble{0}\t", i.ToString());
                foreach (double value in para1)
                {
                    sw.Write("{0}\t", value);
                }
                sw.Write("\n");
            }

            #endregion

            sw.Flush();
            sw.Close();
            fs.Close();
            doc.Save(targetFile);
            doc.Save(folder.Cultivar + "/" + cropType + ".xml");
        }
    }
}
