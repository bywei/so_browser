using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using SHDocVw;
using Sunisoft.IrisSkin;
using System.Net;


namespace 浏览器
{
    public partial class Form_Main : Form
    {
        //皮肤代码
        SkinEngine skin = new SkinEngine();
        AxSHDocVw.AxWebBrowser currentWebBrowser = null;
        public Form_Main()
        {
            Form_Main_Ini(null);
        }
        public Form_Main(string url_)
        {
            Form_Main_Ini(url_); 
        }
        private void Form_Main_Ini(string url_)
        {
            InitializeComponent();
            toolStrip2_SizeChanged(null, null);
            Show();
            Application.DoEvents();
            AxSHDocVw.AxWebBrowser webBrowser = IniWebBrowser();
            currentWebBrowser = webBrowser;
            if (string.IsNullOrEmpty(url_))
                webBrowser.GoHome();
            else
                webBrowser.Navigate(url_);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripButton9.Image = imageList1.Images[0];
            toolStripButton19.Image = imageList1.Images[0];
            toolStripStatusLabel3.Image = imageList2.Images[1];
            toolStripStatusLabel1.Image = imageList1.Images[0];
            //读取自身收藏夹
            //ReadMyFav();
            //读取皮肤
            skin.SkinFile = Application.StartupPath+@"\skin.ssk";
            Application.DoEvents();
            //设置任务栏位置
            toolStrip3.Dock = Mdb.GetOther("taskbarStyle")=="top" ? DockStyle.Top : DockStyle.Bottom;
            //设置搜索器
            Refresh_Searches();
            //设置收藏夹样式
            if (Mdb.GetOther("collectionStyle") == "1")
            {
                toolStripButton5.Checked = true;
                toolStripButton5_Click(null, null);
            }
            else if (Mdb.GetOther("collectionStyle") == "2")
            {
                历史记录HToolStripMenuItem.Checked = true;
                收藏夹SToolStripMenuItem_Click(null, null);
            }
            if ( Mdb.GetOther("isNormalForm")!="1")
                WindowState = FormWindowState.Maximized;
            else
            {
                Left =int.Parse( Mdb.GetOther("formLeft"));
                Top =int.Parse( Mdb.GetOther("formTop"));
                Width =int.Parse( Mdb.GetOther("formWidth"));
                Height =int.Parse( Mdb.GetOther("formHeight"));
            }
        }

        private void ReadMyFav()
        {
            List<FavForlder> forlders = Mdb.GetFavForlders();
            foreach (FavForlder forlder in forlders)
            {
                ToolStripDropDownButton dropbutton = new ToolStripDropDownButton(forlder.forlder_name, imageList2.Images[0]);
                dropbutton.MouseUp += new MouseEventHandler(dropbutton_MouseUp);
                dropbutton.Tag = forlder;
                toolStrip1.Items.Insert(toolStrip1.Items.Count-2, dropbutton);
                ToolStripMenuItem item = new ToolStripMenuItem("添加到本文件夹", null, new EventHandler(AddFavButton_Click));
                item.Tag = forlder.id;
                dropbutton.DropDownItems.Add(item);
                ToolStripSeparator separator = new ToolStripSeparator();
                dropbutton.DropDownItems.Add(separator);
                List<FavSiteurl> siteurls = Mdb.GetFavSiteurls(forlder.id);
                foreach (FavSiteurl siteurl in siteurls)
                {
                    ToolStripMenuItem button = new ToolStripMenuItem(siteurl.siteurl_name, imageList2.Images[1]);
                    button.Tag=siteurl;
                    button.MouseUp += new MouseEventHandler(button_MouseUp);
                    dropbutton.DropDownItems.Add(button);
                }
            }
            List<FavSiteurl> siteurlsx = Mdb.GetFavSiteurls(0);
            foreach (FavSiteurl siteurl in siteurlsx)
            {
                ToolStripButton button = new ToolStripButton(siteurl.siteurl_name, imageList2.Images[1]);
                button.Tag = siteurl;
                button.MouseUp += new MouseEventHandler(button_MouseUp);
                toolStrip1.Items.Insert(toolStrip1.Items.Count-2,button);
            }
        }
        //添加收藏
        void AddFavButton_Click(object sender, EventArgs e)
        {
            int forlder_id = (int)((ToolStripMenuItem)sender).Tag;
            if (new Form_FavSiteurl(forlder_id, WebBrowserProperty.frontProperty.siteTitle, toolStripComboBox1.Text).ShowDialog() == DialogResult.OK)
            {
                FavSiteurl siteurl = Form_FavSiteurl.siteurl;
                ToolStripMenuItem item = new ToolStripMenuItem(siteurl.siteurl_name, imageList2.Images[1]);
                item.Tag = siteurl;
                item.MouseUp += new MouseEventHandler(button_MouseUp);
                item.Tag = Form_FavSiteurl.siteurl;
                ((ToolStripMenuItem)sender).GetCurrentParent().Items.Add(item);
            }
        }
        //收藏
        void button_MouseUp(object sender, MouseEventArgs e)
        {
            FavSiteurl siteurl = (FavSiteurl)((sender as ToolStripMenuItem) == null ? ((ToolStripButton)sender).Tag : ((ToolStripMenuItem)sender).Tag);
            if(e.Button==MouseButtons.Left)
                WebBrowserProperty.frontProperty.webBrowser.Navigate(siteurl.siteurl_value);
            else if (e.Button == MouseButtons.Right)
            {
                siteurl.tag = sender;
                contextMenuStrip1.Tag = siteurl;
                contextMenuStrip1.Show(Cursor.Position);
            }
        }
        //文件夹
        void dropbutton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FavForlder forlder = (FavForlder)((ToolStripDropDownButton)sender).Tag;
                forlder.tag = sender;
                contextMenuStrip2.Tag = forlder;
                contextMenuStrip2.Show(Cursor.Position);
            }
        }

        private AxSHDocVw.AxWebBrowser IniWebBrowser()
        {
            //初始化浏览器
            AxSHDocVw.AxWebBrowser webBrowser = new AxSHDocVw.AxWebBrowser(); 
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.NewWindow2 += new AxSHDocVw.DWebBrowserEvents2_NewWindow2EventHandler(this.axWebBrowser1_NewWindow2);
            webBrowser.ProgressChange += new AxSHDocVw.DWebBrowserEvents2_ProgressChangeEventHandler(this.axWebBrowser1_ProgressChange);
            webBrowser.StatusTextChange += new AxSHDocVw.DWebBrowserEvents2_StatusTextChangeEventHandler(this.axWebBrowser1_StatusTextChange);
            webBrowser.CommandStateChange += new AxSHDocVw.DWebBrowserEvents2_CommandStateChangeEventHandler(this.axWebBrowser1_CommandStateChange);
            webBrowser.TitleChange += new AxSHDocVw.DWebBrowserEvents2_TitleChangeEventHandler(this.axWebBrowser1_TitleChange);
            webBrowser.SetSecureLockIcon += new AxSHDocVw.DWebBrowserEvents2_SetSecureLockIconEventHandler(this.axWebBrowser1_SetSecureLockIcon);
            webBrowser.WindowClosing += new AxSHDocVw.DWebBrowserEvents2_WindowClosingEventHandler(webBrowser_WindowClosing);
            webBrowser.BeforeNavigate2 += new AxSHDocVw.DWebBrowserEvents2_BeforeNavigate2EventHandler(webBrowser_BeforeNavigate2);
            panel2.Controls.Add(webBrowser);
            Application.DoEvents();
            webBrowser.Silent = true;
            //初始化标签页
            ToolStripButton toolStripButton = new ToolStripButton("新标签页        ",imageList1.Images[0]);
            toolStripButton.AutoSize = false;
            toolStripButton.Width = 200;
            toolStripButton.MouseUp += new MouseEventHandler(toolStripButton_MouseUp);
            toolStripButton.Margin = new Padding(0, 0, 5,0);
            //toolStripButton.BackColor = Color.FromArgb(216, 229, 250);
            toolStripButton.BackColor = Color.FromArgb(254, 254, 255);
            toolStripButton.Paint += new PaintEventHandler(toolStripButton_Paint);
            toolStripButton.MouseMove += new MouseEventHandler(toolStripButton_MouseMove);
            toolStripButton.MouseLeave += new EventHandler(toolStripButton_MouseLeave);
            toolStrip3.Items.Insert(toolStrip3.Items.Count-2, toolStripButton);
            //定义对应属性
            WebBrowserProperty property = new WebBrowserProperty();
            property.toolStripButton = toolStripButton;
            property.webBrowser = webBrowser;
            webBrowser.Tag = property;
            toolStripButton.Tag = property;
            //设为当前焦点
            toolStripButton_MouseUp(toolStripButton, new MouseEventArgs(MouseButtons.Left,-1,0,0,0));
            return webBrowser;
        }

        void toolStripButton_MouseLeave(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton=(ToolStripButton)sender;
            WebBrowserProperty property = (WebBrowserProperty)(toolStripButton).Tag;
            property.isInClose = false;
            toolStrip3.Refresh();
        }

        void toolStripButton_MouseMove(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            WebBrowserProperty property = (WebBrowserProperty)(toolStripButton).Tag;
            if (e.X > toolStripButton.Size.Width - closeLeft ^ property.isInClose)
            {
                property.isInClose = !property.isInClose;
                toolStrip3.Refresh();
            }
        }

        int closeWidth = 10,closeLeft = 14,closeTop=6;
        void toolStripButton_Paint(object sender, PaintEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            toolStripButton.TextAlign = ContentAlignment.MiddleLeft;
            WebBrowserProperty property = (WebBrowserProperty)(toolStripButton).Tag;
            Graphics g = e.Graphics;
            if (property.isInClose)
            {
                g.FillEllipse(Brushes.DarkRed, new Rectangle(toolStripButton.Size.Width - closeLeft, closeTop, closeWidth, closeWidth));
            }
            g.DrawLine(property.isInClose ? new Pen(Color.White, 2) : new Pen(Color.Gray, 2), new Point(toolStripButton.Size.Width - closeLeft + 2, closeTop + 2), new Point(toolStripButton.Size.Width - closeLeft+closeWidth-2, closeWidth+closeTop-2));
            g.DrawLine(property.isInClose ? new Pen(Color.White, 2) : new Pen(Color.Gray, 2), new Point(toolStripButton.Size.Width - closeLeft + 2, closeWidth + closeTop - 2), new Point(toolStripButton.Size.Width - closeLeft + closeWidth - 2, closeTop + 2));
        }

        void webBrowser_BeforeNavigate2(object sender, AxSHDocVw.DWebBrowserEvents2_BeforeNavigate2Event e)
        {
            e.flags = toolStripButton12.Checked?1:0;
        }

        void webBrowser_WindowClosing(object sender, AxSHDocVw.DWebBrowserEvents2_WindowClosingEvent e)
        {
            e.cancel = true;
            if (!toolStripStatusLabel1.Text.StartsWith("SetSkin:"))
                toolStripButton18_Click(null, null);
            else
            {
                try
                {
                    new WebClient().DownloadFile(toolStripStatusLabel1.Text.Substring(8), Application.StartupPath + @"\skin.ssk");
                    skin.Active = true;
                    skin.SkinFile = Application.StartupPath + @"\skin.ssk";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        void toolStripButton_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            if (toolStripButton == toolStripButton9)
            {
                AxSHDocVw.AxWebBrowser webBrowser = IniWebBrowser();
                webBrowser.GoHome();
                currentWebBrowser = webBrowser;
            }
            else if (e.Button == MouseButtons.Right)
            {
                WebBrowserProperty property = (WebBrowserProperty)((ToolStripButton)sender).Tag;
                Controls.Remove(property.webBrowser);
                property.webBrowser.Dispose();
                toolStrip3.Items.Remove(property.toolStripButton);
                Application.DoEvents();
                Control webBrowser= Control.FromChildHandle(WinDll.WindowFromPoint(PointToScreen(new Point(panel2.Location.X + 10, panel2.Location.Y + 10))));
                if(webBrowser.ToString()=="AxSHDocVw.AxWebBrowser")
                    toolStripButton_MouseUp(((WebBrowserProperty)webBrowser.Tag).toolStripButton, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                else
                    toolStripButton_MouseUp(toolStripButton9, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (e.X > toolStripButton.Size.Width - closeLeft)
                    toolStripButton_MouseUp(sender, new MouseEventArgs(MouseButtons.Right, 0, 0, 0, 0));
                else
                {
                    WebBrowserProperty property = (WebBrowserProperty)(toolStripButton).Tag;
                    WebBrowserProperty.frontProperty = property;
                    foreach (ToolStripButton tsb in toolStrip3.Items)
                        tsb.Checked = false;
                    ((ToolStripButton)sender).Checked = true;
                    //设置标题和地址
                    if (e.Clicks != -1)
                    {
                        Text = WebBrowserProperty.frontProperty.siteTitle + " - so浏览器 2011 正式版";
                        toolStripComboBox1.Text = WebBrowserProperty.frontProperty.webBrowser.LocationURL;
                        toolStripButton1.Enabled = 后退BToolStripMenuItem.Enabled = WebBrowserProperty.frontProperty.canBack;
                        toolStripButton2.Enabled = 前进FToolStripMenuItem.Enabled = WebBrowserProperty.frontProperty.canForward;
                        toolStripStatusLabel1.Text = WebBrowserProperty.frontProperty.statusText;
                    }
                    //设置前端显示
                    WebBrowserProperty.frontProperty.webBrowser.BringToFront();
                }
            }
        }

        private void axWebBrowser1_NewWindow2(object sender, AxSHDocVw.DWebBrowserEvents2_NewWindow2Event e)
        {
            e.ppDisp = IniWebBrowser().Application;
        }

        private void toolStrip2_SizeChanged(object sender, EventArgs e)
        {
            toolStripComboBox1.Size = new Size(toolStrip2.Width - 530, 25);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            try
            {
                WebBrowserProperty.frontProperty.webBrowser.Navigate(toolStripComboBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void axWebBrowser1_TitleChange(object sender, AxSHDocVw.DWebBrowserEvents2_TitleChangeEvent e)
        {
            AxSHDocVw.AxWebBrowser webBrowser = (AxSHDocVw.AxWebBrowser)sender; 
            WebBrowserProperty property = (WebBrowserProperty)webBrowser.Tag;
            property.siteTitle = e.text;
            property.toolStripButton.Text =GetSubstring(e.text,25);
            property.toolStripButton.ToolTipText = e.text;
            if (webBrowser == WebBrowserProperty.frontProperty.webBrowser)
            {
                Text = e.text + " - so浏览器 2011 正式版";
                toolStripComboBox1.Text = webBrowser.LocationURL;
            }
        }

        private void axWebBrowser1_ProgressChange(object sender, AxSHDocVw.DWebBrowserEvents2_ProgressChangeEvent e)
        {
            toolStripProgressBar1.Value = e.progress * 100 / e.progressMax;
        }

        private void axWebBrowser1_StatusTextChange(object sender, AxSHDocVw.DWebBrowserEvents2_StatusTextChangeEvent e)
        {
            AxSHDocVw.AxWebBrowser webBrowser = (AxSHDocVw.AxWebBrowser)sender;
            WebBrowserProperty property = (WebBrowserProperty)webBrowser.Tag;
            property.statusText = e.text;
            if (webBrowser == WebBrowserProperty.frontProperty.webBrowser)
                toolStripStatusLabel1.Text = e.text;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.GoBack();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.GoForward();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Refresh();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.GoHome();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //记住窗体位置
            Mdb.SetOther("isNormalForm", WindowState == FormWindowState.Normal ? "1" : "0");
            if (WindowState == FormWindowState.Normal)
            {
                Mdb.SetOther("formLeft",Left.ToString());
                Mdb.SetOther("formTop", Top.ToString());
                Mdb.SetOther("formWidth", Width.ToString());
                Mdb.SetOther("formHeight", Height.ToString());
            }
        }

        private void toolStripComboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                toolStripButton7_Click(null, null);
        }

        private void 关于AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form_About().ShowDialog();
        }

        private void 关闭窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 新建窗口NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form_Main(toolStripComboBox1.Text).Show();
        }

        private void axWebBrowser1_CommandStateChange(object sender, AxSHDocVw.DWebBrowserEvents2_CommandStateChangeEvent e)
        {
            AxSHDocVw.AxWebBrowser webBrowser = (AxSHDocVw.AxWebBrowser)sender;
            WebBrowserProperty property =(WebBrowserProperty) webBrowser.Tag;
            switch ((CommandStateChangeConstants)e.command)
            {
                case CommandStateChangeConstants.CSC_NAVIGATEBACK:
                    property.canBack = e.enable;
                    if(webBrowser==WebBrowserProperty.frontProperty.webBrowser)
                        toolStripButton1.Enabled = 后退BToolStripMenuItem.Enabled = e.enable;
                    break;
                case CommandStateChangeConstants.CSC_NAVIGATEFORWARD:
                    property.canForward = e.enable;
                    if (webBrowser == WebBrowserProperty.frontProperty.webBrowser)
                        toolStripButton2.Enabled = 前进FToolStripMenuItem.Enabled = e.enable;
                    break;
            }
        }

        private void axWebBrowser1_SetSecureLockIcon(object sender, AxSHDocVw.DWebBrowserEvents2_SetSecureLockIconEvent e)
        {
            toolStripStatusLabel2.Visible = e.secureLockIcon == 6;
        }

        private void internet选项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("rundll32.exe", "shell32.dll, Control_RunDLL Inetcpl.cpl, 0");
        }
        #region 查看源文件
        [ComImport, Guid("b722bccb-4e68-101b-a2bc-00aa00404770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleCommandTarget
        {
            void QueryStatus(ref Guid pguidCmdGroup, UInt32 cCmds,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] OLECMD[] prgCmds, ref OLECMDTEXT CmdText);
            void Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdExecOpt, ref object pvaIn, ref object pvaOut);

        }
        public struct OLECMDTEXT
        {
            public uint cmdtextf;
            public uint cwActual;
            public uint cwBuf;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public char rgwz;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct OLECMD
        {
            public uint cmdID;
            public uint cmdf;
        }
        public void ViewSource()
        {
            try
            {
                IOleCommandTarget cmdt = (IOleCommandTarget)WebBrowserProperty.frontProperty.webBrowser.Document;
                Object o = new object();
                Guid guid = new Guid("ED016940-BD5B-11CF-BA4E-00C04FD70816");
                cmdt.Exec(ref guid, (uint)2, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #endregion
       
        private void 查找FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Focus();
            SendKeys.Send("^f");
        }

        private void 查看源文件CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewSource();
        }

        private void ReadIeFavorites()
        {
            string favoritesPath = MyRegedit.GetIEExplorerValue("Favorites");
            string[] directories= Directory.GetDirectories(favoritesPath);
            foreach (string directory in directories)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(directory.Substring(favoritesPath.Length + 1), imageList1.Images[1]);
                item.Tag = "folder";
                收藏BToolStripMenuItem.DropDownItems.Add(item);
                string[] files_in = Directory.GetFiles(directory);
                foreach (string file in files_in) 
                    if (file.EndsWith(".url"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string text = GetSubstring(Path.GetFileNameWithoutExtension(fileName), 40);
                        item.DropDownItems.Add(text, imageList1.Images[2],new EventHandler(ToolStripMenuItem_Click)).Tag = file;
                    }
            }
            string[] files_out = Directory.GetFiles(favoritesPath);
            foreach (string file in files_out)
                if (file.EndsWith(".url"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string text = GetSubstring(Path.GetFileNameWithoutExtension(fileName), 40);
                    收藏BToolStripMenuItem.DropDownItems.Add(text, imageList1.Images[2], new EventHandler(ToolStripMenuItem_Click)).Tag = file;
                }
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFavorite((string)((ToolStripMenuItem)sender).Tag);
        }
        private void ToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFavorite((string)((ToolStripButton)sender).Tag);
        }

        private void 收藏夹SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = splitter1.Visible = 收藏夹SToolStripMenuItem.Checked;
            Mdb.SetOther("collectionStyle", 收藏夹SToolStripMenuItem.Checked ? "1" : "0");
            if (收藏夹SToolStripMenuItem.Checked)
            {
                收藏BToolStripMenuItem_DropDownOpening(null, null);
                Application.DoEvents();
                历史记录HToolStripMenuItem.Checked = false;
                panel1.Width = int.Parse(Mdb.GetOther("collectionWidth"));
                if (treeView1.Nodes.Count == 0)
                {
                    for (int i = 3; i < 收藏BToolStripMenuItem.DropDownItems.Count; i++)
                    {
                        string tag = (string)收藏BToolStripMenuItem.DropDownItems[i].Tag;
                        if (tag != "folder")
                        {
                            TreeNode node = new TreeNode(收藏BToolStripMenuItem.DropDownItems[i].Text, 2, 2);
                            node.Tag = tag;
                            treeView1.Nodes.Add(node);
                        }
                        else
                        {
                            TreeNode nodex = new TreeNode(收藏BToolStripMenuItem.DropDownItems[i].Text, 1, 1);
                            nodex.Tag = tag;
                            treeView1.Nodes.Add(nodex);
                            foreach (ToolStripMenuItem item in ((ToolStripMenuItem)收藏BToolStripMenuItem.DropDownItems[i]).DropDownItems)
                            {
                                TreeNode nodey = new TreeNode(item.Text, 2, 2);
                                nodey.Tag = item.Tag;
                                nodex.Nodes.Add(nodey);
                            }
                        }
                    }
                }
                treeView1.BringToFront();
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            收藏夹SToolStripMenuItem.Checked = !收藏夹SToolStripMenuItem.Checked;
            收藏夹SToolStripMenuItem_Click(null, null);
        }

        private void 历史记录HToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = splitter1.Visible = 历史记录HToolStripMenuItem.Checked;
            Mdb.SetOther("collectionStyle", 历史记录HToolStripMenuItem.Checked ? "2" : "0");
            if (历史记录HToolStripMenuItem.Checked)
            {
                收藏夹SToolStripMenuItem.Checked = toolStripButton5.Checked = false;
                panel1.Width = int.Parse(Mdb.GetOther("collectionWidth"));
                if (treeView2.Nodes.Count == 0)
                {
                    UrlHistoryWrapperClass.STATURLEnumerator enumerator = new UrlHistoryWrapperClass().GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (!string.IsNullOrEmpty(enumerator.Current.Title))
                        {
                            TreeNode node = new TreeNode(enumerator.Current.Title, 2, 2);
                            node.Tag = enumerator.Current.URL;
                            treeView2.Nodes.Add(node);
                        }
                    }
                }
                treeView2.BringToFront();
            }
        }

        private string GetSubstring(string text,int length)
        {
            byte[] bytes = Encoding.Default.GetBytes(text);
            if (bytes.Length > length)
                return Encoding.Default.GetString(bytes, 0, length) + "...";
            else
                return Encoding.Default.GetString(bytes, 0, bytes.Length)+"  ";
        }

        private void 添加到收藏夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShellUIHelper ui = new ShellUIHelper();
            object title = WebBrowserProperty.frontProperty.siteTitle;
            ui.AddFavorite(WebBrowserProperty.frontProperty.webBrowser.LocationURL, ref title);
        }

        private void 整理收藏夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShellUIHelper ui = new ShellUIHelper();
            object path=null;
            ui.ShowBrowserUI("OrganizeFavorites", ref path);
        }

        private void 另存为AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_SAVEAS, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 剪切TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_CUT, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 复制CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_COPY, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 粘贴PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_PASTE, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 全选AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_SELECTALL, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 属性RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_PROPERTIES, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "HTML文件|*.htm;*.html|所有文件|*.*";
            if (of.ShowDialog() == DialogResult.OK)
                WebBrowserProperty.frontProperty.webBrowser.Navigate(of.FileName);
        }

        private void 保存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_SAVE, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 页面设置UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_PAGESETUP, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 打印PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_PRINT, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void 打印预览VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.ExecWB(OLECMDID.OLECMDID_PRINTPREVIEW, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            查找FToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            toolStripButton_MouseUp(WebBrowserProperty.frontProperty.toolStripButton, new MouseEventArgs(MouseButtons.Right,1,0,0,0));
        }

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            toolStrip3.Dock = toolStrip3.Dock == DockStyle.Bottom ? DockStyle.Top : DockStyle.Bottom;
            Mdb.SetOther("taskbarStyle", toolStrip3.Dock == DockStyle.Top ? "top" : "bottom");
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if ((TreeView)sender == treeView2)
                    WebBrowserProperty.frontProperty.webBrowser.Navigate((string)e.Node.Tag);
                else
                    OpenFavorite((string)e.Node.Tag);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OpenFavorite(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                while (!sr.EndOfStream)
                {
                    string text = sr.ReadLine();
                    if (text.StartsWith("URL="))
                    {
                        WebBrowserProperty.frontProperty.webBrowser.Navigate(text.Substring(4));
                        break;
                    }
                }
                sr.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Mdb.SetOther("collectionWidth", panel1.Width.ToString());
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Stop();
        }

        private void 联机支持ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate("http://support.microsoft.com/?ln=zh-cn");
        }

        private void 发送意见反馈KToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate("http://www.microsoft.com/china/windows/products/winfamily/ie/iefaq.mspx");
        }

        private void 后退BToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.GoBack();
        }

        private void 前进FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.GoForward();
        }

        private void 主页HToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.GoHome();
        }

        private void 停止PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Stop();
        }

        private void 刷新RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Refresh();
        }

        private void 全屏显示FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost= menuStrip1.Visible =  toolStrip2.Visible = statusStrip1.Visible = !全屏显示FToolStripMenuItem.Checked;
            FormBorderStyle = TopMost ?  FormBorderStyle.Sizable:FormBorderStyle.None;
        }

        private void 检查更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate("http://so.so.so/setup/");
        }

        private void 皮肤还原RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.Delete(Application.StartupPath + @"\skin.ssk");
            skin.Active = false;
            Refresh();
        }

        private void 浏览皮肤LToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate("http://so.so.so/skins.aspx");
        }

        private void 载入皮肤LToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "皮肤文件|*.ssk";
            if (of.ShowDialog() == DialogResult.OK)
            {
                File.Copy(of.FileName, Application.StartupPath + @"\skin.ssk", true);
                skin.SkinFile = Application.StartupPath + @"\skin.ssk";
                skin.Active = true ;
                Refresh();
            }
        }

        Thread th_Suggestion;
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            comboBoxOldText = comboBox1.Text;
        }
        string comboBoxOldText = "";
        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                toolStripButton13_Click(null, null);
            else if (comboBox1.Text == "")
                comboBox1.Height = 24;
            else if (comboBox1.Text != comboBoxOldText && comboBox1.SelectedIndex == -1)
            {
                th_Suggestion = new Thread(new ParameterizedThreadStart(GetSuggestion_百度));
                th_Suggestion.Start(comboBox1.Text);
            }
        }

        public delegate void SuggestionResult_(string[] suggestions, bool isThread);
        public void SuggestionResult(string[] suggestions, bool isThread)
        {
            if (isThread)
                Invoke(new SuggestionResult_(SuggestionResult), new object[] { suggestions, false });
            else if (suggestions.Length == 0)
                comboBox1.Height = 24;
            else
            {
                int x=comboBox1.SelectionStart;
                comboBox1.Items.Clear();
                foreach (string suggestion in suggestions)
                    comboBox1.Items.Add(suggestion);
                comboBox1.Height = 28 + suggestions.Length * 16;
                comboBox1.SelectionStart = x;
                comboBox1.SelectionLength = 0;
            }
        }
        public void GetSuggestion_百度(object inputo)
        {
            try
            {
                string input = (string)inputo;
                string result = new WebClient().DownloadString(string.Format("http://suggestion.baidu.com/su?wd={0}&p=3&cb=window.bdsug.sug&t=100", input));
                SuggestionResult(MyRegex.GetRegValue(@"\[.+\]", result.Replace("\"", "")).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), true);
            }
            catch { }
        }

        private void comboBox1_Leave(object sender, EventArgs e)
        {
            comboBox1.Height = 24;
        }

        private void Change_SearchShow(ToolStripMenuItem item)
        {
            toolStripDropDownButton2.Image = item.Image;
            toolStripDropDownButton2.Text = item.Text;
            toolStripDropDownButton2.Tag = ((string)item.Tag).Split(new string[] { "</>" }, StringSplitOptions.None)[2];
            Mdb.SetOther("searchInfo",(string) item.Tag);
            comboBox1.Focus();
            comboBox1.SelectAll();
        }

        private void Refresh_Searches()
        {
            List<object[]> searches = Mdb.GetSearches();
            for (int i = toolStripDropDownButton2.DropDownItems.Count - 3; i >= 0; i--)
                toolStripDropDownButton2.DropDownItems.RemoveAt(i);
            foreach (object[] search in searches)
            {
                string imgPath = search[1].ToString() == "" ? "" : ((((string)search[1])[1] == ':' ? "" : Application.StartupPath + @"\") + (string)search[1]);
                ToolStripMenuItem item = new ToolStripMenuItem((string)search[0],File.Exists( imgPath)?Image.FromFile(imgPath):imageList2.Images[1]);
                item.Tag = string.Format("{0}</>{1}</>{2}", search[0], search[1], search[2]);
                item.Click += new EventHandler(item_Click);
                toolStripDropDownButton2.DropDownItems.Insert(toolStripDropDownButton2.DropDownItems.Count-2, item);
            }
            string[] searchInfos = Mdb.GetOther("searchInfo").Split(new string[]{"</>"},StringSplitOptions.RemoveEmptyEntries);
            toolStripDropDownButton2.Text = searchInfos[0];
            toolStripDropDownButton2.Image = File.Exists(searchInfos[1]) ? Image.FromFile(searchInfos[1]) : imageList2.Images[1];
            toolStripDropDownButton2.Tag = searchInfos[2];
        }
        void item_Click(object sender, EventArgs e)
        {
            Change_SearchShow((ToolStripMenuItem)sender);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate("http://www.so.so/试用版暂不能访问");
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate("http://www.7086.so");
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            comboBox1.Height = 24;
            if (toolStripDropDownButton2.Tag == null)
                MessageBox.Show("请添加搜索器！", "提示");
            else
                WebBrowserProperty.frontProperty.webBrowser.Navigate(string.Format((string)toolStripDropDownButton2.Tag, comboBox1.Text));
            try
            {
                th_Suggestion.Abort();
            }
            catch { }
        }

        private void 搜索器管理ZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (new Form_Search().ShowDialog() == DialogResult.OK)
                Refresh_Searches();
        }

        private void 收藏BToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if(收藏BToolStripMenuItem.DropDownItems.Count==3)
                ReadIeFavorites();
        }

        private void 打开ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Navigate(((FavSiteurl)contextMenuStrip1.Tag).siteurl_value);
        }

        private void 删除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                FavSiteurl siteurl=(FavSiteurl)contextMenuStrip1.Tag;
                Mdb.DeleteSiteurl(siteurl.id);
                if((siteurl.tag as ToolStripMenuItem) == null)
                {
                    ToolStripButton button =(ToolStripButton)siteurl.tag;
                    button.GetCurrentParent().Items.Remove(button);
                }
                else
                {
                    ToolStripMenuItem item=(ToolStripMenuItem)siteurl.tag;
                    item.GetCurrentParent().Items.Remove(item);
                }
            }
        }

        private void 编辑EToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FavSiteurl siteurl = (FavSiteurl)contextMenuStrip1.Tag;
            if (new Form_FavSiteurl(siteurl).ShowDialog() == DialogResult.OK)
            {
                if ((siteurl.tag as ToolStripMenuItem) == null)
                {
                    ToolStripButton button = (ToolStripButton)siteurl.tag;
                    button.Text = Form_FavSiteurl.siteurl.siteurl_name;
                    button.Tag = Form_FavSiteurl.siteurl;
                }
                else
                {
                    ToolStripMenuItem item = (ToolStripMenuItem)siteurl.tag;
                    item.Text = Form_FavSiteurl.siteurl.siteurl_name;
                    item.Tag = Form_FavSiteurl.siteurl;
                }
            }
        }

        private void 复制网址ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(((FavSiteurl)contextMenuStrip1.Tag).siteurl_value);
        }

        private void 替换为当前网址SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FavSiteurl siteurl = (FavSiteurl)contextMenuStrip1.Tag;
            siteurl = new FavSiteurl(siteurl.id, siteurl.forlder_id, siteurl.siteurl_name, toolStripComboBox1.Text);
            new Form_FavSiteurl(siteurl).ShowDialog();
        }

        private void 删除DToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FavForlder forlder=(FavForlder)contextMenuStrip2.Tag;
            if (MessageBox.Show("确定要删除吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                Mdb.DeleteForlder(forlder.id);
                toolStrip1.Items.Remove((ToolStripDropDownButton)forlder.tag);
            }
        }

        private void 编辑EToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FavForlder forlder = (FavForlder)contextMenuStrip2.Tag;
            if (new Form_FavForlder(forlder).ShowDialog() == DialogResult.OK)
            {
                ((ToolStripDropDownButton)forlder.tag).Text = Form_FavForlder.forlder.forlder_name;
                ((ToolStripDropDownButton)forlder.tag).Tag = Form_FavForlder.forlder;
            }
        }

        private void 新建收藏NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (new Form_FavSiteurl(0, WebBrowserProperty.frontProperty.siteTitle, WebBrowserProperty.frontProperty.webBrowser.LocationURL).ShowDialog() == DialogResult.OK)
            {
                FavSiteurl siteurl = Form_FavSiteurl.siteurl;
                ToolStripButton button = new ToolStripButton(siteurl.siteurl_name, imageList2.Images[1]);
                button.Tag = siteurl;
                button.MouseUp += new MouseEventHandler(button_MouseUp);
                toolStrip1.Items.Insert(toolStrip1.Items.Count-2, button);
            }
        }

        private void 新建文件夹MToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (new Form_FavForlder().ShowDialog() == DialogResult.OK)
            {
                FavForlder forlder=Form_FavForlder.forlder;
                ToolStripDropDownButton dropbutton = new ToolStripDropDownButton(forlder.forlder_name, imageList2.Images[0]);
                dropbutton.Tag = forlder;
                dropbutton.MouseUp+=new MouseEventHandler(dropbutton_MouseUp);
                ToolStripMenuItem item = new ToolStripMenuItem("添加到本文件夹", null, new EventHandler(AddFavButton_Click));
                dropbutton.DropDownItems.Add(item);
                item.Tag = forlder.id;
                ToolStripSeparator separator = new ToolStripSeparator();
                dropbutton.DropDownItems.Add(separator);
                toolStrip1.Items.Insert(13, dropbutton);
            }
        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {
            Process.Start("IExplore.exe", toolStripComboBox1.Text);
        }

        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {
            WebBrowserProperty.frontProperty.webBrowser.Stop();
        }

        private void toolStripDropDownButton3_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string text = e.ClickedItem.Text;
            Object obj = new Object();
            obj = 3;
            this.currentWebBrowser.ExecWB(OLECMDID.OLECMDID_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,ref obj,ref obj);
        }
    }
}
