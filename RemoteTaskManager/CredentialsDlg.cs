using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RemoteTaskManager
{
    public partial class CredentialsDlg : Form
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }

        public CredentialsDlg()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UserName = textBox1.Text;
            Password = textBox2.Text;
            Port = textBox3.Text;
            Close();
        }

        private void CredentialsDlg_Load(object sender, EventArgs e)
        {
            textBox1.Text = UserName;
            textBox2.Text = Password;
            textBox3.Text = Port;
        }
    }
}
