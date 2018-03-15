using System.Collections.Generic;
using System.Linq;

namespace OperatingSystemsCA1
{
    /// <summary>
    /// Runs jobs in in a given timeslice, allows to give all jobs equal priority
    /// </summary>
    public class RoundRobin : Scheduler
    {
        private int currentSlice = 0;
        public int timeSlice;

        public RoundRobin(int timeSlice)
        {
            this.timeSlice = timeSlice;
            this.schedulerTypeName = "RR";
        }

        public RoundRobin(int timeSlice, int rrID)
        {
            this.timeSlice = timeSlice;
            this.schedulerTypeName = "RR" + rrID;
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
                currentRunningJob = sortedJobsPool[0];
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
                    if (currentSlice == timeSlice)
                    {
                        currentSlice = 0;
                        FirstToBack(ref sortedJobsPool);
                        currentRunningJob = sortedJobsPool[0];
                    }
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
                    currentSlice = -1;

                    CalculateTurnaroundTime(timeStep, currentRunningJob, schedulerTypeName);
                    CalculateResponseTime(timeStep, currentRunningJob, schedulerTypeName);
                }
            }

            currentSlice++;
            jobsFinishedReturn = jobsFinished;
            currentRunningJobName = currentRunningJob.name;
        }

        public static void FirstToBack(ref List<Job> jobsPool)
        {
            if (jobsPool.Count() == 0)
            {
                return;
            }

            Job first = jobsPool[0];
            jobsPool.RemoveAt(0);
            jobsPool.Add(first);

        }

        public static void Clean(ref List<Job> jobsThatHaveArrived)
        {
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }
}
