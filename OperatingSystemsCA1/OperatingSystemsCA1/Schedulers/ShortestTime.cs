using System.Collections.Generic;
using System.Linq;

namespace OperatingSystemsCA1
{
    /// <summary>
    /// Orders jobs by the shortest time to completion, sorts the shortest jobs at each new timestep
    /// </summary>
    public class ShortestTime : Scheduler
    {
        public ShortestTime()
        {
            schedulerTypeName = "STCF";
        }

        public override void ProcessTimestep(int timeStep, ref List<Job> newJobsAtCurrentTimestep, bool moreJobsToArrive, out bool jobsFinishedReturn, out string currentRunningJobName, out string completeJob)
        {
            completeJob = null;
            AddClonedJobsToList(ref sortedJobsPool, newJobsAtCurrentTimestep);
            SortByShortestTimeAndClean(ref sortedJobsPool);

            if (timeStep == 0)
            {
                currentRunningJob = sortedJobsPool[0];
            }
            else
            {
                if (currentRunningJob.isFinished)
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
                    }
                }



                if (!jobsFinished)
                {
                    if (sortedJobsPool.Count() > 0)
                    {
                        if (sortedJobsPool[0].timeLeft < currentRunningJob.timeLeft || currentRunningJob == Job.EmptyJob)
                        {
                            currentRunningJob = sortedJobsPool[0]; // current job switches to shortest
                        }
                    }
                    else
                    {
                        currentRunningJob = Job.EmptyJob;
                    }
                }
                else
                {
                    currentRunningJob = Job.EmptyJob;
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

            if (currentRunningJob == Job.EmptyJob && !moreJobsToArrive && sortedJobsPool.Count() == 1)
            {
                jobsFinished = true;
            }

            jobsFinishedReturn = jobsFinished;
            currentRunningJobName = currentRunningJob.name;
        }

        public static void SortByShortestTimeAndClean(ref List<Job> jobsThatHaveArrived)
        {
            // Sort our jobs by shortest time left
            jobsThatHaveArrived.Sort(delegate (Job j1, Job j2) {
                if (j1.timeLeft < j2.timeLeft) return -1;
                else if (j1.timeLeft > j2.timeLeft) return 1;
                else return 0;
            });

            // And clean up completed jobs, maybe do something with them later
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }

}
