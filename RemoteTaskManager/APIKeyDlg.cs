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
    public partial class APIKeyDlg : Form
    {
        public string APIKey { get; set; } = "";
        public APIKeyDlg()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            APIKey = textBox1.Text;
        }

        private void APIKeyDlg_Load(object sender, EventArgs e)
        {
            textBox1.Text = APIKey;
        }
    }
}
