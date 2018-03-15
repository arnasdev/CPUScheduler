using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatingSystemsCA1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Job> jobList = Job.ReadJobs();
            JobScheduler scheduler = new JobScheduler(new ShortestTime(), jobList);
            Console.Read();
        }
    }


    /// <summary>
    /// Class which represents each process to be run by the system
    /// </summary>
    public class Job
    {
        public string name;
        public int runTime;
        public int arrivalTime;
        public int timeLeft;
        public bool isFinished;

        public static Job EmptyJob = new Job("NO JOB", 0, 0);

        Job(string name, int runTime, int arrivalTime)
        {
            this.name = name;
            this.runTime = runTime;
            this.arrivalTime = arrivalTime;

            timeLeft = runTime;
            isFinished = false;
        }

        public Job ShallowCopy()
        {
            return (Job)this.MemberwiseClone();
        }

        /// <summary>
        /// Reads a .csv file of jobs
        /// </summary>
        /// <returns>A list of jobs parsed from a .csv file</returns>
        public static List<Job> ReadJobs()
        {
            List<Job> parsedJobList = null;
            try
            {
                using (var reader = new StreamReader(@"C:\Jobs.csv"))
                {
                    parsedJobList = new List<Job>();
                    Job parsedJob = null;
                    while (!reader.EndOfStream)
                    {
                        var values = reader.ReadLine().Split(' ');
                        parsedJob = new Job(values[0],
                                            int.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat),
                                            int.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat));

                        parsedJobList.Add(parsedJob);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return parsedJobList;
        }
    }

    /// <summary>
    /// Base class for any sorting algorithms, including those which decide on arrival time and those which decide each time-step
    /// </summary>
    public class JobScheduler
    {

        // Schedulers - eventually all schedulers will be here
        
        FIFO fifo;
        ShortestJobFirst shortestFirst;
        ShortestTime shortestTime;
        //RoundRobin rr1;
        //RoundRobin rr2;
        List<Job> jobList;


        /// <param name="scheduler">Type of scheduler</param>
        /// <param name="jobList">List of jobs to be scheduled</param>
        public JobScheduler(Scheduler scheduler, List<Job> jobList)
        {
            shortestTime = new ShortestTime();
            shortestFirst = new ShortestJobFirst();
            fifo = new FIFO();

            //this.scheduler = scheduler;
            this.jobList = jobList;

            //InitialSort();
            RunJobSchedule();
        }

        private void InitialSort()
        {
            //arrivalTimeScheduler.SortArrivalTimes(ref jobList);
        }

        private void RunJobSchedule()
        {
            bool allSchedulersFinished = false;
            int timeStep = 0;
            string output;
            List<Job> newJobsAtCurrentTimestep;

            bool fifoJobsFinished;
            bool shortestFirstJobsFinished;
            bool shortestTimeJobsFinished;

            bool moreJobsToArrive = true;

            Console.WriteLine("T\tFIFO\tSJF\tSTCF\tRR1\tRR2");

            while (!allSchedulersFinished)
            {
                newJobsAtCurrentTimestep = new List<Job>();

                output = string.Empty;

                var jobsArrivingAtCurrentTimestep = jobList.Where(job => job.arrivalTime == timeStep);

                if (moreJobsToArrive)
                {
                    var jobsArrivingAtFutureTimesteps = jobList.Where(job => job.arrivalTime > timeStep);
                    if (jobsArrivingAtFutureTimesteps.Count() > 0)
                    {
                        moreJobsToArrive = true;
                    }
                    else
                    {
                        moreJobsToArrive = false;
                    }
                }
              

                foreach(Job arrivingJob in jobsArrivingAtCurrentTimestep)
                {
                    output += timeStep + " ARRIVED: " + arrivingJob.name + "\n";
                    newJobsAtCurrentTimestep.Add(arrivingJob);
                }


                string fifoJobName;
                string sfJobName;
                string stJobName;
                


                output += RunSchedulerForTimestep(fifo, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out fifoJobsFinished, out fifoJobName);

                output += RunSchedulerForTimestep(shortestFirst, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out shortestFirstJobsFinished, out sfJobName);
                
                output += RunSchedulerForTimestep(shortestTime, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out shortestTimeJobsFinished, out stJobName);


                output += timeStep + "\t"+ fifoJobName + "\t" + sfJobName + "\t"+ stJobName + "\tN/A\tN/A";

                if (fifoJobsFinished && shortestTimeJobsFinished && shortestFirstJobsFinished)
                {
                    allSchedulersFinished = true;
                }
                timeStep++;


                Console.WriteLine(output);
            }
            
        }

        private static string RunSchedulerForTimestep(Scheduler s, int timeStep, ref List<Job> newJobsAtCurrentTimestep, bool moreJobsToArrive, out bool jobsFinished, out string currentRunningJobName)
        {
            string completeJob = string.Empty;
            string output = string.Empty;

            s.ProcessTimestep(timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out jobsFinished, out currentRunningJobName, out completeJob);

            if (completeJob != null)
            {
                output += timeStep + " COMPLETE: "+s.schedulerTypeName+ "." + completeJob + "\n";
            }

            return output;
        }
    }



    #region Schedulers
    /// <summary>
    /// Base class for any sorting algorithms which decide on arrival time
    /// </summary>
    public class Scheduler
    {
        protected Job emptyJob = Job.EmptyJob;      // An empty job for when the system is not processing any jobs, allows for idling in the case of late arriving jobs
        protected bool jobsFinished = false;        // Are all jobs for this scheduler finished
        protected Job currentRunningJob = null;     // The current running job
        protected List<Job> sortedJobsPool = null;  // A list of currently queued jobs, sorted to the specific algorithm required for each scheduler
        public string schedulerTypeName = string.Empty;

        // Cannot create an instance of this base class, subclasses used instead
        protected Scheduler()
        {
            sortedJobsPool = new List<Job>();
        }



        public static void AddClonedJobsToList(ref List<Job> jobList, List<Job> jobsToAdd)
        {
            foreach(Job j in jobsToAdd)
            {
                jobList.Add(j.ShallowCopy());
            }
        }

        public virtual void ProcessTimestep(int timeStep, ref List<Job> newJobsAtCurrentTimestep, bool moreJobsToArrive, out bool jobsFinished, out string currentRunningJobName, out string completeJob)
        {
            jobsFinished = false;
            currentRunningJobName = null;
            completeJob = null;
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
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
                            currentRunningJob = emptyJob;
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
                }
            }

           

            jobsFinishedReturn = jobsFinished;
            currentRunningJobName = currentRunningJob.name;
        }

        public static void Clean(ref List<Job> jobsThatHaveArrived)
        {
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
    public class ShortestJobFirst : Scheduler
    {
        public ShortestJobFirst()
        {
            schedulerTypeName = "SJF";
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
                SortByShortestTime(ref sortedJobsPool);
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
                            currentRunningJob = emptyJob;
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
                }
            }



            jobsFinishedReturn = jobsFinished;
            currentRunningJobName = currentRunningJob.name;
        }

        public static void SortByShortestTime(ref List<Job> jobsThatHaveArrived)
        {
            // Sort our jobs by shortest time left
            jobsThatHaveArrived.Sort(delegate (Job j1, Job j2) {
                if      (j1.timeLeft < j2.timeLeft) return -1;
                else if (j1.timeLeft > j2.timeLeft) return 1;
                else                                return 0;
            });
        }

        public static void Clean(ref List<Job> jobsThatHaveArrived)
        {
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //public class RoundRobin : Scheduler
    //{
    //    private int timeSlice1;
    //    private int timeSlice2;

    //    public override void SortTimes(int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList, out bool jobsFinishedReturn, out string currentRunningJobName)
    //    {
    //        jobsFinishedReturn = false;
    //        currentRunningJobName = null;
    //    }
    //}

    /// <summary>
    /// Orders jobs by the shortest time to completion
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
                    if(sortedJobsPool.Count > 0)
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
                    if(sortedJobsPool.Count() > 0)
                    {
                        if (sortedJobsPool[0].timeLeft < currentRunningJob.timeLeft || currentRunningJob == Job.EmptyJob)
                        {
                            currentRunningJob = sortedJobsPool[0]; // current job switches to shortest
                        }
                    }
                    else
                    {
                        currentRunningJob = emptyJob;
                    }
                }
                else
                {
                    currentRunningJob = emptyJob;
                }

            }

            if(currentRunningJob != Job.EmptyJob)
            {
                if (currentRunningJob.timeLeft > 0)
                {
                    currentRunningJob.timeLeft--;
                }

                if (currentRunningJob.timeLeft <= 0)
                {
                    completeJob = currentRunningJob.name;
                    currentRunningJob.isFinished = true;
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
                if          (j1.timeLeft < j2.timeLeft) return -1;
                else if     (j1.timeLeft > j2.timeLeft) return 1;
                else                                    return 0;
            });

            // And clean up completed jobs, maybe do something with them later
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }

        #region Old Unoptimised but working Code
        //public override void SortTimes(int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList)
        //{
        //int TIMESTEP = 0;
        //bool jobsFinished = false;
        //List<Job> jobsThatHaveArrived = new List<Job>();
        //Job currentRunningJob = null;


        //while (!jobsFinished)
        //{
        //    Console.WriteLine("");
        //    Console.WriteLine("TIMESTEP: " + TIMESTEP);
        //    var jobsArrivingAtCurrentTimestep = jobList.Where(job => job.arrivalTime == TIMESTEP);
        //    foreach(Job j in jobsArrivingAtCurrentTimestep)
        //    {
        //        Console.WriteLine("Job " + j.name + "arrived!");
        //    }


        //    jobsThatHaveArrived.AddRange(jobsArrivingAtCurrentTimestep);
        //    SortArrivedJobsByShortestTime(ref jobsThatHaveArrived);

        //    if (TIMESTEP == 0)
        //    {
        //        currentRunningJob = jobsThatHaveArrived[0];
        //    }
        //    else
        //    {
        //        if (currentRunningJob.isFinished)
        //        {
        //            if (jobsThatHaveArrived.Count > 0)
        //            {
        //                currentRunningJob = jobsThatHaveArrived[0];
        //            }
        //            else
        //            {
        //                jobsFinished = true;
        //                Console.WriteLine("all jobs finished");
        //                continue;
        //            }
        //        }

        //        if (jobsThatHaveArrived[0].timeLeft < currentRunningJob.timeLeft)
        //        {
        //            currentRunningJob = jobsThatHaveArrived[0]; // current job switches to shortest
        //        }       
        //    }

        //    if (currentRunningJob.timeLeft > 0)
        //    {
        //        currentRunningJob.timeLeft--;
        //        Console.WriteLine(currentRunningJob.name + " timeleft: " + currentRunningJob.timeLeft);
        //    }

        //    if (currentRunningJob.timeLeft <= 0)
        //    {
        //        currentRunningJob.isFinished = true;
        //        Console.WriteLine(currentRunningJob.name + " finished");
        //    }

        //    TIMESTEP++;

        //    // todo, need to account for when currentjob is finished and no jobs arrive at this timeslot, but jobs will arrive in future
        //}
        // }
        #endregion
    }
    #endregion

}
