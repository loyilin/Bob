using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Bob
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //窗口失去焦点事件
        private void Application_Deactivated(object sender, EventArgs e)
        {
            this.MainWindow.Hide();
        }
    }
}
