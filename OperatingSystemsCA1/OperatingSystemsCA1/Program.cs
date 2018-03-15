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
        public int firstRunTime = -1;


        // Turnaround time for specific job accessable like turnAroundTimes["job1"]["FIFO"]
        public static Dictionary<string, Dictionary<string, int>> turnAroundTimes = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, Dictionary<string, int>> responseTimes = new Dictionary<string, Dictionary<string, int>>();

        public static Job EmptyJob = new Job("NO JOB", 0, 0); // An empty job for when the system is not processing any jobs, allows for idling in the case of late arriving jobs

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
        RoundRobin rr1;
        RoundRobin rr2;

        List<Job> jobList;


        /// <param name="scheduler">Type of scheduler</param>
        /// <param name="jobList">List of jobs to be scheduled</param>
        public JobScheduler(Scheduler scheduler, List<Job> jobList)
        {
            fifo = new FIFO();
            shortestFirst = new ShortestJobFirst();
            shortestTime = new ShortestTime();
            rr1 = new RoundRobin(3, 1);
            rr2 = new RoundRobin(10, 2);

            this.jobList = jobList;

            RunJobSchedule();
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
            bool roundRobin1JobsFinished;
            bool roundRobin2JobsFinished;

            bool moreJobsToArrive = true;

            Console.WriteLine("T\tFIFO\tSJF\tSTCF\tRR1\tRR2");

            foreach(Job j in jobList)
            {
                Job.turnAroundTimes.Add(j.name, new Dictionary<string, int>());
                Job.responseTimes.Add(j.name, new Dictionary<string, int>());
            }

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
                string rr1JobName;
                string rr2JobName;


                output += RunSchedulerForTimestep(fifo, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out fifoJobsFinished, out fifoJobName);
                output += RunSchedulerForTimestep(shortestFirst, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out shortestFirstJobsFinished, out sfJobName);             
                output += RunSchedulerForTimestep(shortestTime, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out shortestTimeJobsFinished, out stJobName);
                output += RunSchedulerForTimestep(rr1, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out roundRobin1JobsFinished, out rr1JobName);
                output += RunSchedulerForTimestep(rr2, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out roundRobin2JobsFinished, out rr2JobName);

                output += timeStep + "\t"+ fifoJobName + "\t" + sfJobName + "\t"+ stJobName + "\t" + rr1JobName + "\t" + rr2JobName;

                if (fifoJobsFinished && shortestTimeJobsFinished && shortestFirstJobsFinished && roundRobin1JobsFinished && roundRobin2JobsFinished)
                {
                    allSchedulersFinished = true;
                }
                timeStep++;


                Console.WriteLine(output);
            }


            CalculateAndOutputStatistics();
        }

        private void CalculateAndOutputStatistics()
        {
            // Storing aggregate statistics for turnaround and response times
            int fifoTurnaroundTotal = 0, sjfTurnaroundTotal = 0, stcfTurnaroundTotal = 0, rr1TurnaroundTotal = 0, rr2TurnaroundTotal = 0;
            int fifoResponseTotal = 0, sjfResponseTotal = 0, stcfResponseTotal = 0, rr1ResponseTotal = 0, rr2ResponseTotal = 0;
            int jobCount = jobList.Count();

            // Outputting the turnaround times per job
            Console.WriteLine("\n#\tJOB\tFIFO\tSJF\tSTCF\tRR1\tRR2");
            foreach (Job j in jobList)
            {
                int jobFIFOTurnaroundTime = Job.turnAroundTimes[j.name]["FIFO"];
                fifoTurnaroundTotal += jobFIFOTurnaroundTime;

                int jobSJFTurnaroundTime = Job.turnAroundTimes[j.name]["SJF"];
                sjfTurnaroundTotal += jobSJFTurnaroundTime;

                int jobSTCFTurnaroundTime = Job.turnAroundTimes[j.name]["STCF"];
                stcfTurnaroundTotal += jobSTCFTurnaroundTime;

                int jobRR1TurnaroundTime = Job.turnAroundTimes[j.name]["RR1"];
                rr1TurnaroundTotal += jobRR1TurnaroundTime;

                int jobRR2TurnaroundTime = Job.turnAroundTimes[j.name]["RR2"];
                rr2TurnaroundTotal += jobRR2TurnaroundTime;

                Console.WriteLine("T\t" + j.name + "\t" + jobFIFOTurnaroundTime + "\t" + jobSJFTurnaroundTime + "\t" + jobSTCFTurnaroundTime + "\t" + jobRR1TurnaroundTime + "\t" + jobRR2TurnaroundTime);
            }
            Console.WriteLine("= INDIVIDUAL STATS COMPLETE");

            // Outputting the response times per job
            Console.WriteLine("\n#\tJOB\tFIFO\tSJF\tSTCF\tRR1\tRR2");
            foreach (Job j in jobList)
            {
                int jobFIFOResponseTime = Job.turnAroundTimes[j.name]["FIFO"];
                fifoResponseTotal += jobFIFOResponseTime;

                int jobSJFResponseTime = Job.turnAroundTimes[j.name]["SJF"];
                sjfResponseTotal += jobSJFResponseTime;

                int jobSTCFResponseTime = Job.turnAroundTimes[j.name]["STCF"];
                stcfResponseTotal += jobSTCFResponseTime;

                int jobRR1ResponseTime = Job.turnAroundTimes[j.name]["RR1"];
                rr1ResponseTotal += jobRR1ResponseTime;

                int jobRR2ResponseTime = Job.turnAroundTimes[j.name]["RR2"];
                rr2ResponseTotal += jobRR2ResponseTime;

                Console.WriteLine("R\t" + j.name + "\t" + jobFIFOResponseTime + "\t" + jobSJFResponseTime + "\t" + jobSTCFResponseTime + "\t" + jobRR1ResponseTime + "\t" + jobRR2ResponseTime);
            }
            Console.WriteLine("= INDIVIDUAL STATS COMPLETE");

            // Outputting the aggregate turnaround times per scheduler
            Console.WriteLine("\n#\tSCHEDULER\tAVG_TURNAROUND\tAVG_RESPONSE");
            Console.WriteLine("@\tFIFO\t\t" + fifoTurnaroundTotal / jobCount + "\t\t" + fifoResponseTotal / jobCount);
            Console.WriteLine("@\tSJF\t\t" + sjfTurnaroundTotal / jobCount + "\t\t" + sjfResponseTotal / jobCount);
            Console.WriteLine("@\tSTCF\t\t" + stcfTurnaroundTotal / jobCount + "\t\t" + stcfResponseTotal / jobCount);
            Console.WriteLine("@\tRR1\t\t" + rr1TurnaroundTotal / jobCount + "\t\t" + rr1ResponseTotal / jobCount);
            Console.WriteLine("@\tRR2\t\t" + rr2TurnaroundTotal / jobCount + "\t\t" + rr2ResponseTotal / jobCount);
            Console.WriteLine("= AGGREGATE STATS COMPLETE");
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

        public static void Clean(ref List<Job> jobsThatHaveArrived)
        {
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }

    /// <summary>
    /// Orders jobs by shortest time to completion at the first timestep, after which no sorting is done for new arrivals which makes it ineffective
    /// </summary>
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

            if(currentRunningJob.firstRunTime == -1)
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
                if          (j1.timeLeft < j2.timeLeft) return -1;
                else if     (j1.timeLeft > j2.timeLeft) return 1;
                else                                    return 0;
            });

            // And clean up completed jobs, maybe do something with them later
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }

    /// <summary>
    /// 
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
            this.schedulerTypeName = "RR"+rrID;
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
    #endregion

}
