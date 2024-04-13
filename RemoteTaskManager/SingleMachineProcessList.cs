using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RemoteTaskManager.SingleMachineProcessList;

namespace RemoteTaskManager
{
    public class SingleMachineProcessList
    {
        public class MemoryRegion
        {
            public string LibraryPath { get; }
            public string StartAddress { get; }
            public string EndAddress { get; }
            public string Permissions { get; }

            public MemoryRegion(string libraryPath, string startAddress, string endAddress, string permissions)
            {
                LibraryPath = libraryPath;
                StartAddress = startAddress;
                EndAddress = endAddress;
                Permissions = permissions;
            }
        }

        public class ProcessInfo
        {
            public string PID { get; }
            public string Filename { get; }
            public string Cmdline { get; }
            public double CPU { get; }
            public double Memory { get; }

            public List<MemoryRegion> memoryRegions;

            public ProcessInfo(string pid, string filename, string cmdline, double cpu, double memory)
            {
                PID = pid;
                Filename = filename;
                Cmdline = cmdline;
                CPU = cpu;
                Memory = memory;

            }
        }
        public List<ProcessInfo> processes = new List<ProcessInfo>();

        DateTime lastUpdated = DateTime.MinValue;
        public bool Refresh(SshClient client, string sort, bool asc)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 3000 || lastUpdated == DateTime.MinValue)
            {

                if (sort == "")
                    sort = "%cpu"; 
                else if (sort == "4")
                    sort = "%mem";
                if (sort == "3")
                    sort = "%cpu";
                if (sort == "2")
                    sort = "command";
                if (sort == "0")
                    sort = "pid";
                if (asc == true)
                    sort = "-" + sort;

                processes.Clear();
                var command = client.RunCommand("ps -eo pid,command,%cpu,%mem --sort=" + sort);// -%cpu");// | head -n 100");
                string psOutput = command.Result;

                string[] lines = psOutput.Split('\n');
                foreach (var line in lines)
                {
                    string[] parts = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 && int.TryParse(parts[0], out int pid) && double.TryParse(parts[2], out double cpu) && double.TryParse(parts[3], out double memory))
                    {
                        string commandLine = string.Join(" ", parts, 1, parts.Length - 3);
                        processes.Add(new ProcessInfo(pid.ToString(), "", commandLine, cpu, memory));
                    }
                }

                lastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        public void KillProcess (SshClient client, string PID)
        {
            var command = client.RunCommand($"kill -f {PID}");
        }
        public void SuspendProcess(SshClient client, string PID)
        {
            var command = client.RunCommand($"kill STOP {PID}");
        }
        public void ResumeProcess(SshClient client, string PID)
        {
            var command = client.RunCommand($"kill CONT {PID}");
        }
        public void GetMemoryRegions(SshClient client , string pid)
        {
           // List<MemoryRegion> memoryRegions = new List<MemoryRegion>();
            foreach (var processInfo in processes)
            {
                if (processInfo.PID == pid)
                {
                    processInfo.memoryRegions = new List<MemoryRegion>(); 
                    var command = client.RunCommand($"cat /proc/{pid}/maps");
                    string mapsOutput = command.Result;

                    using (StringReader reader = new StringReader(mapsOutput))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Match match = Regex.Match(line, @"([0-9A-Fa-f]+)-([0-9A-Fa-f]+)\s+([rwxsp-]+)\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+:[0-9A-Fa-f]+\s+[0-9]+\s*(.+)");
                            if (match.Success)
                            {
                                string startAddress = match.Groups[1].Value;
                                string endAddress = match.Groups[2].Value;
                                string permissions = match.Groups[3].Value;
                                string libraryPath = match.Groups[4].Value.Trim();

                                processInfo.memoryRegions.Add(new MemoryRegion(libraryPath, startAddress, endAddress, permissions));
                            }
                        }
                    }

                    return;// memoryRegions;
                    
                }
            }
        }
    }
}
