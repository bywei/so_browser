using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace 浏览器
{
    class MyRegedit
    {
        //取ieExplorer属性
        public static string GetIEExplorerValue(string name)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Explorer").OpenSubKey("Shell Folders");
                string result = (string)key.GetValue(name);
                key.Close();
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
