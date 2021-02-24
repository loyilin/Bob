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
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Configuration;
using Clipboard = System.Windows.Forms.Clipboard;
using System.Threading;
using MessageBox = System.Windows.MessageBox;
using Control = System.Windows.Forms.Control;
using IDataObject = System.Windows.Forms.IDataObject;
using DataFormats = System.Windows.Forms.DataFormats;
using Application = System.Windows.Forms.Application;

namespace Bob
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string question = "";

        //实例化notifyIOC控件最小化托盘
        private NotifyIcon notifyIcon = null;

        private KeyboardHook hook = null;

        /// 注册快捷集合
        readonly Dictionary<string, short> hotKeyDic = new Dictionary<string, short>();

        private Configuration config;


        public MainWindow()
        {
            InitializeComponent();
            initialTray();



            //保证窗体显示在上方。
            this.ShowInTaskbar = false;
            this.Topmost = true;
            IntPtr handle = new WindowInteropHelper(this).Handle;

            config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            updataUI();

            //设置窗口单击长按监听
            window.AddHandler(System.Windows.Controls.Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.button_get_trade_record_MouseLeftButtonDown), false);
           
            hook = new KeyboardHook();
            hook.KeyDownEvent += new System.Windows.Forms.KeyEventHandler(hook_KeyDown);//钩住键按下
            hook.selectTextEvent += selectTextEvent;
            hook.Start();

            outText.SizeChanged += TB_SizeChanged;
        }

        private void selectTextEvent()
        {
            Console.WriteLine("selectTextEvent");

            string old = Clipboard.GetText();
            Console.Out.WriteLine("selectTextEvent: 老数据 = " + old);

            SendKeys.SendWait("^c");
            SendKeys.Flush();
            

            question = Clipboard.GetText();
            Console.Out.WriteLine("selectTextEvent: 新数据 = " + question);

            Clipboard.SetDataObject(old, false, 300, 50);
        }


        //根据内容设置文本框高度
        private void TB_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            outText.Height = outText.ActualHeight;
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
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
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
                //object sender = new object();c
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

        

        // 显示窗口c
        public void show_Click()
        {
            string oldStr = Clipboard.GetText();
            
            //SendKeys.SendWait("^C");

            Console.WriteLine("oldStr = " + oldStr);
            string newStr = Clipboard.GetText();
            Console.WriteLine("newStr = " + newStr);
           // Clipboard.SetDataObject(oldStr, true, 3, 100);
            inputText.Text = newStr;


            Utils.SetWindowToForegroundWithAttachThreadInput(this);

            requst(newStr);
        }



        // 隐藏窗口
        public void hide_Click()
        {
            Console.Out.WriteLine("hide_Click");
            this.Hide();
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

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            show_Click();
        }

        // 窗口状态改变，最小化托盘
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            show_Click();
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
                //show_Click();
                //string text = TryFindEditWindowText();
                Console.WriteLine("alt + d");

                getClipbrardText();


               

            }
            else if (e.KeyValue == (int)Keys.Escape) {
                //逻辑
                Console.Out.WriteLine("Escape");
                hide_Click();
            }
        }

        private void OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Console.WriteLine("鼠标事件：" + e.Clicks + "," + e.Button + "," + e.GetType().Name + "," + e.X + "," + e.Y + "," + e.ToString());

        }


        private void getClipbrardText() {
            /*string old = Clipboard.GetText();
            Console.Out.WriteLine("1111 = " + old);

            SendKeys.SendWait("^c");
            SendKeys.Flush();

            question = Clipboard.GetText();
            Console.Out.WriteLine("2222 = " + question);

            Clipboard.SetDataObject(old , false, 300, 50);*/

            Utils.SendCtrlC(Utils.GetForegroundWindow());
            IDataObject iData = Clipboard.GetDataObject();
            if (null != iData)
            {
                if (iData.GetDataPresent(DataFormats.Text)) //检查是否存在文本
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(500);
                        string res = (String)iData.GetData(DataFormats.Text);
                        if (!string.IsNullOrWhiteSpace(res))
                        {
                            Console.Out.WriteLine(i + "3333 = " + res);
                        }
                    }
                }
            }


        }


        //窗口键盘监听
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Console.Out.WriteLine("MainWindow_KeyDown");
            Console.Out.WriteLine("e.IsDown = " + e.IsDown);
            Console.Out.WriteLine("Keyboard.IsKeyDown = " + Keyboard.IsKeyDown(Key.Escape));
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                hide_Click();
                Console.Out.WriteLine("Key.Escape");
            }
        }

        private void requst(string question) {
            Dictionary<String, String> dic = new Dictionary<String, String>();
            string url = "https://openapi.youdao.com/api";
            string q = question;
            string appKey = "0c8ed167be89eadf";
            string appSecret = "5P11hnmSCyT526d6MYN34sV7RPr4OWld";
            string salt = DateTime.Now.Millisecond.ToString();
            dic.Add("from", "en");
            dic.Add("to", "zh-CHS");
            dic.Add("signType", "v3");
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            string curtime = Convert.ToString(millis / 1000);
            dic.Add("curtime", curtime);
            string signStr = appKey + Utils.Truncate(q) + salt + curtime + appSecret; ;
            string sign = Utils.ComputeHash(signStr, new SHA256CryptoServiceProvider());
            dic.Add("q", Utils.getUTF_8(q));
            dic.Add("appKey", appKey);
            dic.Add("salt", salt);
            dic.Add("sign", sign);
            //dic.Add("vocabId", "您的用户词表ID");
            Post(url, dic);
        }

        

        private void Post(string url, Dictionary<String, String> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            Console.WriteLine(result);
            JObject str = JObject.Parse(result);
            if (str["errorCode"].ToString() == "0")
            {
                Console.WriteLine(str["translation"]);
                setText(str);
            } else
            {
                Console.WriteLine("请求失败了");
            }
        }

        private void setText(JObject str) {
            string text = "";
            if (str["basic"] != null) {
                JToken arr = str["basic"]["explains"];
                foreach (JToken baseJ in arr)//遍历数组
                {
                    string t = baseJ.Value<string>();
                    text += t + "\n";
                }
            }
            

            JToken arrays = str["translation"];
            foreach (JToken baseJ in arrays)//遍历数组
            {
                string t = baseJ.Value<string>();
                text += "\n" + t;
            }
            outText.Text = text;
        }

        private void updataUI() {
            string spread = config.AppSettings.Settings["spread"].Value;
            string fixedd = config.AppSettings.Settings["fixed"].Value;
            string transitionType = config.AppSettings.Settings["transitionType"].Value;
            string path = spread == "0" ? "image\\pack up.png" : "image\\spread.png";
            packIV.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            tt.Visibility = spread == "0" ? Visibility.Collapsed : Visibility.Visible;
            inputBorder.Visibility = spread == "0" ? Visibility.Collapsed : Visibility.Visible;
        }

        private void packIV_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("packIV_PreviewMouseDown");
            //读取
            string spread = config.AppSettings.Settings["spread"].Value;
            config.AppSettings.Settings["spread"].Value = spread == "0" ? "1" : "0";
            config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");

            updataUI();
        }

        private void fixedIV_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("fixedIV_PreviewMouseDown");
        }

        private void navIV_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("navIV_PreviewMouseDown");
        }


        public string TryFindEditWindowText()
        {
            string errorText = "111";
            // 查找标题为Error的窗口
            //IntPtr mainHandle = Utils.FindWindow(null, "记事本.txt - 记事本");
            IntPtr mainHandle = Utils.FindWindow(null, "form1");
            IntPtr maindHwnd = Utils.GetForegroundWindow();
            if (maindHwnd != null)
            {
                Control c = Control.FromHandle(maindHwnd);//这就是
                MessageBox.Show(maindHwnd.ToString());
            }
            int i = 0;
            if (maindHwnd != IntPtr.Zero)
            {
                MessageBox.Show("找到了窗体！");

                char temp1 = new char();
                char temp2 = new char();
                if (temp1.ToString() != "Edit")
                {
                    Utils.SendMessage(maindHwnd, Utils.WM_GETTEXT, 20, temp2);//EDIT的句柄，消息，接收缓冲区大小，接收缓冲区指针

                    MessageBox.Show(temp2.ToString());
                }  
                //找到窗体后这里如何去获取找到的这个窗体上的文本框的值？？？？？？？

                //控件id
                /*int controlId = 0x000003F4;
                //获取子窗口句柄
                IntPtr EdithWnd = Utils.GetDlgItem(maindHwnd, controlId);

                Utils.SendMessage(EdithWnd, i, (IntPtr)0, string.Format("当前时间是:{0}", DateTime.Now)); //赋值没问题，表示句柄正确
                StringBuilder stringBuilder = new StringBuilder(512);
                Utils.GetWindowText(EdithWnd, stringBuilder, stringBuilder.Capacity);
                MessageBox.Show(string.Format("取到的值是：{0}", stringBuilder.ToString()));*///取值一直是空字符串

            }
            else
            {
                MessageBox.Show("没有找到窗口");
            }
            string text = Clipboard.GetText();
            Console.WriteLine("textsssss = " + text);


            return errorText;
           
        }
    }
}
