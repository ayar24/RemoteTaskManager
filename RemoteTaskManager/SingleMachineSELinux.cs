using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class SingleMachineSELinux
    {
        public class SecurityInfo
        {
            public string AppArmorInfo { get; set; }
        }
        DateTime lastUpdated = DateTime.MinValue;
        public List<SecurityInfo> packages = new List<SecurityInfo>();
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("apparmor_status");
                string psOutput = command.Result;
                packages = ParseServiceList(psOutput);
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
                var command = client.RunCommand("sestatus");
                string psOutput = command.Result;
                packages = ParseServiceList(psOutput);
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        private List<SecurityInfo> ParseServiceList(string output)
        {
            List<SecurityInfo> services = new List<SecurityInfo>();
               
                {
                    SecurityInfo service = new SecurityInfo
                    {
                        AppArmorInfo = output,
                    };

                    services.Add(service);
                }
            

            return services;
        }
    }
}
