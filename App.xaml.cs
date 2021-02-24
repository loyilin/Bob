using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Bob
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        //应用加载
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Console.WriteLine("Application_Startup");
        }

        //当激活应用程序中的窗口时发生该事件，当切换到另外一个window程序时也会触发。
        private void Application_Activated(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Application_Activated");
            //MainWindow mw = (MainWindow)Application.Current.MainWindow;
            //Utils.SetWindowToForegroundWithAttachThreadInput(mw);
        }

        //当取消激活应用程序中的窗口时发生该事件，当切换到另外一个window程序时也会触发。
        private void Application_Deactivated(object sender, EventArgs e)
        {
            MainWindow mw = (MainWindow)Application.Current.MainWindow;
            //调用主窗口Test()方法。
            mw.hide_Click();
        }

    }
}
