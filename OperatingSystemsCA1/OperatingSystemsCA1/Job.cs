using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OperatingSystemsCA1
{

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
                using (var reader = new StreamReader(@"C:\Users\d00167238\Desktop\Jobs.csv"))
                {
                    parsedJobList = new List<Job>();
                    Job parsedJob = null;
                    while (!reader.EndOfStream)
                    {
                        // split values on space
                        var values = reader.ReadLine().Split(' ');  

                        // parse jobs in this format "name runtime arrivaltime"
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
}
