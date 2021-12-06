// -----------------------------------------------------------------------
// <copyright file="JobParallel.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace APSIM.Shared.Utilities
{
    using System.Collections.Generic;
    using System.ComponentModel;
    /// <summary>
    /// A composite class for a sequence of jobs that will be run asynchronously.
    /// If an error occurs in any job, then this job will also produce an error.
    /// </summary>
    public class JobParallel : JobManager.IRunnable
    {
        /// <summary>A list of jobs that will be run in sequence.</summary>
        public List<JobManager.IRunnable> Jobs { get; set; }

        /// <summary>Constructor</summary>
        public JobParallel()
        {
            Jobs = new List<JobManager.IRunnable>();
        }

        /// <summary>Called to start the job.</summary>
        /// <param name="jobManager">The job manager running this job.</param>
        /// <param name="workerThread">The thread this job is running on.</param>
        public void Run(JobManager jobManager, BackgroundWorker workerThread)
        {
            // Add all jobs to the queue
            foreach (JobManager.IRunnable job in Jobs)
                jobManager.AddChildJob(this, job);
        }

    }
}
