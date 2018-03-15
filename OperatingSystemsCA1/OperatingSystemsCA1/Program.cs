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
        public static string outputFile;

        static void Main(string[] args)
        {
            string whichJobFile = string.Empty;
            if (args.Count() <= 0)
            {
                whichJobFile = "Jobs1";
            }
            else
            {
                whichJobFile = args[0];
            }

            outputFile = System.AppDomain.CurrentDomain.BaseDirectory;

            if (whichJobFile == "Jobs1")
            {
                outputFile += @"demo_output1.txt";
            }

            if (whichJobFile == "Jobs2")
            {
                outputFile += @"demo_output2.txt";
            }

            if (whichJobFile == "Jobs3")
            {
                outputFile += @"demo_output3.txt";
            }

            List<Job> jobList = Job.ReadJobs(whichJobFile);

            JobScheduler scheduler = new JobScheduler(new ShortestTime(), jobList);
            scheduler.RunJobSchedule();

            Console.Read();
        }
    }
}
