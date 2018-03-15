using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatingSystemsCA1
{
    /// <summary>
    /// Class for running jobs and schedulers
    /// </summary>
    public class JobScheduler
    {
        // Schedulers our program is working with
        FIFO fifo;
        ShortestJobFirst shortestFirst;
        ShortestTime shortestTime;
        RoundRobin rr1;
        RoundRobin rr2;

        // List of all jobs
        List<Job> jobList;



        /// <param name="scheduler">Type of scheduler</param>
        /// <param name="jobList">List of jobs to be scheduled</param>
        public JobScheduler(Scheduler scheduler, List<Job> jobList)
        {
            // initialising instances of schedulers
            fifo = new FIFO();
            shortestFirst = new ShortestJobFirst();
            shortestTime = new ShortestTime();
            rr1 = new RoundRobin(3, 1);         // RoundRobin with timeslice of 3 and id of 1
            rr2 = new RoundRobin(10, 2);        // RoundRobin with timeslice of 10 and id of 2

            this.jobList = jobList;
        }

        public void RunJobSchedule()
        {
            // Bools for if schedulers are finished
            bool allSchedulersFinished = false;
            bool fifoJobsFinished;
            bool shortestFirstJobsFinished;
            bool shortestTimeJobsFinished;
            bool roundRobin1JobsFinished;
            bool roundRobin2JobsFinished;

            int timeStep = 0;                   // timestep that gets incremented each tick
            string output;                      // string output

            List<Job> newJobsAtCurrentTimestep; // new jobs arriving at current timestep      
            bool moreJobsToArrive = true;       // are more jobs going to arrive in future?

            // Initialising the job statistic dictionaries
            foreach (Job j in jobList)
            {
                Job.turnAroundTimes.Add(j.name, new Dictionary<string, int>());
                Job.responseTimes.Add(j.name, new Dictionary<string, int>());
            }

            string header = "T\tFIFO\tSJF\tSTCF\tRR1\tRR2";
            Console.WriteLine(header);
            OutputToFile(header);

            // timestep loop
            while (!allSchedulersFinished)
            {
                newJobsAtCurrentTimestep = new List<Job>();     // reinitialise new jobs list
                output = string.Empty;                          // reinitialise output string

                var jobsArrivingAtCurrentTimestep = jobList.Where(job => job.arrivalTime == timeStep);

                // Find out if there are more jobs that will arrive in the future
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


                foreach (Job arrivingJob in jobsArrivingAtCurrentTimestep)
                {
                    output += timeStep + " ARRIVED: " + arrivingJob.name + "\r\n";
                    newJobsAtCurrentTimestep.Add(arrivingJob);
                }

                // Current job strings for each scheduler
                string fifoJobName;
                string sfJobName;
                string stJobName;
                string rr1JobName;
                string rr2JobName;

                // Run the schedulers
                output += RunSchedulerForTimestep(fifo, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out fifoJobsFinished, out fifoJobName);
                output += RunSchedulerForTimestep(shortestFirst, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out shortestFirstJobsFinished, out sfJobName);
                output += RunSchedulerForTimestep(shortestTime, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out shortestTimeJobsFinished, out stJobName);
                output += RunSchedulerForTimestep(rr1, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out roundRobin1JobsFinished, out rr1JobName);
                output += RunSchedulerForTimestep(rr2, timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out roundRobin2JobsFinished, out rr2JobName);

                output += timeStep + "\t" + fifoJobName + "\t" + sfJobName + "\t" + stJobName + "\t" + rr1JobName + "\t" + rr2JobName;

                if (fifoJobsFinished && shortestTimeJobsFinished && shortestFirstJobsFinished && roundRobin1JobsFinished && roundRobin2JobsFinished)
                {
                    allSchedulersFinished = true;
                }

                // Increment timestep and write output to console
                timeStep++;
                Console.WriteLine(output);
                OutputToFile(output);
            }


            // After schedulers have finished, write statistics
            CalculateAndOutputStatistics();
        }

        public static void OutputToFile(string output)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(Program.outputFile, true))
            {
                file.WriteLine(output);
            }
        }

        private void CalculateAndOutputStatistics()
        {
            // Storing aggregate statistics for turnaround and response times
            float fifoTurnaroundTotal = 0, sjfTurnaroundTotal = 0, stcfTurnaroundTotal = 0, rr1TurnaroundTotal = 0, rr2TurnaroundTotal = 0;
            float fifoResponseTotal = 0, sjfResponseTotal = 0, stcfResponseTotal = 0, rr1ResponseTotal = 0, rr2ResponseTotal = 0;
            int jobCount = jobList.Count();

            // Outputting the turnaround times per job
            string headerTurnaround = "\r\n#\tJOB\tFIFO\tSJF\tSTCF\tRR1\tRR2";
            Console.WriteLine(headerTurnaround);
            OutputToFile(headerTurnaround);

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

                string turnaroundTime = "T\t" + j.name + "\t" + jobFIFOTurnaroundTime + "\t" + jobSJFTurnaroundTime + "\t" + jobSTCFTurnaroundTime + "\t" + jobRR1TurnaroundTime + "\t" + jobRR2TurnaroundTime;
                Console.WriteLine(turnaroundTime);
                OutputToFile(turnaroundTime);
            }
            string statsComplete = "= INDIVIDUAL STATS COMPLETE";
            Console.WriteLine(statsComplete);
            OutputToFile(statsComplete);

            // Outputting the response times per job
            string headerResponse = "\r\n#\tJOB\tFIFO\tSJF\tSTCF\tRR1\tRR2";
            Console.WriteLine(headerResponse);
            OutputToFile(headerResponse);
            foreach (Job j in jobList)
            {
                int jobFIFOResponseTime = Job.responseTimes[j.name]["FIFO"];
                fifoResponseTotal += jobFIFOResponseTime;

                int jobSJFResponseTime = Job.responseTimes[j.name]["SJF"];
                sjfResponseTotal += jobSJFResponseTime;

                int jobSTCFResponseTime = Job.responseTimes[j.name]["STCF"];
                stcfResponseTotal += jobSTCFResponseTime;

                int jobRR1ResponseTime = Job.responseTimes[j.name]["RR1"];
                rr1ResponseTotal += jobRR1ResponseTime;

                int jobRR2ResponseTime = Job.responseTimes[j.name]["RR2"];
                rr2ResponseTotal += jobRR2ResponseTime;

                string responseTime = "R\t" + j.name + "\t" + jobFIFOResponseTime + "\t" + jobSJFResponseTime + "\t" + jobSTCFResponseTime + "\t" + jobRR1ResponseTime + "\t" + jobRR2ResponseTime;
                Console.WriteLine(responseTime);
                OutputToFile(responseTime);
            }
            Console.WriteLine(statsComplete);
            OutputToFile(statsComplete+"\r\n");

            // Outputting the aggregate turnaround times per scheduler

            float fifoAvg = fifoTurnaroundTotal / jobCount, sjfAvg = sjfTurnaroundTotal / jobCount, stcAvg = stcfTurnaroundTotal / jobCount, rr1Avg = rr1TurnaroundTotal / jobCount, rr2Avg = rr2TurnaroundTotal / jobCount;

            string aggregateStats = "\n#\tSCHEDULER\tAVG_TURNAROUND\tAVG_RESPONSE";
            aggregateStats += "\r\n@\tFIFO\t\t" + fifoAvg + "\t\t" + fifoResponseTotal / jobCount;
            aggregateStats += "\r\n@\tSJF\t\t" + sjfAvg + "\t\t" + sjfResponseTotal / jobCount;
            aggregateStats += "\r\n@\tSTCF\t\t" + stcAvg + "\t\t" + stcfResponseTotal / jobCount;
            aggregateStats += "\r\n@\tRR1\t\t" + rr1Avg + "\t\t" + rr1ResponseTotal / jobCount;
            aggregateStats += "\r\n@\tRR2\t\t" + rr2Avg + "\t\t" + rr2ResponseTotal / jobCount;
            aggregateStats += "\r\n= AGGREGATE STATS COMPLETE";

            Console.WriteLine(aggregateStats);
            OutputToFile(aggregateStats);
        }

        private static string RunSchedulerForTimestep(Scheduler s, int timeStep, ref List<Job> newJobsAtCurrentTimestep, bool moreJobsToArrive, out bool jobsFinished, out string currentRunningJobName)
        {
            string completeJob = string.Empty;  // Any completed job name is set in this string
            string output = string.Empty;       // String output thats passed back to the main loop

            // Process timestep for this scheduler
            s.ProcessTimestep(timeStep, ref newJobsAtCurrentTimestep, moreJobsToArrive, out jobsFinished, out currentRunningJobName, out completeJob);

            // If complete job != null, means a job was completed
            if (completeJob != null)
            {
                output += timeStep + " COMPLETE: " + s.schedulerTypeName + "." + completeJob + "\r\n";
            }

            return output;
        }
    }

}
