using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RemoteTaskManager.SingleMachineProcessList;

namespace RemoteTaskManager
{
    public class SingleMachineServicesList
    {
        public class ServiceInfo
        {
            public string Name { get; set; }
            public string Enabled { get; set; }
            public string State { get; set; }
            public string RunState { get; set; }
            public string Description { get; set; }
        }
        DateTime lastUpdated = DateTime.MinValue;
        public List<ServiceInfo> services = new List<ServiceInfo>();
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("systemctl list-units --type=service");// -%cpu");// | head -n 100");
                string psOutput = command.Result;
                services = ParseServiceList(psOutput);
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        private List<ServiceInfo> ParseServiceList(string output)
        {
            List<ServiceInfo> services = new List<ServiceInfo>();

            string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines.Skip(1)) // Skip the header line
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 5)
                {
                    ServiceInfo service = new ServiceInfo
                    {
                        Name = parts[0],
                        Enabled = parts[1],
                        State = parts[2],
                        RunState = parts[3],
                        Description = string.Join(" ", parts.Skip(4))
                    };

                    services.Add(service);
                }
            }

            return services;
        }
    }
}
