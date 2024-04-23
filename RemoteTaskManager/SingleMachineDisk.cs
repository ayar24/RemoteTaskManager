using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class SingleMachineDisk
    {
        public class PackageInfo
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string Used { get; set; }
            public string Avail { get; set; }
            public string UsePercent { get; set; }
            public string MountedOn { get; set; }
        }
        DateTime lastUpdated = DateTime.MinValue;
        public List<PackageInfo> packages = new List<PackageInfo>();
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("df -h");// -%cpu");// | head -n 100");
                string psOutput = command.Result;
                packages = ParseServiceList(psOutput);
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        private List<PackageInfo> ParseServiceList(string output)
        {
            List<PackageInfo> services = new List<PackageInfo>();

            string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines.Skip(1)) // Skip the header line
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 5)
                {
                    PackageInfo service = new PackageInfo
                    {
                        Name = parts[0],
                        Size = parts[1],
                        Used = parts[2],
                        Avail = parts[3],
                        UsePercent = parts[4],
                        MountedOn = string.Join(" ", parts.Skip(5))
                    };

                    services.Add(service);
                }
                else if (parts.Length >= 3)
                {
                    PackageInfo service = new PackageInfo
                    {
                        Name = parts[0],
                        Size = parts[1],
                        Used = "",
                        Avail = "",
                        UsePercent = "",
                        MountedOn = ""
                    };

                    services.Add(service);
                }
            }

            return services;
        }
    }
}
