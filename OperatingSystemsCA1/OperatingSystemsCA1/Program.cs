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
        //ShortestFirst sf;
        //FIFO fifo;
        ShortestTime shortestTime;
        //RoundRobin rr1;
        //RoundRobin rr2;


        // Inputs
        Scheduler scheduler;
        List<Job> jobList;

        // Output
        Dictionary<int, Job> jobSchedule;

        /// <param name="scheduler">Type of scheduler</param>
        /// <param name="jobList">List of jobs to be scheduled</param>
        public JobScheduler(Scheduler scheduler, List<Job> jobList)
        {
            shortestTime = new ShortestTime();

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
            List<Job> workingJobsPool = new List<Job>();

            bool shortestTimeJobsFinished;
            bool moreJobsToArrive = true;

            Console.WriteLine("T\tFIFO\tSJF\tSTCF\tRR1\tRR2");

            while (!allSchedulersFinished)
            {
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
                    workingJobsPool.Add(arrivingJob);
                }


                string stJobName;
                

                output += RunSchedulerForTimestep(shortestTime, timeStep, ref jobSchedule, ref workingJobsPool, moreJobsToArrive, out shortestTimeJobsFinished, out stJobName);


                output += timeStep + "\tN/A\tN/A\t"+ stJobName + "\tN/A\tN/A";

                if (shortestTimeJobsFinished)
                {
                    allSchedulersFinished = true;
                }
                timeStep++;


                Console.WriteLine(output);
            }
            
        }

        private static string RunSchedulerForTimestep(Scheduler s, int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> workingJobsPool, bool moreJobsToArrive, out bool jobsFinished, out string currentRunningJobName)
        {
            string completeJob = string.Empty;
            string output = string.Empty;

            s.SortTimes(timeStep, ref jobSchedule, ref workingJobsPool, moreJobsToArrive, out jobsFinished, out currentRunningJobName, out completeJob);

            if (completeJob != null)
            {
                output += timeStep + " COMPLETE: " + completeJob + "\n";
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
        // Cannot create an instance of this base class, subclasses used instead
        protected Scheduler()
        {
        }

        public virtual void SortTimes(int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> workingJobsPool, bool moreJobsToArrive, out bool jobsFinished, out string currentRunningJobName, out string completeJob)
        {
            jobsFinished = false;
            currentRunningJobName = null;
            completeJob = null;
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //public class FIFO : Scheduler
    //{
    //    public override void SortTimes(int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList, out bool jobsFinishedReturn, out string currentRunningJobName)
    //    {
    //        jobsFinishedReturn = false;
    //        currentRunningJobName = null;
    //    }

    //    //public override void SortArrivalTimes(ref List<Job> jobList)
    //    //{
    //    //    jobList.Sort(delegate (Job j1, Job j2) {
    //    //        if (j1.arrivalTime < j2.arrivalTime) return 1;
    //    //        else if (j1.arrivalTime > j2.arrivalTime) return -1;
    //    //        else return 0;
    //    //    });
    //    //}
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //public class ShortestFirst : Scheduler
    //{
    //    public override void SortTimes(int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList, out bool jobsFinishedReturn, out string currentRunningJobName)
    //    {
    //        jobsFinishedReturn = false;
    //        currentRunningJobName = null;
    //    }
    //    //public override void SortArrivalTimes(ref List<Job> jobList)
    //    //{
    //    //    jobList.Sort(delegate (Job j1, Job j2) {
    //    //        if (j1.runTime < j2.runTime) return 1;
    //    //        else if (j1.runTime > j2.runTime) return -1;
    //    //        else return 0;
    //    //    });
    //    //}
    //}

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
        Job emptyJob = Job.EmptyJob;

        bool jobsFinished = false;
        Job currentRunningJob = null;
        
        List<Job> shortestTimeSortedJobsPool = null;

        public override void SortTimes(int timeStep, ref Dictionary<int, Job> jobSchedule, ref List<Job> workingJobsPool, bool moreJobsToArrive, out bool jobsFinishedReturn, out string currentRunningJobName, out string completeJob)
        {
            completeJob = null;
            shortestTimeSortedJobsPool = workingJobsPool;
            SortArrivedJobsByShortestTime(ref shortestTimeSortedJobsPool);

            if (timeStep == 0)
            {
                currentRunningJob = shortestTimeSortedJobsPool[0];
            }
            else
            {
                if (currentRunningJob.isFinished)
                {
                    if(shortestTimeSortedJobsPool.Count > 0)
                    {
                        currentRunningJob = shortestTimeSortedJobsPool[0];
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
                    if(shortestTimeSortedJobsPool.Count() > 0)
                    {
                        if (shortestTimeSortedJobsPool[0].timeLeft < currentRunningJob.timeLeft || currentRunningJob == Job.EmptyJob)
                        {
                            currentRunningJob = shortestTimeSortedJobsPool[0]; // current job switches to shortest
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
         

            jobsFinishedReturn = jobsFinished;
            currentRunningJobName = currentRunningJob.name;
        }

        public static void SortArrivedJobsByShortestTime(ref List<Job> jobsThatHaveArrived)
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
