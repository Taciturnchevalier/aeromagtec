using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aeromagtec.Controls
{
    public partial class myinputbox : Form
    {
        public myinputbox(string label)
        {
            InitializeComponent();
            label1.Text = label;
        }

        public myinputbox(string label, string title)
        {
            InitializeComponent();
            label1.Text = label;
            this.Text = title;
        }

        private void myinputbox_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            textBox1.Text = "127.0.0.1:5050";
        }

        public string Value { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Value = textBox1.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
