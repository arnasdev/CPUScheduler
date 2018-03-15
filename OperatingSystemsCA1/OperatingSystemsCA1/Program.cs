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
            scheduler.RunJobSchedule();

            Console.Read();
        }
    }
}
