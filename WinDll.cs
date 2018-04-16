using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace 浏览器
{
    class WinDll
    {
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point p);

        [DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hwnd, int flag, int msg1, int msg2);

        [DllImport("Urlmon.dll")]
        public static extern bool CoInternetSetFeatureEnabled(AxSHDocVw.AxWebBrowser webbrowser,int flag,bool enable);
    }
}
