using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace JToolBox
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private static NotifyIcon notifyIcon;
        private bool IsTPTS = false;
        private bool IsEnableTracker = false;
        private void CheckBoxTPTS_Checked(object sender, RoutedEventArgs e)
        {
            IsTPTS = TPTS.IsChecked.GetValueOrDefault();
            if(IsTPTS)
            {
                OMGtotal.Show("提示","托盘提示已经开启","success.png");
            }
            else
            {
                OMGtotal.Show("提示", "托盘提示已经关闭", "disable.png");
            }
        }
        private class ApiData
        {
            public class Record
            {
                public int watchdog_lastMinute { get; set; }
                public int staff_rollingDaily { get; set; }
                public int watchdog_total { get; set; }
                public int watchdog_rollingDaily { get; set; }
                public int staff_total { get; set; }
            }

            public bool success { get; set; }
            public Record record { get; set; }
        }

        private static int previousWatchdogTotal = -1;
        private static int previousStaffTotal = -1;
        private async void BanTrackerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableTracker)
            {
                IsEnableTracker = false;
                BanTrackerBtn.Content = "开启";
                LRichTextBox.Document.Blocks.Clear();
            }
            else
            {
                Paragraph paragraph = new Paragraph(new Run("Hypixel Ban Tracker started")
                {
                    Foreground = System.Windows.Media.Brushes.Green,
                    FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei UI"),
                    FontSize = 14
                });;
                LRichTextBox.Document.Blocks.Add(paragraph);
                IsEnableTracker = true;
                BanTrackerBtn.Content = "关闭";
                bool notificationShown = false;

                while (IsEnableTracker)
                {
                    using (var httpClientHandler = new HttpClientHandler())
                    {
                        // 忽略 SSL 证书验证
                        // ...

                        using (var httpClient = new HttpClient(httpClientHandler))
                        {
                            // 设置全局请求头
                            httpClient.DefaultRequestHeaders.Add("User-Agent", "Bro why are you want play Hypixel/2.0");

                            try
                            {
                                // 向 API 发送请求
                                HttpResponseMessage response = await httpClient.GetAsync("https://api.plancke.io/hypixel/v1/punishmentStats");
                                notificationShown = false;
                                // 检查是否成功
                                if (response.IsSuccessStatusCode)
                                {
                                    // 将响应内容反序列化为 ApiData 对象
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    ApiData data = JsonConvert.DeserializeObject<ApiData>(responseContent);

                                    // 获取当前时间，转换为[时(24小时制):分:秒]的格式
                                    string timestamp = DateTime.Now.ToString("HH:mm:ss");

                                    // 如果不是第一次获取，判断是否有增加
                                    if (previousWatchdogTotal != -1 && previousStaffTotal != -1)
                                    {
                                        int watchdogIncrease = data.record.watchdog_total - previousWatchdogTotal;
                                        int staffIncrease = data.record.staff_total - previousStaffTotal;

                                        // 输出带时间戳的信息
                                        if (watchdogIncrease > 0 && !notificationShown)
                                        {
                                            AudioPlayer.PlayEmbeddedResourceAsync("src.getban.MP3");
                                            if (IsTPTS)
                                            {
                                                if (watchdogIncrease >= 5)
                                                {
                                                    OMGtotal.Show("A Player Banned", $"🐕Watchdog banned {watchdogIncrease} player(s) in last {delaytime}s.Insance!\U0001f921", "ban.png");
                                                }
                                                else
                                                {
                                                    OMGtotal.Show("A Player Banned", $"🐕Watchdog banned {watchdogIncrease} player(s) in last {delaytime}s.", "ban.png");
                                                }
                                            }
                                            if (watchdogIncrease >= 5)
                                            {
                                                AddBlackText($"[{timestamp}] 🐕Watchdog banned {watchdogIncrease} player(s) in last {delaytime}s.Insance!\U0001f921");
                                            }
                                            else
                                            {
                                                AddBlackText($"[{timestamp}] 🐕Watchdog banned {watchdogIncrease} player(s) in last {delaytime}s.");
                                            }

                                            // 设置通知已显示标志
                                            notificationShown = true;
                                        }

                                        if (staffIncrease > 0 && !notificationShown)
                                        {
                                            AudioPlayer.PlayEmbeddedResourceAsync("src.getban.MP3");
                                            if (IsTPTS)
                                            {
                                                if (watchdogIncrease >= 5)
                                                {

                                                    OMGtotal.Show("A Player Banned", $"👮Staff banned {staffIncrease} player(s) in last {delaytime}s.Insance!\U0001f921", "ban.png");
                                                }
                                                else
                                                {
                                                    OMGtotal.Show("A Player Banned", $"👮Staff banned {staffIncrease} player(s) in last {delaytime}s.", "ban.png");
                                                }
                                            }
                                            if (staffIncrease >= 5)
                                            {
                                                AddBlackText($"[{timestamp}] 👮Staff banned {staffIncrease} player(s) in last {delaytime}s.Insance!\U0001f921");
                                            }
                                            else
                                            {
                                                AddBlackText($"[{timestamp}] 👮Staff banned {staffIncrease} player(s) in last {delaytime}s.");
                                            }

                                            // 设置通知已显示标志
                                            notificationShown = true;
                                        }
                                    }

                                    // 更新记录
                                    previousWatchdogTotal = data.record.watchdog_total;
                                    previousStaffTotal = data.record.staff_total;
                                }
                                else
                                {
                                    AddRedText($"Failed to retrieve data from the API. Status code: {response.StatusCode}");
                                }
                            }
                            catch (Exception ex)
                            {
                                AddRedText($"An error occurred: {ex.Message}");
                            }
                        }
                    }
                    int.TryParse(delayTextBox.Text, out int delayValue);
                    // 等待10秒

                    delaytime = delayValue / 1000.0;
                    await Task.Delay(delayValue);
                }
            }
            
        }
        private static void ShowInfoNotification(string content)
        {
            // 创建托盘图标
            notifyIcon = new NotifyIcon();

            // 设置托盘图标为项目资源中的 BA.jpg
            string resourceName = "JToolBox.src.BA.ico"; // 替换 YourNamespace 为你的项目命名空间
            using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (iconStream != null)
                {
                    notifyIcon.Icon = new System.Drawing.Icon(iconStream);
                }
                else
                {
                    // 处理资源未找到的情况，可以使用默认图标或者添加适当的错误处理逻辑
                    notifyIcon.Icon = SystemIcons.Information;
                }
            }

            notifyIcon.Text = "JToolBox";
            notifyIcon.Visible = true;

            // 直接设置托盘图标的属性
            notifyIcon.BalloonTipTitle = "JToolBox";
            notifyIcon.BalloonTipText = content;

            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;

            // 显示通知
            notifyIcon.ShowBalloonTip(3000); // 显示通知 3 秒钟
        }
        private double delaytime; 
        private void AddBlackText(string text)
        {
            Paragraph paragraph = new Paragraph(new Run(text)
            {
                Foreground = System.Windows.Media.Brushes.Black,
                FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei UI"),
                FontSize = 14
            });
            LRichTextBox.Document.Blocks.Add(paragraph);
        }
        private void AddRedText(string text)
        {
            Paragraph paragraph = new Paragraph(new Run(text)
            {
                Foreground = System.Windows.Media.Brushes.Red,
                FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei UI"),
                FontSize = 14
            });
            LRichTextBox.Document.Blocks.Add(paragraph);
        }
        private void delayTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 检查输入是否为数字
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true; // 阻止非数字字符的输入
            }
        }

        private void delayTextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            textBox.Text = new string(textBox.Text.Where(char.IsDigit).ToArray());
        }

        private void LRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            LRichTextBox.Document.Blocks.Clear();
            OMGtotal.Show("Welcome","欢迎使用JToolBox","success.png");
            
        }

        private void TPTS_Unchecked(object sender, RoutedEventArgs e)
        {
            IsTPTS = TPTS.IsChecked.GetValueOrDefault();
            if (IsTPTS)
            {
                OMGtotal.Show("提示", "托盘提示已经开启", "success.png");
            }
            else
            {
                OMGtotal.Show("提示", "托盘提示已经关闭", "disable.png");
            }
        }

        private void DonotClickthis_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.bilibili.com/video/BV1GJ411x7h7/");
        }

        

        private void getbanbtn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer.PlayEmbeddedResourceAsync("src.getban.MP3");
        }

        private void tanboxbtn_Click(object sender, RoutedEventArgs e)
        {
            if(msgtitlebox.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("未填写标题");
                return;
            }
            if(msgtextbox.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("未填写内容");
                return;
            }
            if (selimgbox.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("未选择图片");
                return;
            }
            OMGtotal.Show(msgtitlebox.Text, msgtextbox.Text, selimgbox.Text);

        }
    }
}
