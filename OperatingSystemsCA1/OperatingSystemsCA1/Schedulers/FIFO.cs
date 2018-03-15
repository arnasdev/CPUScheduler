using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatingSystemsCA1
{
    /// <summary>
    /// Orders jobs in the order they arrive
    /// </summary>
    public class FIFO : Scheduler
    {
        public FIFO()
        {
            schedulerTypeName = "FIFO";
        }

        public override void ProcessTimestep(int timeStep, ref List<Job> newJobsAtCurrentTimestep, bool moreJobsToArrive, out bool jobsFinishedReturn, out string currentRunningJobName, out string completeJob)
        {
            completeJob = null;
            AddClonedJobsToList(ref sortedJobsPool, newJobsAtCurrentTimestep);
            Clean(ref sortedJobsPool);

            if (currentRunningJob == Job.EmptyJob && !moreJobsToArrive && sortedJobsPool.Count() == 1)
            {
                jobsFinished = true;
            }


            if (timeStep == 0)
            {
                currentRunningJob = Job.EmptyJob;
                if(sortedJobsPool.Count > 0)
                {
                    currentRunningJob = sortedJobsPool[0];
                }    
            }
            else
            {
                if (currentRunningJob.isFinished && !moreJobsToArrive && sortedJobsPool.Count() == 0)
                {
                    currentRunningJob = Job.EmptyJob;
                }

                if (currentRunningJob.isFinished || currentRunningJob == Job.EmptyJob)
                {
                    if (sortedJobsPool.Count > 0)
                    {
                        currentRunningJob = sortedJobsPool[0];
                    }
                    else
                    {
                        if (!moreJobsToArrive)
                        {
                            jobsFinished = true;
                        }
                        else
                        {
                            currentRunningJob = Job.EmptyJob;
                        }
                    }
                }
                else
                {
                    //if(currentRunningJob == Job.EmptyJob && sorted)
                    //{

                    //}
                }
            }

            if (currentRunningJob.firstRunTime == -1)
            {
                currentRunningJob.firstRunTime = timeStep;
            }

            if (currentRunningJob != Job.EmptyJob)
            {
                if (currentRunningJob.timeLeft > 0)
                {
                    currentRunningJob.timeLeft--;
                }

                if (currentRunningJob.timeLeft <= 0)
                {
                    completeJob = currentRunningJob.name;
                    currentRunningJob.isFinished = true;

                    CalculateTurnaroundTime(timeStep, currentRunningJob, schedulerTypeName);
                    CalculateResponseTime(timeStep, currentRunningJob, schedulerTypeName);
                }
            }



            jobsFinishedReturn = jobsFinished;
            currentRunningJobName = currentRunningJob.name;
        }

        // Removes finished jobs
        public static void Clean(ref List<Job> jobsThatHaveArrived)
        {
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }

}
