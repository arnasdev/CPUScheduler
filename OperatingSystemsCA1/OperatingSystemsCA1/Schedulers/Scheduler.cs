using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatingSystemsCA1
{
    /// <summary>
    /// Base class for any sorting algorithms which decide on arrival time
    /// </summary>
    public class Scheduler
    {
        protected bool jobsFinished = false;        // Are all jobs for this scheduler finished
        protected Job currentRunningJob = null;     // The current running job
        protected List<Job> sortedJobsPool = null;  // A list of currently queued jobs, sorted to the specific algorithm required for each scheduler
        public string schedulerTypeName = string.Empty;

        // Cannot create an instance of this base class, subclasses used instead
        protected Scheduler()
        {
            sortedJobsPool = new List<Job>();
        }

        // Necessary to make shallow copies of each job before adding them to the necessary schedulers, otherwise all schedulers work off the same job objects
        public static void AddClonedJobsToList(ref List<Job> jobList, List<Job> jobsToAdd)
        {
            foreach (Job j in jobsToAdd)
            {
                jobList.Add(j.ShallowCopy());
            }
        }

        public static void CalculateTurnaroundTime(int timeStep, Job finishedJob, string schedulerTypeName)
        {
            int turnAroundTime = timeStep + 1 - finishedJob.arrivalTime;
            Job.turnAroundTimes[finishedJob.name][schedulerTypeName] = turnAroundTime;
        }

        public static void CalculateResponseTime(int timeStep, Job finishedJob, string schedulerTypeName)
        {
            int responseTime = finishedJob.firstRunTime - finishedJob.arrivalTime;
            Job.responseTimes[finishedJob.name][schedulerTypeName] = responseTime;
        }

        public virtual void ProcessTimestep(int timeStep, ref List<Job> newJobsAtCurrentTimestep, bool moreJobsToArrive, out bool jobsFinished, out string currentRunningJobName, out string completeJob)
        {
            jobsFinished = false;
            currentRunningJobName = null;
            completeJob = null;
        }
    }
}
