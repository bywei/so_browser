using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace 浏览器
{
    public partial class Form_FavForlder : Form
    {
        public static FavForlder forlder = null;
        public Form_FavForlder()
        {
            InitializeComponent();
            BackColor = Color.FromArgb(216, 229, 250);
        }
        public Form_FavForlder(FavForlder forlder_)
        {
            InitializeComponent();
            BackColor = Color.FromArgb(216, 229, 250);
            forlder = forlder_;
            textBox2.Text = forlder.forlder_name;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Focus();
                return;
            }
            forlder = forlder == null ? Mdb.AddForlder(textBox2.Text) : Mdb.UpdateForlder(forlder.id, textBox2.Text);
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}