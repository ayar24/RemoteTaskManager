//susing Microsoft.HyperV.Management;
using System.Drawing.Design;
using System.Management;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Newtonsoft.Json;
using static System.Formats.Asn1.AsnWriter;
using System.Text.RegularExpressions;
using OpenAI_API;
using System.Text;

namespace RemoteTaskManager
{


    public partial class RemoteTaskManager : Form
    {
        SingleMachine currentVM;
        Queue<string> commandQue;
        HypderVMachineList hyperVMachines;
        RemoteMachineList remoteMachines;
        string processListSort = "";
        bool processListSortOrder = false;

        public RemoteTaskManager()
        {
            InitializeComponent();
        }
        private void DisconnectAndClose()
        {
            Close();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisconnectAndClose();
        }


        private void PopulateListViewWithHyperVMachines()
        {
            string savedSel = "";
            if (listView1.SelectedItems.Count > 0)
                savedSel = listView1.SelectedItems[0].SubItems[0].ToString();

            listView1.Items.Clear();
            // Populate ListView
            foreach (HyperVMachineInfo hyperVM in hyperVMachines.vmList)
            {
                ListViewItem item = new ListViewItem(hyperVM.Name);
                if (hyperVM.State == "3")
                    item.SubItems.Add("Off");
                else if (hyperVM.State == "2")
                    item.SubItems.Add("On");
                item.StateImageIndex = 0;
                if (savedSel == item.SubItems[0].ToString())
                    item.Selected = true;
                listView1.Items.Add(item);

            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 abt = new AboutBox1();
            abt.ShowDialog();
        }

        private void RemoteTaskManager_Load(object sender, EventArgs e)
        {
            //PopulateListViewWithHyperVMachines();
            bindingSource1.Add(new SingleMachine());
            commandQue = new Queue<string>();
            hyperVMachines = new HypderVMachineList();
            remoteMachines = new RemoteMachineList();
            LoadRemoteMachines();
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string vmName = listView1.SelectedItems[0].Text;

                currentVM = new SingleMachine();

                currentVM.machineName = vmName;
                currentVM.rootUser = Properties.Settings.Default.default_user_name;
                currentVM.rootPassword = Properties.Settings.Default.default_root_pw;
                //
                currentVM.state = "new";
                bindingSource1.Clear();
                bindingSource1.Add(currentVM);
            }
        }
        private void refreshBindings()
        {
            this.Invoke(new Action(() =>
            {
                // Refresh the data bindings on the UI thread
                bindingSource1.ResetBindings(false);
            }));
        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                if (currentVM != null)
                {
                    if (currentVM.state == "started")
                    {
                        if (currentVM.FetchIPAddress())
                            refreshBindings();
                        commandQue.Enqueue("connect");
                    }
                    else if (currentVM.state == "new")
                    {
                        if (currentVM.StartMachine() == true)
                            refreshBindings();
                        else
                        {
                            MessageBox.Show("Failed to start Hyper-V Machine");
                        }
                    }
                    else if (currentVM.state == "has-ip" || currentVM.state == "stopped" || currentVM.state == "connected")
                    {
                        if (commandQue.Count > 0)
                        {
                            string command = commandQue.Dequeue();
                            if (command != "")
                            {
                                if (command == "stop")
                                {
                                    if (currentVM.StopMachine())
                                        refreshBindings();
                                }
                                else if (command == "start")
                                {
                                    if (currentVM.StartMachine())
                                        refreshBindings();
                                }
                                else if (command == "connect")
                                {
                                    if (currentVM.ConnectToSSH())
                                        refreshBindings();
                                }
                                else if (command.Contains("get_proc_maps"))
                                {
                                    string param = command.Split(",")[1];
                                    currentVM.RefreshProcMaps(param);

                                    this.Invoke(new Action(() => ProcMapsListViewForm(param)));
                                    //   ProcMapsListViewForm();
                                }
                                else if (command.Contains("kill_process"))
                                {
                                    string param = command.Split(",")[1];
                                    currentVM.KillProcess(param);
                                }
                                else if (command.Contains("suspend_process"))
                                {
                                    string param = command.Split(",")[1];
                                    currentVM.SuspendProcess(param);
                                }
                                else if (command.Contains("resume_process"))
                                {
                                    string param = command.Split(",")[1];
                                    currentVM.ResumeProcess(param);
                                }
                            }
                        }
                    }

                    if (currentVM.state == "connected")
                    {
                        int currentTab = tabControl1.InvokeRequired ? (int)tabControl1.Invoke(new Func<int>(() => tabControl1.SelectedIndex)) : tabControl1.SelectedIndex;
                        if (currentTab == 1)
                        {
                            if (currentVM.RefreshProcessList(processListSort, processListSortOrder))
                                //ProcessListViewForm();
                                this.Invoke(new Action(ProcessListViewForm));
                        }
                        else if (currentTab == 2)
                        {
                            if (currentVM.RefreshServicesList("name", true))
                                //ProcessListViewForm();
                                this.Invoke(new Action(ServiceListViewForm));
                        }
                        else if (currentTab == 6)
                        {
                            int currentLog = toolStrip2.InvokeRequired ? (int)toolStrip2.Invoke(new Func<int>(() => toolStripComboBox1.SelectedIndex)) : toolStripComboBox1.SelectedIndex;

                            if (currentVM.GetLogEntries(currentLog))
                                //ProcessListViewForm();
                                this.Invoke(new Action(LogEntriesPopulate));
                        }
                        else if (currentTab == 5)
                        {


                            if (currentVM.RefreshUsersList("name", true))
                                //ProcessListViewForm();
                                this.Invoke(new Action(UsersListViewForm));
                        }
                    }
                }
                Thread.Sleep(50);
            }
        }
        public void LogEntriesPopulate()
        {
            //string selectedPid = "";
            int currentLog = toolStripComboBox1.SelectedIndex;
            int selectVPffset = 0;
            if (listView3.SelectedItems.Count > 0)
            {
                //    selectedPid = listView3.SelectedItems[0].SubItems[0].Text;

            }
            if (listView3.TopItem != null)
                selectVPffset = listView3.TopItem.Index;//.Count;
                                                        //Point savedVerticalScrollPosition = listView1.AutoScrollOffset;// VerticalScroll.Value;

            //   processListUpdating = true;
            listView3.BeginUpdate();
            //  listView3.SelectedItems.Clear();
            //listView4.Invoke(new Action(listView4.Items.Clear));
            listView3.Items.Clear();
            //   string filter = toolStripTextBox3.Text;
            // Populate the ListView with process information
            if (currentLog == 0 || currentLog == 2)
            {
                foreach (var logMessage in currentVM.logMessages.logMessages)
                {
                    ListViewItem item = new ListViewItem(logMessage.Timestamp.ToString());//.PID.ToString());
                    item.SubItems.Add(logMessage.Message);

                    //   if (filter == "" || process.Cmdline.Contains(filter) || process.PID.ToString().Contains(filter))
                    //listView4.Invoke(new Action(() => listView4.Items.Add(item)));
                    listView3.Items.Add(item);

                }
            }
            else if (currentLog == 1)
            {
                foreach (var logMessage in currentVM.logMessages.bpfMessages)
                {
                    ListViewItem item = new ListViewItem(logMessage.Timestamp.ToString());//.PID.ToString());
                    item.SubItems.Add(logMessage.Message);

                    //   if (filter == "" || process.Cmdline.Contains(filter) || process.PID.ToString().Contains(filter))
                    //listView4.Invoke(new Action(() => listView4.Items.Add(item)));
                    listView3.Items.Add(item);

                }
            }
            if (selectVPffset > 0)
                listView3.EnsureVisible(selectVPffset);// savedVerticalScrollPosition ;
            foreach (ColumnHeader column in listView3.Columns)
            {
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            listView3.EndUpdate();

            //processListUpdating = false;
        }

        public bool processListUpdating = false;
        public void ProcessListViewForm()
        {

            string selectedPid = "";
            int selectVPffset = 0;
            if (listView4.SelectedItems.Count > 0)
            {
                selectedPid = listView4.SelectedItems[0].SubItems[0].Text;

            }
            if (listView4.TopItem != null)
                selectVPffset = listView4.TopItem.Index;//.Count;
            //Point savedVerticalScrollPosition = listView1.AutoScrollOffset;// VerticalScroll.Value;

            processListUpdating = true;
            listView4.BeginUpdate();
            listView4.SelectedItems.Clear();
            //listView4.Invoke(new Action(listView4.Items.Clear));
            listView4.Items.Clear();
            string filter = toolStripTextBox3.Text;
            // Populate the ListView with process information
            foreach (var process in currentVM.machinePS.processes)
            {
                ListViewItem item = new ListViewItem(process.PID.ToString());
                item.SubItems.Add(process.Filename);
                item.SubItems.Add(process.Cmdline);
                item.SubItems.Add(process.CPU.ToString("F2"));
                item.SubItems.Add(process.Memory.ToString("F2"));
                if (selectedPid == process.PID)
                {
                    item.Selected = true;

                }
                if (filter == "" || process.Cmdline.Contains(filter) || process.PID.ToString().Contains(filter))
                    //listView4.Invoke(new Action(() => listView4.Items.Add(item)));
                    listView4.Items.Add(item);

            }
            if (selectVPffset > 0)
                listView4.EnsureVisible(selectVPffset);// savedVerticalScrollPosition ;
            foreach (ColumnHeader column in listView4.Columns)
            {
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            listView4.EndUpdate();

            processListUpdating = false;
        }

        public void UsersListViewForm()
        {

            string selectedPid = "";
            int selectVPffset = 0;
            if (listView7.SelectedItems.Count > 0)
            {
                selectedPid = listView7.SelectedItems[0].SubItems[0].Text;

            }
            if (listView7.TopItem != null)
                selectVPffset = listView7.TopItem.Index;//.Count;
            //Point savedVerticalScrollPosition = listView1.AutoScrollOffset;// VerticalScroll.Value;

            //processListUpdating = true;
            listView7.BeginUpdate();
            listView7.SelectedItems.Clear();
            //listView4.Invoke(new Action(listView4.Items.Clear));
            listView7.Items.Clear();
            //string filter = toolStripTextBox3.Text;
            // Populate the ListView with process information
            foreach (var user in currentVM.machineUsers.users)
            {
                ListViewItem item = new ListViewItem(user.UserName);

                item.SubItems.Add(user.UserId.ToString());
                item.SubItems.Add(user.GroupId.ToString());
                item.SubItems.Add(user.HomeDirectory);
                item.SubItems.Add(user.Shell);
                if (selectedPid == user.UserName)
                {
                    item.Selected = true;

                }
                //if (filter == "" || process.Cmdline.Contains(filter) || process.PID.ToString().Contains(filter))
                //listView4.Invoke(new Action(() => listView4.Items.Add(item)));
                listView7.Items.Add(item);

            }
            if (selectVPffset > 0)
                listView7.EnsureVisible(selectVPffset);// savedVerticalScrollPosition ;

            foreach (ColumnHeader column in listView7.Columns)
            {
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            listView7.EndUpdate();
        }
        public void ServiceListViewForm()
        {

            string selectedPid = "";
            int selectVPffset = 0;
            if (listView6.SelectedItems.Count > 0)
            {
                selectedPid = listView6.SelectedItems[0].SubItems[0].Text;

            }
            if (listView6.TopItem != null)
                selectVPffset = listView6.TopItem.Index;//.Count;
                                                        //Point savedVerticalScrollPosition = listView1.AutoScrollOffset;// VerticalScroll.Value;

            //    processListUpdating = true;
            listView6.BeginUpdate();
            listView6.SelectedItems.Clear();
            //listView4.Invoke(new Action(listView4.Items.Clear));
            listView6.Items.Clear();
            string filter = toolStripTextBox3.Text;
            // Populate the ListView with process information
            foreach (var service in currentVM.machineServices.services)
            {
                ListViewItem item = new ListViewItem(service.Name);

                item.SubItems.Add(service.Enabled);
                item.SubItems.Add(service.State);
                item.SubItems.Add(service.RunState);
                item.SubItems.Add(service.Description);
                if (selectedPid == service.Name)
                {
                    item.Selected = true;

                }
                //if (filter == "" || process.Cmdline.Contains(filter) || process.PID.ToString().Contains(filter))
                //listView4.Invoke(new Action(() => listView4.Items.Add(item)));
                listView6.Items.Add(item);

            }
            if (selectVPffset > 0)
                listView6.EnsureVisible(selectVPffset);// savedVerticalScrollPosition ;
            foreach (ColumnHeader column in listView6.Columns)
            {
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            listView6.EndUpdate();

            // processListUpdating = false;
        }
        public void ProcMapsListViewForm(string PID)
        {
            listView5.Items.Clear();
            //      if (listView4.SelectedItems.Count > 0)
            {
                //        string PID = listView4.SelectedItems[0].Text;

                // Populate the ListView with process information
                foreach (var process in currentVM.machinePS.processes)
                {
                    if (process.PID == PID)
                    {
                        if (process.memoryRegions != null)
                        {
                            foreach (var memregion in process.memoryRegions)
                            {
                                ListViewItem item = new ListViewItem(memregion.LibraryPath);
                                item.SubItems.Add(memregion.StartAddress);
                                item.SubItems.Add(memregion.StartAddress);
                                item.SubItems.Add(memregion.Permissions);

                                listView5.Items.Add(item);
                            }
                        }
                        foreach (ColumnHeader column in listView5.Columns)
                        {
                            column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                        }
                        return;
                    }
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            commandQue.Enqueue("stop");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            commandQue.Enqueue("start");
        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                HypderVMachineList hyperVMachinesNew = HyperVHelper.GetAllHyperVMachines();
                bool equaLists = true;
                if (hyperVMachinesNew.vmList.Count != hyperVMachines.vmList.Count)
                    equaLists = false;
                else
                {
                    for (int i = 0; i < hyperVMachinesNew.vmList.Count; i++)
                    {
                        if (hyperVMachinesNew.vmList[i].Name != hyperVMachines.vmList[i].Name
                             || hyperVMachinesNew.vmList[i].State != hyperVMachines.vmList[i].State)
                        {
                            equaLists = false;
                            break;
                        }
                    }
                }
                if (equaLists == false)
                // TODO: Dont update GUI if list hasnt changed.
                {
                    hyperVMachines = hyperVMachinesNew;
                    this.Invoke(new Action(PopulateListViewWithHyperVMachines));
                }
                Thread.Sleep(3000);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            string ip = toolStripTextBox1.Text;
            // check unique first..
            if (HyperVHelper.IsValidIPv4(ip) == false)
            {
                MessageBox.Show("Not a valid IP address");
                return;
            }
            foreach (var remoteMachine in remoteMachines.vmList)
            {
                if (remoteMachine.IP == ip)
                {
                    MessageBox.Show("IP Already added");
                    return;
                }
            }
            RemoteMachineInfo rmInfo = new RemoteMachineInfo();
            rmInfo.IP = ip;
            rmInfo.Port = 22;

            remoteMachines.vmList.Add(rmInfo);

            AddRemoteMachineToListView(rmInfo);

            toolStripTextBox1.Text = "";
            SaveRemoteMachines();
        }
        private void AddRemoteMachineToListView(RemoteMachineInfo rmInfo)
        {
            ListViewItem item = new ListViewItem(rmInfo.IP);
            item.SubItems.Add("22");
            item.StateImageIndex = 0;

            listView2.Items.Add(item);
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string selectedIP = listView2.SelectedItems[0].Text;

                foreach (var remoteMachine in remoteMachines.vmList)
                {
                    if (remoteMachine.IP == selectedIP)
                    {
                        remoteMachines.vmList.Remove(remoteMachine);
                        break;
                    }
                }
                listView2.Items.Remove(listView2.SelectedItems[0]);
                SaveRemoteMachines();
            }
        }

        private void turnOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            commandQue.Enqueue("stop");
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            commandQue.Enqueue("start");
        }

        private void defaultCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CredentialsDlg dlg = new CredentialsDlg();

            dlg.UserName = Properties.Settings.Default.default_user_name;
            dlg.Password = Properties.Settings.Default.default_root_pw;
            dlg.Port = Properties.Settings.Default.default_port;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.default_user_name = dlg.UserName;
                Properties.Settings.Default.default_root_pw = dlg.Password;
                Properties.Settings.Default.default_port = dlg.Port;
                Properties.Settings.Default.Save();
            }
        }
        private void LoadRemoteMachines()
        {
            string json = Properties.Settings.Default.remote_machines;
            if (json != "")
            {
                remoteMachines = JsonConvert.DeserializeObject<RemoteMachineList>(json);
            }
            foreach (var remoteMachine in remoteMachines.vmList)
            {
                AddRemoteMachineToListView(remoteMachine);
            }
        }
        private void SaveRemoteMachines()
        {
            string json = JsonConvert.SerializeObject(remoteMachines, Formatting.Indented);
            Properties.Settings.Default.remote_machines = json;
            Properties.Settings.Default.Save();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string selectedIP = listView2.SelectedItems[0].Text.ToString();

                foreach (var remoteMachine in remoteMachines.vmList)
                {
                    if (remoteMachine.IP == selectedIP)
                    {
                        CredentialsDlg dlg = new CredentialsDlg();
                        dlg.UserName = remoteMachine.UserName;
                        dlg.Password = remoteMachine.Password;
                        dlg.Port = remoteMachine.Port.ToString();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            remoteMachine.UserName = dlg.UserName;
                            remoteMachine.Password = dlg.Password;
                            string input = dlg.Port; // Replace with your input string
                            if (int.TryParse(input, out int result))
                            {
                                remoteMachine.Port = result;
                            }
                            else
                            {
                                remoteMachine.Port = 22;
                            }
                        }
                        else
                        {
                            return;
                        }
                        SaveRemoteMachines();
                        break;
                    }
                }
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                toolStripButton4.Enabled = true;
                toolStripButton1.Enabled = true;
            }
            else
            {
                toolStripButton4.Enabled = false;
                toolStripButton1.Enabled = false;
            }
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (HyperVHelper.IsValidIPv4(toolStripTextBox1.Text))
                toolStripButton3.Enabled = true;
            else
                toolStripButton3.Enabled = false;
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string vmName = listView2.SelectedItems[0].Text;

                currentVM = new SingleMachine();

                currentVM.machineName = vmName;
                currentVM.IP = vmName;
                //
                currentVM.state = "has-ip";
                bindingSource1.Clear();
                bindingSource1.Add(currentVM);
                commandQue.Enqueue("connect");
            }
        }

        private void toolStrip3_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox3_TextChanged(object sender, EventArgs e)
        {
            ProcessListViewForm();
        }
        public string last_selected_process = "-1";
        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0 && processListUpdating == false)
            {

                string PID = listView4.SelectedItems[0].Text;
                if (PID != "" && PID != last_selected_process)
                {
                    last_selected_process = PID;
                    commandQue.Enqueue("get_proc_maps," + PID);
                }
            }
        }

        private void terminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                string PID = listView4.SelectedItems[0].Text;
                commandQue.Enqueue("kill_process," + PID);
            }
        }

        private void suspendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                string PID = listView4.SelectedItems[0].Text;
                commandQue.Enqueue("suspend_process," + PID);
            }
        }

        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                string PID = listView4.SelectedItems[0].Text;
                commandQue.Enqueue("resume_process," + PID);
            }
        }

        private void listView4_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (processListSort == e.Column.ToString())
                processListSortOrder = !processListSortOrder;
            else
                processListSort = e.Column.ToString();
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {

        }
        static string InsertLineBreaksEveryNWords(string input, int n)
        {
            // Use regular expressions to split the input into words
            string[] words = Regex.Split(input, @"\s+");

            // Use a StringBuilder to construct the result
            StringBuilder result = new StringBuilder();
            int wordCount = 0;

            foreach (string word in words)
            {
                result.Append(word);
                wordCount++;

                // Insert a line break after every 'n' words
                if (wordCount % n == 0)
                    result.Append("\n");
                else
                    result.Append(" "); // Add a space between words
            }

            return result.ToString();
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            string apiKey = Properties.Settings.Default.openai_api_key;

            OpenAIAPI api = new OpenAIAPI(apiKey);
           
            var chat = api.Chat.CreateConversation();

            /// give instruction as System
            chat.AppendSystemMessage("You are an IT system engineer exploring the process list of a machine, and provide your analysis on that machine.");

            // give a few examples as user and assistant
            //  chat.AppendUserInput("Is this an animal? Cat");
            //  chat.AppendExampleChatbotOutput("Yes");
            //  chat.AppendUserInput("Is this an animal? House");
            //  chat.AppendExampleChatbotOutput("No");

            // now let's ask it a question'
            chat.AppendUserInput("What do you think about this process list? do you see any special risk ?");
            foreach (var process in currentVM.machinePS.processes)
            {
                chat.AppendUserInput("Cmdline: " + process.Cmdline + " Memory: " + process.Memory + " CPU: " + process.CPU);
            }
            // and get the response
            //    Task<string> response = chat.GetResponseFromChatbotAsync();
            //Task.
            string response = "";
            Task.Run(async () =>
             {
                 response = await chat.GetResponseFromChatbotAsync(); // Call the async function within Task.Run
                 Console.WriteLine(response);

                 // Continue with any code that needs to run after the async operation is complete
             }).GetAwaiter().GetResult(); // Block the current thread until the Task is complete
                                          // toolTip1.ToolTipTitle = response;
                                          //  toolTip1.

            response = InsertLineBreaksEveryNWords(response, 10);
            toolTip1.SetToolTip(listView4, response);
            toolTip1.ShowAlways = true;
        }

        private void openAIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenAISettings dlg = new OpenAISettings();

            dlg.APIKey = Properties.Settings.Default.openai_api_key;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.openai_api_key = dlg.APIKey;
                Properties.Settings.Default.Save();
            }
        }

        private void startMonitoringToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }
    }
}