using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http.Json;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using static System.Windows.Forms.LinkLabel;

namespace RemoteTaskManager
{
    // Class that represents a connection to a single machine
    public class SingleMachine
    {
        private SshClient gClient;
        public SingleMachine()
        {
            canOpenTerminal = false;
            canCanOpenCockpit = false;
            canStart = false;
            canStop = false;
            canRestart = false;
            machinePS = new SingleMachineProcessList();
            machineServices = new SingleMachineServicesList();
            machineUsers = new SingleMachineUsersList();
            logMessages = new SingleMachineLogs();
            machineRPM = new SingleMachineRPM();
            machineDisk = new SingleMachineDisk();
            machineCron = new SingleMachineCron();
            machineSELinux = new SingleMachineSELinux();
            machineNMON = new SingleMachineNMon();
        }
        public string? mystring { get; set; }
        public string? machineName { get; set; }
        public string? IP { get; set; }
        public string? state { get; set; }
        public string? OSVersion { get; set; }
        public string? TotalRAM { get; set; }
        public string? CPUType { get; set; }
        public string? KernelVersion { get; set; }
        public string? rootUser { get; set; }
        public string? rootPassword { get; set; }
        public bool canStart { get; set; }
        public bool canStop { get; set; }
        public bool canRestart { get; set; }
        public bool canOpenTerminal { get; set; }
        public bool canCanOpenCockpit { get; set; }
        public bool canDisconnect { get; set; }
        public bool isUbunto { get; set; }
        public bool isCentOs { get; set; }
        public bool isWindows { get; set; }

        public SingleMachineRPM machineRPM;
        public SingleMachineNMon machineNMON;
        public SingleMachineDisk machineDisk;
        public SingleMachineCron machineCron;
        public SingleMachineProcessList machinePS;
        public SingleMachineServicesList machineServices;
        public SingleMachineUsersList machineUsers;
        public SingleMachineSELinux machineSELinux;
        public SingleMachineLogs logMessages;

        public string? statusMessage { get; set; }

        public bool RefreshServicesList(string sort, bool asc)
        {
            return machineServices.Refresh(gClient, sort, asc);
        }
        public bool RefreshUsersList(string sort, bool asc)
        {
            return machineUsers.Refresh(gClient, sort, asc);
        }
        public bool RefreshRPMList(string sort, bool asc)
        {
            if (isCentOs)
                return machineRPM.RefreshCentos(gClient, sort, asc);
            return machineRPM.Refresh(gClient, sort, asc);
        }
        public bool RefreshCronList(string sort, bool asc)
        {
            return machineCron.Refresh(gClient, sort, asc);
        }
        public bool RefreshDiskList(string sort, bool asc)
        {
            return machineDisk.Refresh(gClient, sort, asc);
        }
        public bool RefreshSELinux(string sort, bool asc)
        {
            if (isCentOs)
                return machineSELinux.RefreshCentos(gClient, sort, asc);
            return machineSELinux.Refresh(gClient, sort, asc);
        }
        public bool RefreshNMonList(string sort, bool asc)
        {
            return machineNMON.Refresh(gClient, sort, asc);
        }
        public bool RefreshProcessList(string sort, bool asc)
        {
            return machinePS.Refresh(gClient, sort, asc);
        }
        public bool RefreshProcMaps(string PID)
        {
            machinePS.GetMemoryRegions(gClient, PID);
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

            if (isUbunto)
            {
                if (logID == 1)
                {
                    return false;// logMessages.GeteBPFOutput(gClient);
                }
                else if (logID == 0)
                {
                    logFile = "/var/log/syslog";
                    return logMessages.GetLogEntries(gClient, logFile);

                }
                else if (logID == 2)
                {
                    logFile = "/var/log/auth.log";
                    return logMessages.GetLogEntries(gClient, logFile);
                }
            } else if (isCentOs)
            {
                if (logID == 1)
                {
                    return false;// logMessages.GeteBPFOutput(gClient);
                }
                else if (logID == 0)
                {
                    logFile = "/var/log/messages";
                    return logMessages.GetLogEntries(gClient, logFile);

                }
                else if (logID == 2)
                {
                    logFile = "/var/log/secure";
                    return logMessages.GetLogEntries(gClient, logFile);
                }
            }
            return false;
        }
        public bool StartMachine()
        {
            bool ret = HyperVHelper.StartHyperVVM(machineName, true);
            state = "started";
            statusMessage = "Getting IP Address from VM..";

            return ret;
        }
        public bool StopMachine()
        {
            HyperVHelper.StartHyperVVM(machineName, false);
            state = "stopped";
            statusMessage = "Machine Stopped.";
            canStop = false;
            canStart = true;
            canRestart = false;
            canOpenTerminal = false;
            canCanOpenCockpit = false;
            canDisconnect = false;
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
                ManagementObjectCollection settings = vm.GetRelated("Msvm_VirtualSystemSettingData", "Msvm_SettingsDefineState", null, null, "SettingData", "ManagedElement", false, null);
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
        public bool DisconnectFromSSH()
        {
            if (gClient != null)
            {
                if (gClient.IsConnected)
                {
                    gClient.Disconnect();
                }
            }
            return true;
        }
        public bool ConnectToSSH()
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

            if (rootUser == "" || rootPassword == "")
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
                    bool bIsWindows = false;
                    gClient.Connect();

                    if (gClient.IsConnected)
                    {
                        string osVersionCommand = "cat /etc/os-release | grep PRETTY_NAME | awk -F '\"' '{print $2}'";
                        string kernelVersionCommand = "uname -r";
                        string totalRamCommand = "free -m | awk '/Mem:/ {print $2}'";
                        string cpuInfoCommand = "lscpu | grep 'Model name' | awk -F ':' '{print $2}'";

                        if (gClient.ConnectionInfo.ServerVersion.Contains("Windows"))
                        {
                            bIsWindows = true;
                            osVersionCommand = "systeminfo | findstr /B /C:\"OS Name\" /C:\"OS Version\"";
                            kernelVersionCommand = "uname -r";
                            totalRamCommand = "wmic memorychip get capacity";
                            cpuInfoCommand = "wmic cpu get Name, NumberOfCores, NumberOfLogicalProcessors /format:list";

                        }

                        string osVersion = ExecuteCommand(gClient, osVersionCommand);
                        string kernelVersion = ExecuteCommand(gClient, kernelVersionCommand);
                        string totalRam = ExecuteCommand(gClient, totalRamCommand);
                        string cpuInfo = ExecuteCommand(gClient, cpuInfoCommand);

                        cpuInfo = cpuInfo.TrimStart();
                        totalRam = totalRam.Replace("\r\n", "");
                        totalRam = totalRam.Replace("\n", "");
                        if (bIsWindows)
                        {
                            osVersion = osVersion.Split("\r")[0].Split(":")[1];
                            osVersion = osVersion.TrimStart();
                            cpuInfo = (cpuInfo.Split("\r\n"))[0].Split("=")[1];
                            totalRam = totalRam.Split("\r")[1];
                            this.isWindows = true;
                        }
                        else
                            this.isWindows = false;
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
                        canDisconnect = true;

                        isCentOs = false;
                        isUbunto = false;
                        if (osVersion.Contains("Ubuntu"))
                            isUbunto = true;
                        if (osVersion.Contains("Red Hat"))
                            isCentOs = true;
                        if (osVersion.Contains("Oracle"))
                            isCentOs = true;

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
            return res;
        }

        
    }
    
 }
