using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace 浏览器
{
    public class WebBrowserProperty
    {
        public static WebBrowserProperty frontProperty;

        public bool canBack;
        public bool canForward;
        public bool isInClose;
        public string statusText="";
        public string siteTitle = "";
        public AxSHDocVw.AxWebBrowser webBrowser;
        public ToolStripButton toolStripButton;
    }
}
