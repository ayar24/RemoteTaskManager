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

    public partial class OpenAISettings : Form
    {
        public string APIKey { get; set; } = "";

        public OpenAISettings()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            APIKey = textBox1.Text;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenAISettings_Load(object sender, EventArgs e)
        {
            textBox1.Text = APIKey;
        }
    }
}
