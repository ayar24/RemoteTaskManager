using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
namespace RemoteTaskManager
{
    public class SingleMachineUsersList
    {

        public class UserInfo
        {
            public string UserName { get; set; } = "";    
            public string FullName { get; set; } = "";
            public string UserId { get; set; } = "";
            public string GroupId { get; set; } = "";
            public string HomeDirectory { get; set; } = "";
            public string Shell { get; set; } = "";
        }

        static List<UserInfo> ParsePasswdOutput(string output)
        {
            List<UserInfo> users = new List<UserInfo>();

            string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length >= 6)
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserName = parts[0];
                    userInfo.UserId = parts[2];
                    userInfo.GroupId = parts[3];
                    userInfo.HomeDirectory = parts[5];
                    userInfo.Shell = parts[6];
                    users.Add(userInfo);
                }
            }

            return users;
        }
        DateTime lastUpdated = DateTime.MinValue;
        public List<UserInfo> users = new List<UserInfo>();
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {
                var command = client.RunCommand("cat /etc/passwd");
                string psOutput = command.Result;
                users = ParsePasswdOutput(psOutput);
                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
