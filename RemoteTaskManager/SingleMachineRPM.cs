using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class SingleMachineRPM
    {
        public class PackageInfo
        {
            public string Name { get; set; }
            public string Enabled { get; set; }
            public string State { get; set; }
            public string RunState { get; set; }
            public string Description { get; set; }
        }
        DateTime lastUpdated = DateTime.MinValue;
        public List<PackageInfo> packages = new List<PackageInfo>();
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("apt list");// -%cpu");// | head -n 100");
                string psOutput = command.Result;
                packages = ParseServiceList(psOutput,' ');
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        public bool RefreshCentos(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("rpm -qa");// -%cpu");// | head -n 100");
                string psOutput = command.Result;
                packages = ParseServiceList(psOutput, '\n');
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        private List<PackageInfo> ParseServiceList(string output, char delim)
        {
            List<PackageInfo> services = new List<PackageInfo>();

            string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines.Skip(1)) // Skip the header line
            {
                string[] parts = line.Split(new char[] { delim }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                {
                    PackageInfo service = new PackageInfo
                    {
                        Name = parts[0],
                        Enabled = "",
                        State = "",
                        RunState = "",
                        Description = ""
                    };

                    services.Add(service);
                } 
                else if (parts.Length >= 5)
                {
                    string finalName = parts[0];
                    string[] packageName = parts[0].Split('/', StringSplitOptions.None);
                    if (packageName.Count() > 1)
                    {
                        finalName = packageName[0];
                    }
                    PackageInfo service = new PackageInfo
                    {
                        

                        Name = finalName,
                        Enabled = parts[1],
                        State = parts[2],
                        RunState = parts[3],
                        Description = string.Join(" ", parts.Skip(4))
                    };

                    services.Add(service);
                }
               /* else if (parts.Length >= 3)
                {
                    string finalName = parts[0];
                    string[] packageName = parts[0].Split('/', StringSplitOptions.None);
                    if (packageName.Count() > 1)
                    {
                        finalName = packageName[0];
                    }
                    PackageInfo service = new PackageInfo
                    {
                        Name = finalName,
                        Enabled = parts[1],
                        State = "",
                        RunState = "",
                        Description = ""
                    };
               
                    services.Add(service);
                }*/
            }

            return services;
        }
    }
}
