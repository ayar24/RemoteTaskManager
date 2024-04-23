using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RemoteTaskManager
{
    public partial class VulnerScan : Form
    {
        SingleMachine currentVM = null;
        public void SetCurrentVM(SingleMachine current) { currentVM = current; }
        public VulnerScan()
        {
            InitializeComponent();
        }

        private void scanText_TextChanged(object sender, EventArgs e)
        {

        }

        private void VulnerScan_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();


            //currentVM.ScanVonrabilities2();
        }
        private const string VulnersBaseUrl = "https://vulners.com/api/v3/";
        public string VulnersAPIKey = "ZP5Z5G6M6W9RJUH9U2KDW2ENT7P4GK3PFI93KYEM9Z9EIROLII5MGUD3W4ES9IAW\"";

        public async Task<List<string>> GetPackageVulnerabilitiesAsync2(List<string> packages)
        {
            List<string> vulnerabilities = new List<string>();

            using (var httpClient = new HttpClient())
            {
                foreach (string package in packages)
                {
                    string url = $"{VulnersBaseUrl}search/lucene/";// software/audit";

                    var requestData = new
                    {
                        software = package,
                        apiKey = VulnersAPIKey
                    };

                    var response = await httpClient.PostAsJsonAsync(url, requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        dynamic data = JObject.Parse(jsonResponse);

                        // Extract vulnerabilities from the JSON response
                        foreach (var vuln in data["data"][0]["vulnerabilities"])
                        {
                            string description = vuln["description"];
                            vulnerabilities.Add(description.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve vulnerabilities for package: {package}");
                    }
                }
            }

            return vulnerabilities;
        }

        private const string NvdBaseUrl = "https://services.nvd.nist.gov/rest/json/cves/2.0";

        public async Task<List<string>> GetPackageVulnerabilitiesAsync(List<string> packages)
        {
            List<string> vulnerabilities = new List<string>();
            int currentIndex = 0;
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                //foreach (string package in packages)
                while (currentIndex + 30 < packages.Count)
                {
                    string url = $"{NvdBaseUrl}?keywordSearch={string.Join(" ", packages.GetRange(currentIndex, currentIndex + 30))}";
                    currentIndex += 30;
                    HttpResponseMessage response = new HttpResponseMessage();
                    try
                    {
                        response = await httpClient.GetAsync(url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error occurred: {ex.Message}");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(jsonResponse);

                        // Extract vulnerabilities from the JSON response
                        foreach (var result in data["vulnerabilities"])
                        {
                            string description = result["cve"]["description"]["description_data"][0]["value"];
                            vulnerabilities.Add(description.ToString());
                        }
                    }
                    else
                    {
                        //  Console.WriteLine($"Failed to retrieve vulnerabilities for package: {package}");
                    }
                }
            }

            return vulnerabilities;
        }
        public
        async Task ScanVonrabilitiesTask()
        {
            List<string> packages = new List<string> { };
            foreach (var package in currentVM.machineRPM.packages)
                packages.Add(package.Name);
            List<string> vulnerabilities = await GetPackageVulnerabilitiesAsync(packages);

            Console.WriteLine("Known vulnerabilities:");
            foreach (string vulnerability in vulnerabilities)
            {
                Console.WriteLine(vulnerability);
            }
        }
        public
        void ScanVonrabilities()
        {
            Task.Run(async () =>
            {
                await ScanVonrabilitiesTask();
            }).Wait(); // or use .Result

        }
        public
       async Task ScanVonrabilitiesTask2()
        {
            List<string> packages = new List<string> { };
            foreach (var package in currentVM.machineRPM.packages)
                packages.Add(package.Name);
            List<string> vulnerabilities = await GetPackageVulnerabilitiesAsync2(packages);

            Console.WriteLine("Known vulnerabilities:");
            foreach (string vulnerability in vulnerabilities)
            {
                Console.WriteLine(vulnerability);
            }
        }

        public
        void ScanVonrabilities2()
        {
            Task.Run(async () =>
            {
                await ScanVonrabilitiesTask2();
            }).Wait(); // or use .Result

        }
        private void NextScan(string str, int current, int count)
        {
            toolStripStatusLabel1.Text = "Scanning package " + current.ToString() + " out of " + count.ToString();
            string[] parts = str.Split('/');
            toolStripProgressBar1.Value = current / count;
            scanText.Text += parts[0];
            scanText.Text += Environment.NewLine;
            scanText.SelectionStart = scanText.Text.Length;

            // Scroll to the end
            scanText.ScrollToCaret();
            scanText.Refresh();

            return;
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int packageCount = currentVM.machineRPM.packages.Count;
            int curIdx = 0;
            foreach (var package in currentVM.machineRPM.packages)
            {
                curIdx++;
                this.Invoke(new Action<string, int, int>(NextScan), package.Name, curIdx, packageCount);
                Thread.Sleep(200);
            }
        }
    }
}
