using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using IWshRuntimeLibrary;

namespace 浏览器
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            IniSetInfo();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_Main(args.Length == 0 ? null : args[0]));
        }

        static void IniSetInfo()
        {
            try
            {
                //设置默认浏览器
                RegistryKey key = Registry.ClassesRoot.OpenSubKey("http").OpenSubKey("shell").OpenSubKey("open");
                if (((string)key.OpenSubKey("command", true).GetValue("")).IndexOf(Application.ExecutablePath)==-1)
                {
                    key.OpenSubKey("command", true).SetValue("", string.Format("\"{0}\" \"%1\"", Application.ExecutablePath));
                    key.OpenSubKey("ddeexec").OpenSubKey("Application", true).SetValue("", "soExplorer");
                    //创建快捷方式
                    string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\so浏览器.lnk";
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.WorkingDirectory = Application.StartupPath;
                    shortcut.WindowStyle = 1;
                    shortcut.Description = "so浏览器";
                    shortcut.IconLocation = Application.StartupPath + @"\logo.ico";
                    shortcut.Save();
                }
                key.Close();
                RegistryKey keyStartPage = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("Main",true);//OpenSubKey("Start Page", true);
                keyStartPage.SetValue("Start Page", "www.so.so");
                keyStartPage.Close();
            }
            catch { }
        }
    }
}