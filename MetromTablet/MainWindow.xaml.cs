using System;
using System.Windows;
using MetromTablet.Views;
using Microsoft.Win32;
using System.Reflection;
using System.Windows.Interop;
using System.ServiceProcess;
using MetromTablet.Helper;


namespace MetromTablet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Window mainWindow;

        public static bool prodTimerRan = false;
        public static bool gotTime = false;


        public MainWindow()
        {
            InitializeComponent();

            Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height + 2;//SystemParameters.MaximizedPrimaryScreenHeight;
            Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width + 2;//SystemParameters.MaximizedPrimaryScreenWidth;

            mainWindow = this;
            lblTitle.Content = this.Title += " v." + Assembly.GetExecutingAssembly().GetName().Version;
            LogInPage p = new LogInPage();
            mrFrame.NavigationService.Navigate(p);

            //this.SourceInitialized += Window1_SourceInitialized;
            //this.Deactivated += Window_Deactivated;

            //AppDomain currentDomain = default(AppDomain);
            //currentDomain = AppDomain.CurrentDomain;
            //// Handler for unhandled exceptions.
            //currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            //System.Windows.Forms.Application.ThreadException += GlobalThreadExceptionHandler;
            //AllowMoving(false);

			//SetStartup();
			//DisableUAC();
			////ShutDowwnIfPowerOffline();
			////SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
			//DisableWindowsUpdate();
        }


        private static void DisableUAC()
        {
            RegistryKey key = Registry.LocalMachine
                .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key.GetValue("ConsentPromptBehaviorAdmin") == null || Convert.ToInt32(key.GetValue("ConsentPromptBehaviorAdmin")) != 0)
                key.SetValue("ConsentPromptBehaviorAdmin", 0);
            key.Close();
        }


        private static void SetStartup()
        {
            RegistryKey rKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rKey.SetValue("AURA", @"C:\METROM\AURAstartup.exe");//System.Reflection.Assembly.GetExecutingAssembly().Location);
            //rKey.DeleteValue(AppName, false);
        }


        //public void Window_Deactivated(object sender, EventArgs e)
        //{
        //	this.Topmost = true;
        //}

        public void Window1_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }


        public void AllowMoving(bool allow)
        {
            if (allow)
            {
                this.SourceInitialized -= Window1_SourceInitialized;
                //this.Deactivated -= Window_Deactivated;
            }
            else
            {
                this.SourceInitialized += Window1_SourceInitialized;
                //this.Deactivated += Window_Deactivated;
            }
        }


        private void DisableWindowsUpdate()
        {
            var sc = new ServiceController("wuauserv");
            try
            {
                if (sc != null && sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Disabled);
                sc.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IntPtr intPtr = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                EdgeGestureUtil.DisableEdgeGestures(intPtr, true);
            }
            catch { }
        }

        //DispatcherTimer shutdownTimer = new DispatcherTimer();

        //void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        //{

        //    if (e.Mode == PowerModes.StatusChange)
        //    {
        //        PowerStatus powerStatus = SystemInformation.PowerStatus;

        //        if (powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline)
        //        {
        //            //shutdownTimer.Interval = new TimeSpan(0, 0, 30);
        //            //shutdownTimer.IsEnabled = true;
        //            //shutdownTimer.Tick += shutdownTimer_Tick;

        //            //	Process.Start("shutdown", "/s /t 5");
        //            new Thread(() =>
        //            {
        //                string msg = "The system will shutdown in 30 seconds";

        //                if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        //                {
        //                    System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, msg);
        //                }
        //                else
        //                {
        //                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Action(() =>
        //                    {
        //                        System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, msg);
        //                    }));
        //                }
        //                //System.Windows.MessageBox.Show(this, "The system will shutdown in 30 seconds");
        //            }).Start();

        //        }
        //        if (powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
        //        {
        //            shutdownTimer.IsEnabled = false;
        //        }
        //    }
        //}


        //private void shutdownTimer_Tick(object sender, EventArgs e)
        //{
        //    System.Windows.Application.Current.Shutdown();
        //    Process.Start("shutdown", "/s /t 1");
        //}


        //void ShutDowwnIfPowerOffline()
        //{
        //    PowerStatus powerStatus = SystemInformation.PowerStatus;

        //    if (powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline)
        //    {
        //        shutdownTimer.Interval = new TimeSpan(0, 0, 30);
        //        shutdownTimer.IsEnabled = true;
        //        shutdownTimer.Tick += shutdownTimer_Tick;

        //        //Process.Start("shutdown", "/s /t 5");

        //        new Thread(() =>
        //        {
        //            string msg = "Power is offline. Please, connect power supply" + Environment.NewLine +
        //                "Otherwise the system will shutdown in 30 seconds";
        //            Thread.Sleep(3000);
        //            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        //            {
        //                System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, msg);
        //            }
        //            else
        //            {
        //                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Action(() =>
        //                {
        //                    System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, msg);
        //                }));
        //            }
        //        }).Start();
        //    }
        //}


        //private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        //{
        //	Exception ex = default(Exception);
        //	ex = (Exception)e.ExceptionObject;
        //	ILog log = LogManager.GetLogger(typeof(MetromRailPage));
        //	log.Error(ex.Message + "\n" + ex.StackTrace);
        //}


        //private static void GlobalThreadExceptionHandler(object sender, System.Threading.ThreadExceptionEventArgs e)
        //{
        //	Exception ex = default(Exception);
        //	ex = e.Exception;
        //	ILog log = LogManager.GetLogger(typeof(MetromRailPage)); //Log4NET
        //	log.Error(ex.Message + "\n" + ex.StackTrace);
        //}

    }
}
