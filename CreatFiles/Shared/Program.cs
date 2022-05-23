using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace Shared
{
    class Program
    {
        /// <summary>
        /// The entry.
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {

            //string root = " D:\\Dropbox\\Summary\\DA_50_9_EnKF";
            string root = ".";
            int exitCode = 0;
            try
            {
                if (args.Count() > 0)
                {
                    FileInfo info = new FileInfo(args[0]);
                    root = info.ToString();
                }
                FolderInfo folder = new FolderInfo(root);
                MyXml.Serialize(folder.Origin);

                Console.WriteLine("Finished!");
                Thread.Sleep(500);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                exitCode = 1;
            }

            if (exitCode != 0)
                Console.WriteLine("ERRORS FOUND!!");

            return exitCode;


        }

        #region Test
        /// <summary>
        /// 
        /// </summary>
        static void Test()
        {
            double[,] std = new double[,] { { 1, 0.09, 0.08, 0.07, 0.06, 0.05, 0.04 } };


            double[,] corr = new double[,]{ { 1, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1 },
                                            { 0.6, 1, 0.6, 0.5, 0.4, 0.3, 0.2 },
                                            { 0.5 ,0.6, 1, 0.6, 0.5, 0.4, 0.3 },
                                            { 0.4, 0.5 ,0.6, 1, 0.6, 0.5, 0.4 },
                                            { 0.3, 0.4, 0.5 ,0.6, 1, 0.6, 0.5 },
                                            { 0.2, 0.3, 0.4, 0.5 ,0.6, 1, 0.6 },
                                            { 0.1, 0.2, 0.3, 0.4, 0.5 ,0.6, 1 } };

            DataType.Matrix Corr = new DataType.Matrix(corr);

            DataType.Matrix Std = new DataType.Matrix(std);

            DataType.Matrix Cov = Std.Transpose() * Std;

            for (int i = 0; i < Cov.Row; i++)
            {
                for (int j = 0; j < Cov.Col; j++)
                {
                    Cov.Arr[i, j] *= corr[i, j];
                }
            }

            DataType.Matrix Dec = Cov.Decompose();

            Console.ReadLine();
        }
        #endregion
    }
}
