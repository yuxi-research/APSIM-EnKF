using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using Models.Core;
using System.Threading;
using APSIM.Shared.Utilities;
using DataAssimilation;
using System.Data.SQLite;
using System.Media;

namespace Models
{
    class ProjectOld
    {
        /// <summary>
        /// The old entry point (with the project DataAssimilation).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void MainOld(string[] args)
       {           
            Stopwatch timer = new Stopwatch();
            timer.Start();
            FolderStructure folder = new FolderStructure(0);
            DAControl control = new DAControl(0);
            SQLiteConnection.CreateFile(folder.SQLite);
            SQLiteConnection sqlCon = new SQLiteConnection("Data Source=" + folder.SQLite);
            sqlCon.Open();

            for (int i = 0; i < control.StateNames.Count(); i++)
            {
                CreateSQLiteTable(control.EnsembleSize, control.StateNames[i], sqlCon, folder);
            }
            sqlCon.Close();

            MainNew(folder, args);
            timer.Stop();
            Console.WriteLine("Total time elapsed is {0}", timer.Elapsed.TotalSeconds.ToString("0.00 sec"));
            PlaySimpleSound();
            Console.ReadLine();
        }

        public static void CreateSQLiteTable(int ensembleSize, string tableName, SQLiteConnection sqlCon,FolderStructure floder)
        {

            string sqlStr = "CREATE TABLE " + tableName + " (ID int PRIMARY KEY, Day int, Month int, Year int, Truth double, ";
            for (int i = 0; i < ensembleSize; i++)
            {
                sqlStr += "Ensemble" + i + "_Prior double, ";
            }
            sqlStr += "PriorMean double, ";
            sqlStr += "Obs double, ";
            sqlStr += "P double, ";
            sqlStr += "K double";
            for (int i = 0; i < ensembleSize; i++)
            {
                sqlStr += ", Ensemble" + i + "_Posterior double";
            }
            for (int i = 0; i < ensembleSize; i++)
            {
                sqlStr += ", Ensemble" + i + "_Obs double";
            }
            sqlStr += ", PosteriorMean double)";

            SQLiteCommand command = new SQLiteCommand(sqlStr, sqlCon);
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Play a sound.
        /// </summary>
        public static void PlaySimpleSound()
        {
            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Alarm04.wav");
            simpleSound.Play();
            Thread.Sleep(4000);
        }
        /// <summary>
        /// Modified from MainOld.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int MainNew(FolderStructure folder,string[] args)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), "ApsimX");
            Directory.CreateDirectory(tempFolder);
            Environment.SetEnvironmentVariable("TMP", tempFolder, EnvironmentVariableTarget.Process);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Manager.ResolveManagerAssembliesEventHandler);

            int exitCode = 0;
            try
            {
                //string fileName = folder.Input + "/" + folder.FileName + ".apsimx";
                string fileName = null;
                // Check the command line arguments.
                if (args.Length >= 1)
                {
                    fileName = args[0];
                }
                else
                {
                    fileName = folder.Input + "/" + folder.FileName + ".apsimx";
                }

                //if (args.Length < 1 || args.Length > 4)
                //{
                //    throw new Exception("Usage: ApsimX ApsimXFileSpec [/Recurse] [/Network] [/IP:<server IP>]");
                //}

                Stopwatch timer = new Stopwatch();
                Console.WriteLine(Path.GetFullPath(fileName));
                timer.Start();

                // Create a instance of a job that will go find .apsimx files. Then
                // pass the job to a job runner.
                JobManager.IRunnable job;
                if (fileName.Contains('*') || fileName.Contains('?'))
                    job = Runner.ForFolder(fileName, true, false);
                else
                    job = Runner.ForFile(fileName, false);
                JobManager jobManager = new JobManager();
                jobManager.AddJob(job);
                jobManager.Start(waitUntilFinished: true);

                List<Exception> errors = jobManager.Errors(job);
                if (errors.Count > 0)
                {
                    errors.ForEach(e => Console.WriteLine(e.ToString() + Environment.NewLine));
                    exitCode = 1;
                }
                else
                    exitCode = 0;
                timer.Stop();
                Console.WriteLine("Finished running simulations. Duration " + timer.Elapsed.TotalSeconds.ToString("#.00") + " sec.");
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
    }
}
