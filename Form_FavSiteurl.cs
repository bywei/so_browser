using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace 浏览器
{
    public partial class Form_FavSiteurl : Form
    {
        private int forlder_id = 0;
        public static FavSiteurl siteurl = null;
        public Form_FavSiteurl()
        {
            InitializeComponent();
        }
        public Form_FavSiteurl(FavSiteurl siteurl_)
        {
            InitializeComponent();
            siteurl = siteurl_;
            textBox2.Text = siteurl.siteurl_name;
            textBox3.Text = siteurl.siteurl_value;
        }
        public Form_FavSiteurl(int forlder_id_, string siteurl_name_, string siteurl_value_)
        {
            InitializeComponent();
            forlder_id = forlder_id_;
            textBox2.Text = siteurl_name_;
            textBox3.Text = siteurl_value_;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Focus();
                return;
            }
            if (textBox3.Text == "")
            {
                textBox3.Focus();
                return;
            }
            siteurl = siteurl == null ? Mdb.AddSiteurl(0, textBox2.Text, textBox3.Text) : Mdb.UpdateSiteurl(siteurl.id, siteurl.forlder_id, textBox2.Text, textBox3.Text);
            DialogResult = DialogResult.OK;
        }
    }
}