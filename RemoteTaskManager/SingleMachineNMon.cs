using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class SingleMachineNMon
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
                var command = client.RunCommand("nmon -f -s 1 -c 1");// -%cpu");// | head -n 100");
                Thread.Sleep(1300);
                var command2 = client.RunCommand("cat *.nmon");// -%cpu");// | head -n 100");
                var command3 = client.RunCommand("rm -rf *.nmon");// -%cpu");// | head -n 100");
                string psOutput = command2.Result;
                packages = ParseServiceList(psOutput);
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        static double ExtractValue(string line)
        {
            // Use regex to extract numeric value
            Regex regex = new Regex(@"[0-9]+\.?[0-9]*");
            Match match = regex.Match(line);
            if (match.Success)
            {
                return double.Parse(match.Value);
            }
            return 0.0;
        }
        private List<PackageInfo> ParseServiceList(string output)
        {
            List<PackageInfo> services = new List<PackageInfo>();
            PackageInfo service = new PackageInfo
            {
                Name = ""//userUsage.ToString(),
            };
            string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines.Skip(1)) // Skip the header line
            {
                // Check if the line contains CPU usage information
                if (line.Contains("CPU_ALL") || line.StartsWith("CPU_ALL_CPU"))
                {
                    // Split the line by delimiter (usually comma)
                    string[] parts = line.Split(',');
                    if (parts[1].StartsWith("T"))
                    {
                        // Extract the CPU usage values
                        double userUsage = double.Parse(parts[2]);
                        double sysUsage = double.Parse(parts[3]);
                        double idleUsage = double.Parse(parts[4]);
                        service.Name = userUsage.ToString();
                        
                       
                    }
                  }
                // Check if the line contains CPU usage information
                if (line.StartsWith("DISKREAD"))
                {
                    if (line.StartsWith("DISKREAD,T"))
                    {
                        // Split the line by delimiter (usually comma)
                        string[] parts = line.Split(',');
                        //                    if (parts[1].StartsWith("T"))
                        {
                            // Extract the CPU usage values
                            if (parts.Count() > 10)
                                service.Avail = ExtractValue(parts[10]).ToString();


                        }
                    }
                }
                if (line.StartsWith("DISKWRITE"))
                {
                    // Split the line by delimiter (usually comma)
                    // Split the line by delimiter (usually comma)
                    if (line.StartsWith("DISKWRITE,T"))
                    {
                        string[] parts = line.Split(',');
                        //                    if (parts[1].StartsWith("T"))
                        {
                            // Extract the CPU usage values
                            if (parts.Count ()  > 10) // Should look for the index of sda
                               service.Used = ExtractValue(parts[10]).ToString();

                        }
                    }
                }
                if (line.StartsWith("MEM"))
                {
                    if (line.StartsWith("MEM,T"))
                    {
                        
                        string[] parts = line.Split(',');
                        if (parts.Count() >= 6)
                       
                        {
                            // Extract the CPU usage values
                            service.UsePercent = ExtractValue(parts[6]).ToString();

                        }
                    }
                }
                //   string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //     string[] tokens = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //   double cpuUsage = double.Parse(tokens[1]);


            }
            services.Add(service);
            return services;
        }

        
    }
}
