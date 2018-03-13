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
        string name;
        int runTime;
        int arrivalTime;

        Job(string name, int runTime, int arrivalTime)
        {
            this.name = name;
            this.runTime = runTime;
            this.arrivalTime = arrivalTime;
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
        }
    }

    /// <summary>
    /// Base class for any sorting algorithms which decide on arrival time
    /// </summary>
    public class ArrivalTimeScheduler
    {
        // Cannot create an instance of this base class, subclasses used instead
        protected ArrivalTimeScheduler()
        {
        }

        public virtual void SortArrivalTimes()
        {
        }
    }

    /// <summary>
    /// Base class for any sorting algorithms which decide each time-step
    /// </summary>
    public class TimestepScheduler
    {
        // Cannot create an instance of this base class, subclasses used instead
        protected TimestepScheduler()
        {
        }

        public virtual void SortTimeStep()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FIFO : ArrivalTimeScheduler
    {
        public override void SortArrivalTimes()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ShortestFirst : ArrivalTimeScheduler
    {
        public override void SortArrivalTimes()
        {
            List<Job> jobList = new List<Job>();

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RoundRobin : TimestepScheduler
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class ShortestTime : TimestepScheduler
    {
    }
}
