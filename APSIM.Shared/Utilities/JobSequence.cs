// -----------------------------------------------------------------------
// <copyright file="JobSequence.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace APSIM.Shared.Utilities
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;

    /// <summary>
    /// A composite class for a sequence of jobs that will be run sequentially.
    /// If an error occurs in any job, then subsequent jobs won't be run.
    /// </summary>
    public class JobSequence : JobManager.IRunnable
    {
        /// <summary>A list of jobs that will be run in sequence.</summary>
        public List<JobManager.IRunnable> Jobs { get; set; }

        /// <summary>Constructor</summary>
        public JobSequence()
        {
            Jobs = new List<JobManager.IRunnable>();
        }

        /// <summary>Called to start the job.</summary>
        /// <param name="jobManager">The job manager running this job.</param>
        /// <param name="workerThread">The thread this job is running on.</param>
        public void Run(JobManager jobManager, BackgroundWorker workerThread)
        {
            for (int j = 0; j < Jobs.Count; j++)
            {
                // Add job to the queue
                jobManager.AddChildJob(this, Jobs[j]);

                // Wait for it to be completed.
                while (!jobManager.IsJobCompleted(Jobs[j]))
                    Thread.Sleep(200);
            }            
        }
    }
}
