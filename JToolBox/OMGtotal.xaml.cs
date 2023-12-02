using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace JToolBox
{
    public partial class OMGtotal : Window
    {
        private const int AnimationDuration = 500;
        private const int DisplayDuration = 2000;
        private const int NotificationDelay = 200; // 连续通知之间的延迟时间（毫秒）
        private System.Windows.Media.Animation.DoubleAnimation slideInAnimation;
        private System.Windows.Media.Animation.DoubleAnimation slideOutAnimation;
        private static DateTime lastNotificationTime = DateTime.MinValue;

        private OMGtotal()
        {
            InitializeComponent();

            // 创建滑入动画
            slideInAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = SystemParameters.WorkArea.Width,
                To = SystemParameters.WorkArea.Width - Width,
                Duration = TimeSpan.FromMilliseconds(AnimationDuration),
                FillBehavior = FillBehavior.Stop
            };

            // 创建滑出动画
            slideOutAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = SystemParameters.WorkArea.Width,
                Duration = TimeSpan.FromMilliseconds(AnimationDuration),
                FillBehavior = FillBehavior.Stop
            };

            // 当动画完成时，关闭窗口
            slideOutAnimation.Completed += (sender, e) => Close();

            // 设置窗口为圆角
            SetWindowRoundCorner();

            // 设置窗口置顶
            Topmost = true;

            // 设置窗口不在任务栏中显示
            ShowInTaskbar = false;
        }

        private void SetWindowRoundCorner()
        {
            var windowInteropHelper = new WindowInteropHelper(this);
            var handle = windowInteropHelper.Handle;
            var windowChrome = new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(10),
                GlassFrameThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, windowChrome);

            var margins = new MARGINS { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
            DwmExtendFrameIntoClientArea(handle, ref margins);
        }

        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [System.Runtime.InteropServices.DllImport("DwmApi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        private void ShowNotification(string title, string text, string icon)
        {
            // 检查是否满足通知延迟条件
            if ((DateTime.Now - lastNotificationTime).TotalMilliseconds < NotificationDelay)
            {
                // 如果通知间隔太短，延迟显示当前通知
                var timer1 = new System.Windows.Threading.DispatcherTimer();
                timer1.Tick += (sender, e) => ShowNotification(title, text, icon);
                timer1.Interval = TimeSpan.FromMilliseconds(NotificationDelay);
                timer1.Start();

                return;
            }

            // 更新上次通知时间
            lastNotificationTime = DateTime.Now;
            // 设置标题和正文
            Title.Content = title;
            this.text.Text = text;

            // 设置图标
            string iconResourceName = $"JToolBox.src.{icon}";
            using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconResourceName))
            {
                if (iconStream != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = iconStream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 加载到内存中
                    bitmap.EndInit();
                    this.icon.Source = bitmap;
                }
            }

            // 设置窗口初始位置为工作区右下角
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom - Height;

            // 显示窗口
            Application.Current.Dispatcher.Invoke(() =>
            {
                Show();
            });

            // 启动滑入动画
            BeginAnimation(Window.LeftProperty, slideInAnimation);

            // 设置计时器，在 DisplayDuration 后启动滑出动画并关闭窗口
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += (sender, e) =>
            {
                BeginAnimation(Window.LeftProperty, slideOutAnimation);



                timer.Stop();
            };
            timer.Interval = TimeSpan.FromMilliseconds(DisplayDuration);
            timer.Start();
        }

        public static void Show(string title, string text, string icon)
        {
            var notificationWindow = new OMGtotal();
            notificationWindow.ShowNotification(title, text, icon);
        }
    }
}