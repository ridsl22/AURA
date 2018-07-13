using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MetromTablet.Helper;


namespace MetromTablet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		Log log = Log.GetInstance();
		//private Mutex mutex;

		//[DllImport("user32.dll")]
		//[return: MarshalAs(UnmanagedType.Bool)]
		//static extern bool SetForegroundWindow(IntPtr hWnd);

		public App()
		{
            using (Process p = Process.GetCurrentProcess())
                p.PriorityClass = ProcessPriorityClass.High;
            Startup += new StartupEventHandler(App_Startup); // Can be called from XAML 			

			//// Try to grab mutex
			//bool createdNew;
			//mutex = new Mutex(true, "MetromTablet", out createdNew);

			//if (!createdNew)
			//{
			//	// Bring other instance to front and exit.
			//	Process current = Process.GetCurrentProcess();
			//	foreach (Process process in Process.GetProcessesByName(current.ProcessName))
			//	{
			//		if (process.Id != current.Id)
			//		{
			//			SetForegroundWindow(process.MainWindowHandle);
			//			break;
			//		}
			//	}
			//	Application.Current.Shutdown();
			//}
			//else
			//{
			//	// Add Event handler to exit event.
			//	Exit += CloseMutexHandler;
			//}
		} 


		void App_Startup(object sender, StartupEventArgs e)
		{
			//Here if called from XAML, otherwise, this code can be in App() 
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}


		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;
			log.ProcessError(exception);
			if (e.IsTerminating)
			{
				//write the critical error file! 
			}
		}


		//protected virtual void CloseMutexHandler(object sender, EventArgs e)
		//{
		//	mutex.Close();
		//}
    }
}
