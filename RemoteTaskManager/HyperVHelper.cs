using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class RemoteMachineInfo
    {
        public RemoteMachineInfo () {}
        public string? IP { get; set; }
        public int? Port { get; set; } = 22;
        public string? UserName { get; set; } = "";
        public string? Password { get; set; } = "";
    }

        public class HyperVMachineInfo : RemoteMachineInfo
    {
        public HyperVMachineInfo() { }
        public HyperVMachineInfo (string name, string state, string description)
        {
            Name = name;
            State = state;
            Description = description;
        }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? Description { get; set; }
    }
   public class HypderVMachineList
    {
        public HypderVMachineList() { vmList = new List<HyperVMachineInfo> { }; }
        public List<HyperVMachineInfo> vmList { get; set; }
    }
    public class RemoteMachineList
    {
        public RemoteMachineList() { vmList = new List<RemoteMachineInfo> { }; }
        public List<RemoteMachineInfo> vmList { get; set; }
    }
    public  class HyperVHelper
    {
        public static bool IsValidIPv4(string ipAddress)
        {
            // Regular expression to match a valid IPv4 address
            string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            if (Regex.IsMatch(ipAddress, pattern))
            {
                // Check if the IP address can be parsed by the IPAddress class
                if (IPAddress.TryParse(ipAddress, out IPAddress parsedIPAddress))
                {
                    // Ensure it's an IPv4 address
                    return parsedIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
                }
            }

            return false;
        }
  
        public static HypderVMachineList GetAllHyperVMachines()
        {
            HypderVMachineList result = new HypderVMachineList();// new List<HyperVMachineInfo> { };
            // Create a connection to the Hyper-V management scope
            ManagementScope scope = new ManagementScope(@"\\.\root\virtualization\v2");

            // Create a query to retrieve virtual machines
            ObjectQuery query = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem WHERE Caption = 'Virtual Machine'");

            // Initialize the searcher with the query and scope
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            // Get the list of virtual machines
            ManagementObjectCollection vmCollection = searcher.Get();
            //  listView1.Columns.Add("Hyper-V VMs");
            //listView1.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle())
            //      listView1.Columns.Add("State");

            // Populate ListView
            foreach (ManagementObject vm in vmCollection)
            {
                HyperVMachineInfo hyperVMachineInfo = new HyperVMachineInfo(vm["ElementName"].ToString(),
                    vm["EnabledState"].ToString(), vm["Description"].ToString());

                result.vmList.Add(hyperVMachineInfo);
            }
            return result;
        }

        public static bool StartHyperVVM(string machineName, bool enabled)
        {
            bool ret = true;
            string vmName = machineName;
            try
            {
                // Connect to the Hyper-V virtualization namespace
                ManagementScope scope = new ManagementScope(@"\\.\root\virtualization\v2");

                // Create a management object representing the virtual machine
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery($"SELECT * FROM Msvm_ComputerSystem WHERE ElementName = '{vmName}'"));
                ManagementObject vm = searcher.Get().OfType<ManagementObject>().FirstOrDefault();

                if (vm != null)
                {
                    // Invoke the Start method to start the virtual machine
                    ManagementBaseObject inParams = vm.GetMethodParameters("RequestStateChange");
                    if (enabled == true)
                        inParams["RequestedState"] = 2; // 2 means "Enabled"
                    else
                        inParams["RequestedState"] = 3; // 2 means "Disabled"

                    ManagementBaseObject outParams = vm.InvokeMethod("RequestStateChange", inParams, null);

                    uint returnValue = (uint)outParams["ReturnValue"];

                    if (returnValue == 0)
                    {
                        Console.WriteLine($"Virtual machine '{vmName}' started successfully.");
                        ret = true;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to start virtual machine '{vmName}'. Error code: {returnValue}");
                        ret = true;
                    }
                }
                else
                {
                    Console.WriteLine($"Virtual machine '{vmName}' not found.");
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                ret = false;
            }
            return ret;
        }
    }
}
