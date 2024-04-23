using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    public class LogMessage
    {
        public LogMessage(string message, string timestamp)
        {
            Timestamp = timestamp;
            Message = message;
        }
        public string Timestamp { get; set; }
        public string Message { get; set; }
    }
    public class SingleMachineLogs
    {
        public SingleMachineLogs()
        {
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

                    logMessages.Add(new LogMessage(message, timestamp));
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
        public bool GetLogEntries(SshClient client, string logFile)
        {
            TimeSpan timeDifference = DateTime.Now.Subtract(lastUpdated);

            if (timeDifference.TotalMilliseconds > 10000 || lastUpdated == DateTime.MinValue)
            {
                if (logFile != lastLogFile)
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
   
}
