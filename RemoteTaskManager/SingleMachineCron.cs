using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class SingleMachineCron
    {
        public class CronJobInfo
        {
            public string CmdLine { get; set; } = "";
            public string User { get; set; } = "";
            public string Minute { get; set; } = "";
            public string Hour { get; set; } = "";
            public string DayOfM { get; set; } = "";
            public string DayOfW { get; set; } = "";
            public string Month { get; set; } = "";
        }
        DateTime lastUpdated = DateTime.MinValue;
        public List<CronJobInfo> cronjobs = new List<CronJobInfo>();
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("cat /etc/crontab");// -%cpu");// | head -n 100");
                string psOutput = command.Result;
                cronjobs = ParseCronList(psOutput);
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        private List<CronJobInfo> ParseCronList(string output)
        {
            List<CronJobInfo> services = new List<CronJobInfo>();

            string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines.Skip(1)) // Skip the header line
            {
                if (line[0] == '#')
                    continue;
                if (line.StartsWith("#"))
                    continue;
                if (line.StartsWith("SHELL"))
                    continue;

                string[] partab = line.Split(new char[] { '\t' }, StringSplitOptions.None);// RemoveEmptyEntries);

                string[] parts = partab[0].Split(new char[] { ' ' }, StringSplitOptions.None);// RemoveEmptyEntries);

                string[] parts1 = partab[1].Split(new char[] { ' ' }, StringSplitOptions.None);// RemoveEmptyEntries);

                string[] parts2 = partab[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (partab.Length >= 4)
                {
                    CronJobInfo service = new CronJobInfo
                    {
                        Minute = parts[0],
                        Hour = parts[1],
                        DayOfM = parts1[0],
                        Month = parts1[1],
                        DayOfW = parts1[2],
                        User = parts2[0],
                        CmdLine =  partab[3]//string.Join(" ", parts2.Skip(1))
                    };

                    services.Add(service);
                }
                else if (partab.Length >= 3)
                {
                    CronJobInfo service = new CronJobInfo
                    {
                        Minute = parts[0],
                        Hour = parts[1],
                        DayOfM = parts1[0],
                        Month = parts1[1],
                        DayOfW = parts1[2],
                        User = parts2[0],
                        CmdLine = string.Join(" ", parts2.Skip(1))
                    };

                    services.Add(service);
                }

            }

            return services;
        }
    }
}

