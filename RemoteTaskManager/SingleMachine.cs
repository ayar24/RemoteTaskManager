using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.Logging;
using Renci.SshNet;
using static System.Windows.Forms.LinkLabel;

namespace RemoteTaskManager
{

    public class LogMessage
    {
       public LogMessage (string message, string timestamp)
        {
            Timestamp = timestamp;
            Message = message;
        }
        public string Timestamp { get; set; }
        public string Message { get; set; }
    }
    public class LOGMessages {
        public LOGMessages () { 
            logMessages = new List<LogMessage>();
            bpfMessages = new List<LogMessage>();
            bpfInstalled = false;
        }
        private bool bpfInstalled = false;
        private void ParseLogMessages(string logFileContents)
        {
            List<LogMessage> logEntries = new List<LogMessage>();

            string[] lines = logFileContents.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                // Split the line into parts
                string[] parts = line.Split(new[] { ' ' }, 6, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 6)
                {
                    string timestamp = $"{parts[0]} {parts[1]} {parts[2]}";
                    string machineName = parts[4];
                    string processInfo = parts[5];
                    string message = parts[5];

                    logMessages.Add(new LogMessage(message, timestamp ));
                }
            }

            return;// logEntries;
        }
        private void ParseBPFMessages(string logFileContents)
        {
            List<LogMessage> logEntries = new List<LogMessage>();

            string[] lines = logFileContents.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                // Split the line into parts
                string[] parts = line.Split(new[] { ' ' }, 6, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 6)
                {
                    string timestamp = $"{parts[0]} {parts[1]} {parts[2]}";
                    string machineName = parts[4];
                    string processInfo = parts[5];
                    string message = parts[5];

                    bpfMessages.Add(new LogMessage(message, timestamp));
                }
            }

            return;// logEntries;
        }
        public string lastLogFile = "";
        public bool GetLogEntries (SshClient client, string logFile)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 10000 || lastUpdated == DateTime.MinValue)
            {
                if (logFile != lastLogFile )
                {
                    logMessages.Clear();
                    lastLogFile = logFile;
                }
                var lineCountCommand = client.CreateCommand($"wc -l {logFile}");
                int currentLineCount = int.Parse(lineCountCommand.Execute().Split(' ')[0]);

                int newLineCount = currentLineCount - logMessages.Count;

                string commandStr = $"cat {logFile}";
                if (logMessages.Count > 0 && newLineCount > 0)
                        commandStr = $"tail -n {newLineCount} {logFile}";
                if (newLineCount > 0 || logMessages.Count == 0)
                {
                    var command = client.RunCommand(commandStr);

                    string logContent = command.Result;
                    if (logContent == "")
                    {
                        var command2 = client.RunCommand("cat /var/log/messages");
                        logContent = command2.Result;
                    }
                    ParseLogMessages(logContent);
                    lastUpdated = DateTime.Now;
                    return true;
                }
            }
            return false;
        }
        public bool GeteBPFOutput(SshClient client)
        {
            if (bpfInstalled == false)
            {
                bpfInstalled = true;
                using (var scp = new ScpClient(client.ConnectionInfo))
                {
                    scp.Connect();

                    // Upload the local file to the remote server
                    using (var fileStream = new FileStream("syscall_hook_c.txt", FileMode.Open))
                    {
                        scp.Upload(fileStream, "syscall_hook.c");
                    }

                    scp.Disconnect();
                }
                client.RunCommand("sudo apt-get install -y llvm clang bpfcc-tools linux-headers-$(uname -r)");
                client.RunCommand("clang -O2 -target bpf -c syscall_hook.c -o syscall_hook.o");
                client.RunCommand("bpftool prog load syscall_hook.o /sys/fs/bpf/syscall_hook");
                client.RunCommand("bpftool prog attach /sys/fs/bpf/syscall_hook type tracepoint name sys_enter_read");
                client.RunCommand("bpftool prog attach /sys/fs/bpf/syscall_hook type tracepoint name sys_enter_write");
            }
            var command = client.RunCommand("cat /sys/kernel/debug/tracing/trace_pipe");
            ParseBPFMessages(command.Result);
            return true;
        }

        public List<LogMessage> logMessages;
        public List<LogMessage> bpfMessages;
        DateTime lastUpdated = DateTime.MinValue;
    }
    public class RPM
    {
        DateTime lastUpdated;

    }
    public class NMON
    {
        DateTime lastUpdated;
    }
   
    public class SELinux
    {
        DateTime lastUpdated;
    }
    public class SingleMachine
    {
        private SshClient gClient;
        public SingleMachine ()
        {
            canOpenTerminal = false;
            canCanOpenCockpit = false;
            canStart = false;
            canStop = false;
            canRestart = false;
            machinePS = new SingleMachineProcessList();
            machineServices = new SingleMachineServicesList();
            machineUsers = new SingleMachineUsersList();
            logMessages = new LOGMessages();
        }
        public string? mystring { get; set; } 
        public string? machineName { get; set; }
        public string? IP  { get; set; }
        public string? state  { get; set; } 
        public string? OSVersion  { get; set; } 
        public string? TotalRAM  { get; set; } 
        public string? CPUType  { get; set; } 
        public string? KernelVersion  { get; set; } 
        public string? rootUser  { get; set; } 
        public string? rootPassword  { get; set; }
        public bool canStart { get; set; }
        public bool canStop { get; set; }
        public bool canRestart { get; set; }
        public bool canOpenTerminal { get; set; }
        public bool canCanOpenCockpit { get; set; }

        public RPM      machineRPM;
        public NMON     machineNMON;
        public SingleMachineProcessList     machinePS;
        public SingleMachineServicesList    machineServices;
        public SingleMachineUsersList       machineUsers;
        public SELinux  machineSELinux;
        public LOGMessages  logMessages;

        public string? statusMessage { get; set; }

        public bool RefreshServicesList(string sort, bool asc)
        {
            return machineServices.Refresh(gClient, sort, asc);
        }
        public bool RefreshUsersList(string sort, bool asc)
        {
            return machineUsers.Refresh(gClient, sort, asc);
        }
        public bool RefreshProcessList (string sort, bool asc)
        {
            return machinePS.Refresh(gClient,sort, asc);
        }
        public bool RefreshProcMaps(string PID)
        {
            machinePS.GetMemoryRegions(gClient,PID);
            return true;
        }
        public bool KillProcess(string PID)
        {
            machinePS.KillProcess(gClient, PID);
            return true;
        }
        public bool SuspendProcess(string PID)
        {
            machinePS.SuspendProcess(gClient, PID);
            return true;
        }
        public bool ResumeProcess(string PID)
        {
            machinePS.ResumeProcess(gClient, PID);
            return true;
        }
        public bool GetLogEntries(int logID)
        {
            string logFile = "";
            if (logID ==1)
            {
                return false;// logMessages.GeteBPFOutput(gClient);
            }else if (logID == 0)
            {
                logFile = "/var/log/syslog";
                return logMessages.GetLogEntries(gClient, logFile);

            } else if (logID ==2)
            {
                logFile = "/var/log/auth.log";
                return logMessages.GetLogEntries(gClient, logFile);
            }
            return false;
        }
        public bool StartMachine  ()
        {
           bool ret=  HyperVHelper.StartHyperVVM(machineName,true);
            state = "started";
            statusMessage = "Getting IP Address from VM..";
            
            return ret;
        }
        public bool StopMachine()
        {
            HyperVHelper.StartHyperVVM(machineName,false);
            state = "stopped";
            statusMessage = "Machine Stopped.";
            canStop = false;
            canStart = true;
            canRestart = false;
            canOpenTerminal = false;
            canCanOpenCockpit = false;
            IP = "";
            return true;
        }

        public bool FetchIPAddress()
        {
           
            IP = GetIPAddress(machineName);
            if (IP != "")
            {
                statusMessage = "Got IP Address from VM..Connecting..";
                state = "has-ip";
                canOpenTerminal = true;
                canCanOpenCockpit = true;
                canStop = true;
                canRestart = true;
                canStart = false;
                return true;
            }
            else
            {
               
            }
            return false;
        }
        private string GetIPAddress(string vmName)
        {
            ManagementScope scope = new ManagementScope(@"\\.\root\virtualization\v2");

            // Query for the selected virtual machine
            ObjectQuery query = new ObjectQuery($"SELECT * FROM Msvm_ComputerSystem WHERE ElementName = '{vmName}'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection vmCollection = searcher.Get();

            foreach (ManagementObject vm in vmCollection)
            {
                vm.Get();
                ManagementObjectCollection settings = vm.GetRelated("Msvm_VirtualSystemSettingData","Msvm_SettingsDefineState",null,null,"SettingData","ManagedElement",false,null);
                foreach (ManagementObject settring in settings)
                {
                    settring.Get();
                    ManagementObjectCollection adapters = settring.GetRelated("Msvm_SyntheticEthernetPortSettingData");
                    foreach (ManagementObject adapter in adapters)
                    {
                        ManagementObjectCollection configurations = adapter.GetRelated("Msvm_GuestNetworkAdapterConfiguration");
                        foreach (ManagementObject configuration in configurations)
                        {
                            configuration.Get();
                            string[] ipAddresses = configuration["IPAddresses"] as string[];
                            if (ipAddresses != null && ipAddresses.Length > 0)
                            {
                                return ipAddresses[0]; // Return the first IP address
                            }
                        }
                    }
                }
            }
            return "";
        }

        private string ExecuteCommand(SshClient client, string command)
        {
            using (var cmd = client.CreateCommand(command))
            {
                var result = cmd.Execute();
                return result;
            }
        }

        public bool ConnectToSSH ()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                // Always return true to ignore certificate validation errors
                return true;
            };

            string host = IP;
            if (rootUser is null || rootPassword is null) 
                return false;
            if (rootUser == ""|| rootPassword == "")
                return false;
            string username = rootUser;
            string password = rootPassword;
            int port = 22; // The default SSH port
            bool res = false;
            if (gClient == null)
            {
                gClient = new SshClient(host, port, username, password);
            }
           // using (var client = new SshClient(host, port, username, password))
            {
                
                try
                {
                    gClient.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(2000);
                    
                    gClient.Connect();

                    if (gClient.IsConnected)
                    {
                        // Execute commands to get system information
                        string osVersionCommand = "cat /etc/os-release | grep PRETTY_NAME | awk -F '\"' '{print $2}'";
                        string kernelVersionCommand = "uname -r";
                        string totalRamCommand = "free -m | awk '/Mem:/ {print $2}'";
                        string cpuInfoCommand = "lscpu | grep 'Model name' | awk -F ':' '{print $2}'";
                        
                        string osVersion = ExecuteCommand(gClient, osVersionCommand);
                        string kernelVersion = ExecuteCommand(gClient, kernelVersionCommand);
                        string totalRam = ExecuteCommand(gClient, totalRamCommand);
                        string cpuInfo = ExecuteCommand(gClient, cpuInfoCommand);
                        
                        cpuInfo = cpuInfo.TrimStart();
                        totalRam = totalRam.Replace("\n", "");
                        Console.WriteLine("OS Version: " + osVersion);
                        Console.WriteLine("Kernel Version: " + kernelVersion);
                        Console.WriteLine("Total RAM (MB): " + totalRam);
                        Console.WriteLine("CPU: " + cpuInfo);

                        this.OSVersion = osVersion;
                        this.KernelVersion = kernelVersion;
                        this.TotalRAM = totalRam + " MB";
                        this.CPUType = cpuInfo;
                        state = "connected";
                        res = true;
                        statusMessage = "Connected.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                   // gClient.Disconnect();
                }
            }
            return res ;
        }

    }
    
 }
