﻿namespace UserInterface.Commands
{
    using APSIM.Shared.Utilities;
    using Models.Core;
    using Models.Core.Runners;
    using Presenters;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Media;
    using System.Timers;

    class RunCommand : ICommand
    {
        /// <summary>The job to run</summary>
        private List<JobManager.IRunnable> jobs = null;

        /// <summary>The name of the job</summary>
        private string jobName;

        /// <summary>The job manager running the simulations.</summary>
        private JobManager jobManager;

        /// <summary>The explorer presenter.</summary>
        private ExplorerPresenter explorerPresenter;

        /// <summary>The stop watch we can use to time the runs.</summary>
        private Timer timer = null;

        /// <summary>The stop watch we can use to time the runs.</summary>
        private Stopwatch stopwatch = new Stopwatch();

        /// <summary>Retuns true if simulations are running.</summary>
        public bool IsRunning { get; set; }


        /// <summary>Constructor</summary>
        /// <param name="job">The job to run.</param>
        /// <param name="name">The name of the job</param>
        /// <param name="presenter">The explorer presenter.</param>
        /// <param name="multiProcess">Use the multi-process runner?</param>
        public RunCommand(JobManager.IRunnable job, string name, ExplorerPresenter presenter, bool multiProcess)
        {
            jobs = new List<JobManager.IRunnable>();
            this.jobs.Add(job);
            this.jobName = name;
            this.explorerPresenter = presenter;

            if (multiProcess)
                jobManager = new JobManagerMultiProcess();
            else
                jobManager = new JobManager();
        }

        /// <summary>Constructor</summary>
        /// <param name="job">The job to run.</param>
        /// <param name="name">The name of the job</param>
        /// <param name="presenter">The explorer presenter.</param>
        /// <param name="multiProcess">Use the multi-process runner?</param>
        public RunCommand(List<JobManager.IRunnable> jobs, string name, ExplorerPresenter presenter, bool multiProcess)
        {
            this.jobs = jobs;
            this.jobName = name;
            this.explorerPresenter = presenter;

            if (multiProcess)
                jobManager = new JobManagerMultiProcess();
            else
                jobManager = new JobManager();
        }

        /// <summary>Perform the command</summary>
        public void Do(CommandHistory CommandHistory)
        {
            IsRunning = true;

            stopwatch.Start();
                
            jobs.ForEach(job => jobManager.AddJob(job));
            jobManager.Start(waitUntilFinished: false);

            timer = new Timer();
            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Elapsed += OnTimerTick;
            timer.Start();
        }

        /// <summary>
        /// Undo the command
        /// </summary>
        public void Undo(CommandHistory CommandHistory)
        {
        }

        /// <summary>
        /// This gets called everytime a simulation completes. When all are done then
        /// invoke each model's OnAllCompleted method.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private string GetErrorsFromSimulations()
        {
            string errorMessage = null;
            foreach (JobManager.IRunnable job in jobs)
                foreach (Exception error in jobManager.Errors(job))
                    errorMessage += error.ToString() + Environment.NewLine;

            return errorMessage;
        }

        /// <summary>
        /// The timer has ticked. Update the progress bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            int numSimulations = jobManager.CountJobTypeInQueue<Simulation>();
            double percentComplete = jobManager.PercentComplete;

            if (numSimulations > 0)
            {
                explorerPresenter.MainPresenter.ShowMessage(jobName + " running (" +
                         (numSimulations) + ")", Models.DataStore.ErrorLevel.Information);

                explorerPresenter.MainPresenter.ShowProgress(Convert.ToInt32(percentComplete));
            }
            if (percentComplete == 100)
            {
                timer.Stop();
                stopwatch.Stop();
                jobManager.Stop();

                string errorMessage = GetErrorsFromSimulations();
                if (errorMessage == null)
                    explorerPresenter.MainPresenter.ShowMessage(jobName + " complete "
                            + " [" + stopwatch.Elapsed.TotalSeconds.ToString("#.00") + " sec]", Models.DataStore.ErrorLevel.Information);
                else
                    explorerPresenter.MainPresenter.ShowMessage(errorMessage, Models.DataStore.ErrorLevel.Error);

                SoundPlayer player = new SoundPlayer();
                if (DateTime.Now.Month == 12 && DateTime.Now.Day == 25)
                    player.Stream = Properties.Resources.notes;
                else
                    player.Stream = Properties.Resources.success;
                player.Play();
                IsRunning = false;
                jobManager = null;
                jobs = null;
            }
        }
    }
}
