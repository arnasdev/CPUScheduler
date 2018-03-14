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
            JobScheduler scheduler = new JobScheduler(new FIFO(), new RoundRobin(), jobList);
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
        // Inputs
        ArrivalTimeScheduler arrivalTimeScheduler;
        TimestepScheduler timestepScheduler;
        List<Job> jobList;

        // Output
        Dictionary<int, Job> jobSchedule;

        /// <param name="arrivalTimeScheduler">Arrival Scheduler: FIFO or ShortestFirst</param>
        /// <param name="timestepScheduler">Timestep Scheduler: RoundRobin or ShortestTime</param>
        /// <param name="jobList">List of jobs to be scheduled</param>
        public JobScheduler(ArrivalTimeScheduler arrivalTimeScheduler, TimestepScheduler timestepScheduler, List<Job> jobList)
        {
            this.arrivalTimeScheduler = arrivalTimeScheduler;
            this.timestepScheduler = timestepScheduler;
            this.jobList = jobList;

            InitialSort();
            CreateJobSchedule();
        }

        private void InitialSort()
        {
            arrivalTimeScheduler.SortArrivalTimes(ref jobList);
        }

        private void CreateJobSchedule()
        {
            timestepScheduler.SortTimeSteps(ref jobSchedule, ref jobList);
        }
    }

    #region Arrival Time Schedulers
    /// <summary>
    /// Base class for any sorting algorithms which decide on arrival time
    /// </summary>
    public class ArrivalTimeScheduler
    {
        // Cannot create an instance of this base class, subclasses used instead
        protected ArrivalTimeScheduler()
        {
        }

        public virtual void SortArrivalTimes(ref List<Job> jobList)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FIFO : ArrivalTimeScheduler
    {
        public override void SortArrivalTimes(ref List<Job> jobList)
        {
            jobList.Sort(delegate (Job j1, Job j2) {
                if (j1.arrivalTime < j2.arrivalTime) return 1;
                else if (j1.arrivalTime > j2.arrivalTime) return -1;
                else return 0;
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ShortestFirst : ArrivalTimeScheduler
    {
        public override void SortArrivalTimes(ref List<Job> jobList)
        {
            jobList.Sort(delegate (Job j1, Job j2) {
                if (j1.runTime < j2.runTime) return 1;
                else if (j1.runTime > j2.runTime) return -1;
                else return 0;
            });
        }
    }
    #endregion

    #region Time Step Schedulers
    /// <summary>
    /// Base class for any sorting algorithms which decide each time-step
    /// </summary>
    public class TimestepScheduler
    {
        // Cannot create an instance of this base class, subclasses used instead
        protected TimestepScheduler()
        {
        }

        public virtual void SortTimeSteps(ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RoundRobin : TimestepScheduler
    {
        private int timeSlice1;
        private int timeSlice2;

        public override void SortTimeSteps(ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList)
        {
           
        }
    }

    /// <summary>
    /// Orders jobs by the shortest time to completion
    /// </summary>
    public class ShortestTime : TimestepScheduler
    {
        public override void SortTimeSteps(ref Dictionary<int, Job> jobSchedule, ref List<Job> jobList)
        {

            // pseudocode begin
            var currentTime = -1;
            List<Job> jobsThatHaveArrived = new List<Job>();

            // some kind of loop here that runs through timesteps


            // --------- first timestep ---------
            var jobsAtZero = jobList.Where(job => job.arrivalTime == 0).ToList();

            currentTime = 0;
            jobsThatHaveArrived.AddRange(jobsAtZero);
            SortArrivedJobsByShortestTime(ref jobsThatHaveArrived);

            var runningJob = jobSchedule[currentTime];  // assign first running job

            
            var jobsAtOne = jobList.Where(job => job.arrivalTime == 1).ToList();

            // if job arriving at next timestep with shortest running time has less running time than current job
            // set the current job to be this new job and run through it 
            if(jobsThatHaveArrived[0].timeLeft < runningJob.timeLeft)
            {
                runningJob = jobsThatHaveArrived[0]; // current job switches to shortest
            }

            // Process this timestep and decrement time remaining for job
            if (runningJob.timeLeft > 0)
            {
                // decrement timeleft
                runningJob.timeLeft--;
            }

            if(runningJob.timeLeft <= 0)
            {
                // job is finished
                runningJob.isFinished = true;
            }

            if (jobsThatHaveArrived.Count == 0)
            {
                // Finished all jobs
            }

            // --------- next timestep ---------
            currentTime = 1;
            jobsThatHaveArrived.AddRange(jobsAtOne);
            SortArrivedJobsByShortestTime(ref jobsThatHaveArrived);

            if (jobsThatHaveArrived[0].timeLeft < runningJob.timeLeft)
            {
                runningJob = jobsThatHaveArrived[0]; // current job switches to shortest
            }

            // Process this timestep and decrement time remaining for job
            if (runningJob.timeLeft > 0)
            {
                // decrement timeleft
                runningJob.timeLeft--;
            }

            if (runningJob.timeLeft <= 0)
            {
                // job is finished
                runningJob.isFinished = true;
            }

            if(jobsThatHaveArrived.Count == 0)
            {
                // Finished all jobs
            }

            // --------- next timestep ---------
            jobSchedule[currentTime] = null;

            foreach (Job j in jobList)
            {
                
            }
            // pseudocode end
        }

        public static void SortArrivedJobsByShortestTime(ref List<Job> jobsThatHaveArrived)
        {
            // Sort our jobs by shortest time left
            jobsThatHaveArrived.Sort(delegate (Job j1, Job j2) {
                if      (j1.timeLeft < j2.timeLeft)     return 1;
                else if (j1.timeLeft > j2.timeLeft)     return -1;
                else                                    return 0;
            });

            // And clean up completed jobs, maybe do something with them later
            jobsThatHaveArrived.RemoveAll(job => job.isFinished == true);
        }
    }
    #endregion

}
