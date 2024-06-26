﻿using System;
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
        public string SysMsg { get; set; } = "";
        public string Prompt { get; set; } = "";

        public OpenAISettings()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            APIKey = textBox1.Text;
            SysMsg = textBox2.Text;
            Prompt = textBox3.Text;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenAISettings_Load(object sender, EventArgs e)
        {
            textBox1.Text = APIKey;
            textBox2.Text = SysMsg;
            textBox3.Text = Prompt;
        }
    }
}
