using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Bob
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        //实例化notifyIOC控件最小化托盘
        private NotifyIcon notifyIcon = null;

        private KeyboardHook hook = null;

        /// 注册快捷集合
        readonly Dictionary<string, short> hotKeyDic = new Dictionary<string, short>();


        public MainWindow()
        {
            InitializeComponent();
            initialTray();
            //设置窗口单击长按监听
            window.AddHandler(System.Windows.Controls.Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.button_get_trade_record_MouseLeftButtonDown), false);
            Console.Out.WriteLine("mainWindow");
           
            hook = new KeyboardHook();
            hook.KeyDownEvent += new System.Windows.Forms.KeyEventHandler(hook_KeyDown);//钩住键按下
            hook.Start();
        }

        // 最小化系统托盘
        private void initialTray()
        {
            //隐藏主窗体
            this.Visibility = Visibility.Hidden;
            //设置托盘的各个属性
            notifyIcon = new NotifyIcon();
            //notifyIcon.BalloonTipText = "Bob运行中...";//托盘气泡显示内容
            notifyIcon.Text = "Bob";
            notifyIcon.Visible = true;//托盘按钮是否可见
            //重要提示：此处的图标图片在resouces文件夹。可是打包后安装发现无法获取路径，导致程序死机。建议复制一份resouces文件到UI层的bin目录下，确保万无一失。
            notifyIcon.Icon = new System.Drawing.Icon("./translate.ico");//托盘中显示的图标
            //notifyIcon.ShowBalloonTip(1000);//托盘气泡显示时间
            //双击事件
            //_notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);
            //鼠标点击事件
            //notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);
            //右键菜单--退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("关闭");
            exit.Click += new EventHandler(exit_Click);

            //右键菜单--关于
            System.Windows.Forms.MenuItem abort = new System.Windows.Forms.MenuItem("关于");
            abort.Click += new EventHandler(abort_Click);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { abort, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            //窗体状态改变时触发
            this.StateChanged += MainWindow_StateChanged;
        }


        // 托盘图标鼠标单击事件
        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //鼠标左键，实现窗体最小化隐藏或显示窗体
            if (e.Button == MouseButtons.Left)
            {
                if (this.Visibility == Visibility.Visible)
                {
                    hide_Click();
                }
                else
                {
                    show_Click();
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                //object sender = new object();
                // EventArgs e = new EventArgs();
                hide_Click();//触发单击退出事件
            }
        }

        // 窗体状态改变时候触发
        private void SysTray_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.hide_Click();
            } 
        }


        // 显示窗口
        private void show_Click()
        {
            //this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
            //解决最小化到任务栏可以强行关闭程序的问题。
            //this.ShowInTaskbar = false;//使Form不在任务栏上显示
            //this.Activate();
        }



        // 隐藏窗口
        private void hide_Click()
        {
            this.Visibility = Visibility.Hidden;
            //解决最小化到任务栏可以强行关闭程序的问题。
            //this.ShowInTaskbar = false;//使Form不在任务栏上显示
        }


        // 退出选项
        private void exit_Click(object sender, EventArgs e)
        {
            //退出程序
            notifyIcon.Visible = false;
            hook.Stop();
            System.Environment.Exit(0);
        }



        // 关于选项
        private void abort_Click(object sender, EventArgs e)
        {
            if (System.Windows.MessageBox.Show("确定退出吗?",
                                               "application",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question,
                                                MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                //System.Windows.Application.Current.Shutdown();
                System.Environment.Exit(0);
            }

         
        }


        // 窗口状态改变，最小化托盘
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                hide_Click();
            }
        }

        //鼠标点击不放可以拖动窗口
        private void button_get_trade_record_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }

        //全局键盘监听
        private void hook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == (int)Keys.S && (int)System.Windows.Forms.Control.ModifierKeys == (int)Keys.Alt)
            {
                //逻辑
                Console.Out.WriteLine("alt + s");

            }
            else if (e.KeyValue == (int)Keys.D && (int)System.Windows.Forms.Control.ModifierKeys == (int)Keys.Alt)
            {
                //逻辑
                Console.Out.WriteLine("alt + d");
                show_Click();
            }
        }


        //窗口键盘监听
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                hide_Click();
            }
        }

    }
}
