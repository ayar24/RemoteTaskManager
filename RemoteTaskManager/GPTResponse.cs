using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteTaskManager
{
    public partial class GPTResponse : Form
    {
        public GPTResponse()
        {
            InitializeComponent();
        }

        private void GPTResponse_Load(object sender, EventArgs e)
        {

        }
        private
        void AddChar()
        {
            if (finalResponse == " ")
                finalResponse = "This is the default Response for GPT, if you see this it means that your API keys or connection to openAI api is not working properly";
            richTextBox1.Text = partialText;
            richTextBox1.Refresh();

        }
        string finalResponse = "";
        string partialText = "";
        public void SetGPTResponse(string response)
        {
            finalResponse = response;

            backgroundWorker1.RunWorkerAsync();


        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < finalResponse.Length; i++)
            {
                partialText += finalResponse[i];

                Thread.Sleep(50);
                this.Invoke(new Action(AddChar));
            }
        }
    }
}
