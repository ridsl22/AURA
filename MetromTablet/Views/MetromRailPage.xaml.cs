using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MetromTablet.Models;
using System.IO.Ports;
using MetromTablet.RS485Manager;
using System.IO;
using MetromTablet.Communication;
using System.Diagnostics;
using MetromTablet.Views;
using System.Reflection;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Threading;
using Metrom.AURA.Log;
using Metrom.AURA.Base;
using Metrom.Base;
using System.IO.Compression;
using Metrom.AURA.ViewLog;


namespace MetromTablet
{
	/// <summary>
	/// Interaction logic for MetromRailPage.xaml
	/// </summary>
	public partial class MetromRailPage : Page, INotifyPropertyChanged, IViewLogDlgHost
	{

		#region Fields

		private double latitude = 49.83242;
		private double longitude = 023.99657;
		private string _displayTextFirstRow = " ---------------- ";
		private string _displayTextSecondRow = " ---------------- ";
		private string peerFrontRange = "----";
		private string peerRearRange = "----";
		private const string ACUPath = "Client-ACU-Release\\AURADiag.exe";
		private DateTime dt = new DateTime();

        private string jsonData;
        private string diagnosticsData;
        private int jsonCounter = 0;
        private string jsProductivity = String.Empty;
        private string jsConfig = String.Empty;
        private Visibility metromVisible;
        private Visibility btnHiddenVisible;

        private bool serialPortOpen = false;
        private bool sentMaintZip = false;
        private bool sentEventZip = false;
        private bool sentIncidentZip = false;
        private bool prodTimerRan = false;
        private bool gotTime = false;

        public static SerialPort serialPort;

        private List<string> portList;

		private int portIndex = 0;

		private SerialPortConfig serialPortConfig = new SerialPortConfig();

		private byte[] inBuf_ = new byte[2048];

		private byte[] outBuf_ = new byte[256];

		private byte outSeqNo_ = 0;

		private ushort lastInSeqNo_ = 0xffff;

		private DirectMessageReadManager readMgr_ = new DirectMessageReadManager();

		private Dictionary<uint, Peer> peerMap_ = new Dictionary<uint, Peer>();

		private ObservableCollection<Peer> peers_ = new ObservableCollection<Peer>();

		private DateTime lastCEMRangeData_ = DateTime.MinValue;

		private AURAMessageFactory auraMsgMgr_;

		private DeviceInfo devInfo_ = new DeviceInfo();

		private AppCommState commState_ = AppCommState.NotConnected;

		private string receivedData = String.Empty;

		private string port;

		private string profile = String.Empty;
		private static DispatcherTimer prodDispatcherTimer;
		private DispatcherTimer flashTimer = new DispatcherTimer();
		private DispatcherTimer mIdle;
        private DispatcherTimer serialListeningTimer;
        private DispatcherTimer serialDetectingTimer;
        private DispatcherTimer releaseMemoryTimer;
        private NumKeyboardPopup np;
		private CalibDataPopUp calib;

		private Msg_UIGPSUpdate gpsUpdate = new Msg_UIGPSUpdate();
		private const uint kSecPerDay = 24 * 60 * 60;

		public static uint cCode, mCode;
		public string maintLogFileName = "maintLog_";
		public string maintLogFileNameWithoutExt = "maintLog_";
		public string metaFileName;
		public byte[] otaData, otaDataReceived;
		private string otaArchiveName;
		private UIMConfig uimConfig = new UIMConfig();
		private CEMConfig cemConfig = new CEMConfig();
		private MachineConfig machineConfig;
		private AppSettings appSettings;
		public static bool pmAcknowledged = false;

		public static byte[] fwUpdateData_ = null;
		public static uint fwUpdateSize_ = 0;
		public static uint fwUpdateCurOfs_ = 0;
		public static uint fwUpdateCurOfsPM_ = 0;

		private ViewLog2Dlg dlgViewLog2_ = null;
		private UIInfo ui_ = new UIInfo();
		private TwoPartVersion ConfigVersion = new TwoPartVersion();
		private Productivity productivity = new Productivity();
		private string jsToSend = String.Empty;
		private string startTimeEvent = String.Empty;
		private string startTimeIncident = String.Empty;
		private bool eventlog = false;
		private bool canExec = false;
		private string mainMetaFileName, incidentMetaFileName, maintMetaFileName;

		private const uint kMaxFWUpdateBlockSize = 128;

		private string status = String.Empty;
		private bool btnUpdateFWEnabled = true;
		private bool btnUpdateFWCEMEnabled = true;
        private bool isDeviceConnected;
        private bool hiddenMenuUnlocked = false;
		private string metromDir = @"C:\METROM\";
		private string logDir = @"C:\METROM\Logs\";
		private string metromUpdatesDir = @"C:\METROM\Updates\";
        
        private ICommand upCommand;
		private ICommand downCommand;
		private ICommand leftCommand;
		private ICommand rightCommand;
		private ICommand confirmCommand;
		private ICommand cancelCommand;
		private ICommand checkListCommand;
		private ICommand configSerialCommand;
		private ICommand displayJsonLogCommand;
		private ICommand displayDiagCommand;
		//private ICommand displayMachineConfigCommand;
		//private ICommand hiddenCommand;
		private ICommand acuCommand;
		private ICommand viewMainLogCommand;
		private ICommand viewRecentMainLogCommand;
		private ICommand viewIncLogCommand;
		private ICommand viewRecentIncLogCommand;
		private ICommand updateCEMCommand;
		private ICommand updatePMCommand;
		private ICommand updCommand;
		private ICommand clearCommand;
		private ICommand lockCommand;
		private ICommand hiddenTouchDownCommand;
		private ICommand hiddenTouchUpCommand;

		#endregion Fields


		#region Properties

		public string Port
		{
			get { return port; }
			set { port = value; }
		}


		public DeviceInfo DevInfo
		{ get { return devInfo_; } }


		public string DisplayTextFirstRow
		{
			get
			{
				return _displayTextFirstRow;
			}
			set
			{
				_displayTextFirstRow = value;
				NotifyPropertyChanged("DisplayTextFirstRow");
			}
		}


		public string DisplayTextSecondRow
		{
			get
			{
				return _displayTextSecondRow;
			}
			set
			{
				_displayTextSecondRow = value;
				NotifyPropertyChanged("DisplayTextSecondRow");
			}
		}

		public bool BtnUpdateFWEnabled
		{
			get
			{
				return btnUpdateFWEnabled;
			}
			set
			{
				btnUpdateFWEnabled = value;
				NotifyPropertyChanged("BtnUpdateFWEnabled");
			}
		}


		public bool BtnUpdateFWCEMEnabled
		{
			get
			{
				return btnUpdateFWCEMEnabled;
			}
			set
			{
				btnUpdateFWCEMEnabled = value;
				NotifyPropertyChanged("BtnUpdateFWCEMEnabled");
			}
		}


		public string PeerFrontRange
		{
			get
			{
				return peerFrontRange;
			}
			set
			{
				peerFrontRange = value;
				NotifyPropertyChanged("PeerFrontRange");
			}
		}


		public string PeerRearRange
		{
			get
			{
				return peerRearRange;
			}
			set
			{
				peerRearRange = value;
				NotifyPropertyChanged("PeerRearRange");
			}
		}


		public string ReceivedData
		{
			get
			{
				return receivedData;
			}
			set
			{
				receivedData = value;
				NotifyPropertyChanged("ReceivedData");
			}
		}


		public string JsonData
		{
			get
			{
				return jsonData;
			}
			set
			{
				jsonData = value;
				NotifyPropertyChanged("JsonData");
			}
		}


		public string DiagnosticsData
		{
			get
			{
				return diagnosticsData;
			}
			set
			{
				diagnosticsData = value;
				NotifyPropertyChanged("DiagnosticsData");
			}
		}


		public string Status
		{
			get
			{
				return status;
			}
			set
			{
				status = value;
				NotifyPropertyChanged("Status");
			}
		}


        public Visibility MetromVisible
        {
            get
            {
                return metromVisible;
            }
            set
            {
                metromVisible = value;
                NotifyPropertyChanged("MetromVisible");
            }
        }


        public Visibility BtnHiddenVisible
        {
            get
            {
                return btnHiddenVisible;
            }
            set
            {
                btnHiddenVisible = value;
                NotifyPropertyChanged("BtnHiddenVisible");
            }
        }


        #endregion Properties


        #region Commands

        public ICommand UpCommand
		{
			get
			{
				if (upCommand == null)
				{
					upCommand = new RelayCommand(GoUp);
				}
				return upCommand;
			}
		}

		public ICommand DownCommand
		{
			get
			{
				if (downCommand == null)
				{
					downCommand = new RelayCommand(GoDown);
				}
				return downCommand;
			}
		}

		public ICommand RightCommand
		{
			get
			{
				if (rightCommand == null)
				{
					rightCommand = new RelayCommand(GoRight);
				}
				return rightCommand;
			}
		}

		public ICommand LeftCommand
		{
			get
			{
				if (leftCommand == null)
				{
					leftCommand = new RelayCommand(GoLeft);
				}
				return leftCommand;
			}
		}

		public ICommand ConfirmCommand
		{
			get
			{
				if (confirmCommand == null)
				{
					confirmCommand = new RelayCommand(Confirm);
				}
				return confirmCommand;
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				if (cancelCommand == null)
				{
					cancelCommand = new RelayCommand(Cancel);
				}
				return cancelCommand;
			}
		}

		public ICommand CheckListCommand
		{
			get
			{
				if (checkListCommand == null)
				{
					checkListCommand = new RelayCommand(InitCheckList);
				}
				return checkListCommand;
			}
		}

		public ICommand ConfigSerialCommand
		{
			get
			{
				if (configSerialCommand == null)
				{
					configSerialCommand = new RelayCommand(ConfigSerial);
				}
				return configSerialCommand;
			}
		}

		public ICommand DisplayJsonLogCommand
		{
			get
			{
				if (displayJsonLogCommand == null)
				{
					displayJsonLogCommand = new RelayCommand(DisplayJsonLog);
				}
				return displayJsonLogCommand;
			}
		}


		public ICommand DisplayDiagCommand
		{
			get
			{
				if (displayDiagCommand == null)
				{
					displayDiagCommand = new RelayCommand(DisplayDiag);
				}
				return displayDiagCommand;
			}
		}


		//public ICommand DisplayMachineConfigCommand
		//{
		//	get
		//	{
		//		if (displayMachineConfigCommand == null)
		//		{
		//			displayMachineConfigCommand = new RelayCommand(DisplayMachineConfig);
		//		}
		//		return displayMachineConfigCommand;
		//	}
		//}

		public ICommand AcuCommand
		{
			get
			{
				if (acuCommand == null)
				{
					acuCommand = new RelayCommand(ShowCalib);
				}
				return acuCommand;
			}
		}


		public ICommand ViewMainLogCommand
		{
			get
			{
				if (viewMainLogCommand == null)
				{
					viewMainLogCommand = new RelayCommand(DisplayMainLogs);
				}
				return viewMainLogCommand;
			}
		}

		public ICommand ViewRecentMainLogCommand
		{
			get
			{
				if (viewRecentMainLogCommand == null)
				{
					viewRecentMainLogCommand = new RelayCommand(DisplayRecentMainLogs);
				}
				return viewRecentMainLogCommand;
			}
		}

		public ICommand ViewIncLogCommand
		{
			get
			{
				if (viewIncLogCommand == null)
				{
					viewIncLogCommand = new RelayCommand(DisplayIncidentLogs);
				}
				return viewIncLogCommand;
			}
		}

		public ICommand ViewRecentIncLogCommand
		{
			get
			{
				if (viewRecentIncLogCommand == null)
				{
					viewRecentIncLogCommand = new RelayCommand(DisplayRecentIncidentLogs);
				}
				return viewRecentIncLogCommand;
			}
		}

		public ICommand UpdateCEMCommand
		{
			get
			{
				if (updateCEMCommand == null)
				{
					updateCEMCommand = new RelayCommand(UpdateCEM);
				}
				return updateCEMCommand;
			}
		}

		public ICommand UpdatePMCommand
		{
			get
			{
				if (updatePMCommand == null)
				{
					updatePMCommand = new RelayCommand(UpdatePM);
				}
				return updatePMCommand;
			}
		}

		public ICommand ClearCommand
		{
			get
			{
				if (clearCommand == null)
				{
					clearCommand = new RelayCommand(ClearData);
				}
				return clearCommand;
			}
		}

		public ICommand LockCommand
		{
			get
			{
				if (lockCommand == null)
				{
					lockCommand = new RelayCommand(Lock);
				}
				return lockCommand;
			}
		}

		public ICommand UpdCommand
		{
			get
			{
				if (updCommand == null)
				{
					updCommand = new RelayCommand(UpdateApp);
				}
				return updCommand;
			}
		}

		public ICommand HiddenTouchDownCommand
		{
			get
			{
				if (hiddenTouchDownCommand == null)
				{
					hiddenTouchDownCommand = new RelayCommand(HiddenTouchDown);
				}
				return hiddenTouchDownCommand;
			}
		}

		public ICommand HiddenTouchUpCommand
		{
			get
			{
				if (hiddenTouchUpCommand == null)
				{
					hiddenTouchUpCommand = new RelayCommand(HiddenTouchUp);
				}
				return hiddenTouchUpCommand;
			}
		}

		#endregion Commands


		#region Constructor

		public MetromRailPage()
		{
			InitializeComponent();
			this.DataContext = this;

			Version appVer = Assembly.GetExecutingAssembly().GetName().Version;
            InitCheckList();
            MetromVisible = Visibility.Hidden;

            maintLogFileNameWithoutExt += DateTime.Today.ToString("yyyyMMdd");
			maintLogFileName = maintLogFileNameWithoutExt + ".txt";

            serialListeningTimer = new DispatcherTimer();
            serialListeningTimer.Tick += SerialListeningTimer_Tick;
            serialListeningTimer.Interval = new TimeSpan(0, 0, 2);

            serialDetectingTimer = new DispatcherTimer();
            serialDetectingTimer.Tick += SerialDetectingTimer_Tick;
            serialDetectingTimer.Interval = new TimeSpan(0, 0, 5);
            serialDetectingTimer.IsEnabled = false;

            readMgr_.NewPacket += readMgr_NewPacket;
			auraMsgMgr_ = new AURAMessageFactory(Dispatcher, this);

			serialPortConfig.BaudRate = 115200;
			serialPortConfig.DataBits = 8;
			serialPortConfig.Parity = Parity.None;
			serialPortConfig.StopBits = StopBits.One;
			serialPortConfig.Handshake = Handshake.None;
			serialPortConfig.WriteTimeout = 100;
			serialPortConfig.ReadTimeout = 100;

			SetAppSettings();
			WindowsAccess(false);

			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}

            //user inactivity
            //InputManager.Current.PreProcessInput += Idle_PreProcessInput;
            mIdle = new DispatcherTimer();
            mIdle.Interval = new TimeSpan(0, 0, appSettings.IdleIntervalSec);
            mIdle.IsEnabled = true;
            mIdle.Tick += Idle_Tick;

            releaseMemoryTimer = new DispatcherTimer();
            releaseMemoryTimer.Interval = new TimeSpan(1, 0, 0);
            releaseMemoryTimer.Tick += ReleaseMemoryTimer_Tick;
            releaseMemoryTimer.Start();

            btnDisplayMainLogs.IsEnabled = false;
			btnDisplayIncidentLogs.IsEnabled = false;
			btnDisplayRecentMainLogs.IsEnabled = false;
			btnDisplayRecentIncidentLogs.IsEnabled = false;
		}

        #endregion Constructor


        #region Methods


        #region AURA UI

        private void Confirm()
		{
            Status = "BTN_CONFIRM_PRESSED";
            SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CONFIRM_PRESSED));
		}


		private void GoUp()
		{
            Status = "BTN_UP_PRESSED";
            SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_UP_PRESSED));
		}


		private void GoDown()
		{
            Status = "BTN_DOWN_PRESSED";
            SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_DOWN_PRESSED));
		}


		private void GoRight()
		{
            Status = "BTN_RIGHT_PRESSED";
            SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_RIGHT_PRESSED));
		}


		private void GoLeft(object obj)
		{
            Status = "BTN_LEFT_PRESSED";
            SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_LEFT_PRESSED));
		}


		private void Cancel()
		{
            Status = "BTN_CANCEL_PRESSED";
            SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
		}


		//private void DisplayMachineConfig()
		//{
		//	textBoxConfigData.Visibility = Visibility.Visible;
		//	btnJson.Content = "Back";
		//	btnClear.Visibility = Visibility.Visible;
		//	try
		//	{
		//		if (LogInPage.machineType == "HCTREAS")
		//			textBoxConfigData.Text = File.ReadAllText(metromDir + "hctreasConfig.json");
		//		else
        //                  textBoxConfigData.Text = File.ReadAllText(metromDir + "jupiterConfig.json");
		//	}
		//	catch { }
		//	lblInfo.Content = LogInPage.machineType + " config";
		//}


		private void DisplayJsonLog()
		{
            if (!btnJson.Content.Equals("Back"))
            {
				textBoxJsonData.Visibility = Visibility.Visible;
				textBoxJsonData.Text = JsonData;
				btnJson.Content = "Back";
				btnClear.Visibility = Visibility.Visible;
				lblInfo.Content = "Modem JSON data";
				textBoxJsonData.ScrollToEnd();
			}
			else
			{
				textBoxJsonData.Visibility = Visibility.Hidden;
				textBoxConfigData.Visibility = Visibility.Hidden;
				textBoxDiag.Visibility = Visibility.Hidden;
				btnClear.Visibility = Visibility.Hidden;
				btnJson.Content = "   Display\nJSON Data";
				lblInfo.Content = String.Empty;
			}
		}


		private void DisplayDiag()
		{
			textBoxDiag.Visibility = Visibility.Visible;
			btnJson.Content = "Back";
			btnClear.Visibility = Visibility.Visible;
			lblInfo.Content = "  Diagnostics logs";
			textBoxDiag.ScrollToEnd();
		}


		private void ConfigSerial()
		{
			portIndex = 0;
			if (serialPort != null)
				SetupNewPort(serialPort, true);
			if (!ConfigSerialUntilPortGoodOrCancel())
				return;
			StartNewSession();
		}


		public static void GetSecurityCode()
		{
			// Calculate the current Metrom Reference Time (MRT) Base time value. MRT is seconds
			// since the MRT Epoch (2011/01/01 00:00:00 UTC); the MRT Base value is seconds since
			// the MRT Epoch for whole days to current (i.e., whole days to but not including the
			// current day).
			uint mrt = (uint)Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
			uint mrtBase = mrt - (mrt % kSecPerDay);

			// Calculate the number of days represented by the MRT Base value and use that to
			// calculate ersatz pseudorandom numbers to serve as "security codes": one 3-digit
			// for the "customer code" and one 4-digit for the "Metrom code".
    		uint mrtDay = mrtBase / kSecPerDay;

			uint cKey = ((mrtDay * 17) ^ (((mrtDay * 3) & 0x00cf) << 5)) | (mrtDay / 271);

			cCode = 203 + (cKey % 769);
			if ((cCode % 100) == 0)
				--cCode;

			uint mKey = ((mrtDay * 17) ^ (((mrtDay * 3) & 0x00ff) << 8)) | (mrtDay / 271);

			mCode = 1301 + (mKey % 8597);
			if ((mCode % 100) == 0)
				--mCode;

			//uint oldKey = (mrtDay ^ ((mrtDay & 0xcf) << 5)) | (mrtDay / 271);
			//uint oldCode = 203 + (oldKey % 769);
			//if ((oldCode % 100) == 0)
			//	--oldCode;
		}


		// ----- ML_YYYYMMDDhhmmss.json ----- Maintenance log metafile -----
		private void GenerateMetaFile(string metaFilePath, string asset, string time, LongData dataType,
			double lat, double lon, string startTime, string endTime, string fileName, int fileSize)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine("\"log\": {");
			sb.AppendLine("\"asset\":\"" + asset + "\",");
			sb.AppendLine("\"time\":\"" + time + "\",");

            switch (dataType)
            {
                case LongData.CasEventLog:
                    sb.AppendLine("\"type\":\"" + "CAS_Event" + "\",");
                    break;
                case LongData.CasIncidentLog:
                    sb.AppendLine("\"type\":\"" + "CAS_Inc" + "\",");
                    break;
                default:
                    sb.AppendLine("\"type\":\"" + dataType.ToString() + "\",");
                    break;
            }

            sb.AppendLine("\"loc\": {");
			sb.AppendLine("\"lat\":" + lon.ToString() + ",");
			sb.AppendLine("\"lon\":" + lat.ToString());
			sb.AppendLine("},");
			sb.AppendLine("\"startTime\":\"" + startTime + "\",");
			sb.AppendLine("\"endTime\":\"" + endTime + "\",");
			sb.AppendLine("\"fileName\":\"" + fileName + "\",");
			sb.AppendLine("\"fileSize\":" + (fileSize / 1024 + 1).ToString());
			sb.AppendLine("}");
			sb.AppendLine("}");
			File.WriteAllText(metaFilePath, sb.ToString());
		}


		int maintLogSize;

		private void GenerateMaintLog(string machineName, double lat, double lon, ObservableCollection<CheckList> logTasks)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Maintenance Log");
			sb.AppendLine(machineName);
			sb.AppendLine(DateTime.Now.ToString("MM/dd/yyyy, HH:mm") + "(PC)");//(GPS (or "cell" or "PC")
			sb.AppendLine("Location: (" + lat.ToString() + ", " + lon.ToString() + ")");
			sb.AppendLine("Operator: " + LogInPage.logIn);

			foreach (CheckList l in logTasks)
			{
				if (l.Version == vDaily)
				{
					sb.AppendLine();
					sb.AppendLine("============================= Daily Checklist");
					sb.AppendLine();
					sb.AppendLine("========== Completed Items:");
					foreach (Task t in l.Tasks)
					{
						if (t.IsChecked)
							sb.AppendLine(t.Name);
					}
					sb.AppendLine();
					sb.AppendLine("========== Skipped items:");
					foreach (Task t in l.Tasks)
					{
						if (!t.IsChecked)
							sb.AppendLine(t.Name);
					}
				}
			}

			foreach (CheckList l in logTasks)
			{
				if (l.Version == v50hours)
				{
					sb.AppendLine();
					sb.AppendLine("============================= 50 Hours Checklist");
					sb.AppendLine();
					sb.AppendLine("========== Completed Items:");
					foreach (Task t in l.Tasks)
					{
						if (t.IsChecked)
							sb.AppendLine(t.Name);
					}
					sb.AppendLine();
					sb.AppendLine("========== Skipped items:");
					foreach (Task t in l.Tasks)
					{
						if (!t.IsChecked)
							sb.AppendLine(t.Name);
					}
				}
			}

			foreach (CheckList l in logTasks)
			{
				if (l.Version == v200hours)
				{
					sb.AppendLine();
					sb.AppendLine("============================= 200 Hours Checklist");
					sb.AppendLine();
					sb.AppendLine("========== Completed Items:");
					foreach (Task t in l.Tasks)
					{
						if (t.IsChecked)
							sb.AppendLine(t.Name);
					}
					sb.AppendLine();
					sb.AppendLine("========== Skipped items:");
					foreach (Task t in l.Tasks)
					{
						if (!t.IsChecked)
							sb.AppendLine(t.Name);
					}
				}
			}

			foreach (CheckList l in logTasks)
			{
				if (l.Version == vSeason)
				{
					sb.AppendLine();
					sb.AppendLine("============================= Seasonal Checklist");
					sb.AppendLine();
					sb.AppendLine("========== Completed Items:");
					foreach (Task t in l.Tasks)
					{
						if (t.IsChecked)
							sb.AppendLine(t.Name);
					}
					sb.AppendLine();
					sb.AppendLine("========== Skipped items:");
					foreach (Task t in l.Tasks)
					{
						if (!t.IsChecked)
							sb.AppendLine(t.Name);
					}
				}
			}

			AddNotDueChecklist(sb);
			File.WriteAllText(maintLogFileName, sb.ToString());
			maintLogSize = sb.Length;
		}


		private void AddNotDueChecklist(StringBuilder sb)
		{
			if (!sb.ToString().Contains(vDaily))
			{
				sb.AppendLine();
				sb.AppendLine("============================= Daily Checklist");
				sb.AppendLine("Not due.");
			}
			if (!sb.ToString().Contains(v50hours))
			{
				sb.AppendLine();
				sb.AppendLine("============================= 50 Hours Checklist");
				sb.AppendLine("Not due.");
			}
			if (!sb.ToString().Contains(v200hours))
			{
				sb.AppendLine();
				sb.AppendLine("============================= 200 Hours Checklist");
				sb.AppendLine("Not due.");
			}
			if (!sb.ToString().Contains(vSeason))
			{
				sb.AppendLine();
				sb.AppendLine("============================= Seasonal Checklist");
				sb.AppendLine("Not due.");
			}
		}


		static readonly object locker = new object();
        //byte[] simulated = new byte[125];//170, 0, 85, 125, 0,


		private void SendCasLogZip(string assetname, string fileName, LongData lData)
		{
			string zipLogName = assetname + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";
			string zipLog = logDir + zipLogName;
			ZipArchive zip;

			try
			{
				if (File.Exists(zipLog))
				{
					File.Delete(zipLog);
				}

				zip = ZipFile.Open(zipLog, ZipArchiveMode.Create);
				zip.CreateEntryFromFile(fileName, System.IO.Path.GetFileName(fileName), CompressionLevel.Optimal);

				switch (lData)
				{
					case LongData.Maint:
						zip.CreateEntryFromFile(maintMetaFileName, System.IO.Path.GetFileName(maintMetaFileName), CompressionLevel.Optimal);
						break;
					case LongData.CasEventLog:
						zip.CreateEntryFromFile(mainMetaFileName, System.IO.Path.GetFileName(mainMetaFileName), CompressionLevel.Optimal);
						break;
					case LongData.CasIncidentLog:
						zip.CreateEntryFromFile(incidentMetaFileName, System.IO.Path.GetFileName(incidentMetaFileName), CompressionLevel.Optimal);
						break;
				}
			
				zip.Dispose();

				byte[] zipData = File.ReadAllBytes(zipLog);
				int dataLength = zipData.Length;
				int remainder = 120 - dataLength % 120;
				byte[] fullData = new byte[dataLength + remainder];
				int i;

				for (i = 0; i < dataLength; i++)
				{
					fullData[i] = zipData[i];
				}

				for (i = dataLength; i < dataLength + remainder; i++)
				{
					fullData[i] = 255;
				}

				int curIndex = 0;
				byte[] tempData = new byte[512];

				//th = new Thread(() => 
				//{
				//    Thread.CurrentThread.IsBackground = true;
				pmAcknowledged = false;

				SendMgmtMsg(new Msg_UILogInfo(lData, 120, (uint)dataLength, (uint)remainder, zipLogName));

                //simulated[0] = 170;
                //simulated[1] = 0;
                //simulated[2] = 85;
                //simulated[4] = 120;

				DateTime sentLastChunkTime = DateTime.Now;
				while (curIndex < dataLength)
				{
					for (int j = 0; j < 120; j++)
					{
						tempData[j] = fullData[curIndex + j];
					}


					if (pmAcknowledged)
					{
						sentLastChunkTime = DateTime.Now;
						pmAcknowledged = false;
						curIndex += 120;
                        SendMgmtMsg(new Msg_UISendLogZip(tempData, 120));

                        //for (int ndx = 0; ndx < 125; ndx++)
                        //    serialPort.Write(simulated, ndx, 1);//remove!!!
 
						//pmAcknowledged = false;
					}

					if ((DateTime.Now - sentLastChunkTime).TotalSeconds > 120)
					{
						//Status = "Connection to the server has been lost...";
						BtnUpdateFWEnabled = true;
						BtnUpdateFWCEMEnabled = true;
						return;
					}

					int percent = 100 - (100 * curIndex / dataLength);

					switch (lData)
					{
						case LongData.Maint:
							Status = "Pending: maintenance log upload... " + percent.ToString() + "% remaining";
							break;
						case LongData.CasEventLog:
							Status = "Pending: CAS Event log upload... " + percent.ToString() + "% remaining";
							break;
						case LongData.CasIncidentLog:
							Status = "Pending: CAS Incident log upload... " + percent.ToString() + "% remaining";
							break;
					}
					BtnUpdateFWEnabled = false;
					BtnUpdateFWCEMEnabled = false;
				}

				Status = String.Empty;
				BtnUpdateFWEnabled = true;
				BtnUpdateFWCEMEnabled = true;				
			}
			catch (Exception e)
            {
               // MessageBox.Show(e.Message);
            }
		}


		private void CreateZipFromBytes(byte[] byteArray, string zipName)
		{
			try
			{
				if (!Directory.Exists(metromUpdatesDir))
				{
					Directory.CreateDirectory(metromUpdatesDir);
				}

				File.WriteAllBytes(zipName, byteArray);
			}
			catch { }
		}


		private void ClearData()
		{
			if (lblInfo.Content.ToString().Contains("JSON"))
			{
				JsonData = String.Empty;
			}
			else if (lblInfo.Content.ToString().Contains("Diagnostics"))
			{
				DiagnosticsData = string.Empty;
			}
			//else if (lblInfo.Content.ToString().Contains("config"))
			//{
			//	textBoxConfigData.Text = String.Empty;
			//}
		}


		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			//if ((Properties.Settings.Default.Port != null) && (Properties.Settings.Default.Port.Length > 0))
			if (serialPortOpen)
			{
				AttemptToOpenPort(Properties.Settings.Default.Port, false);
			}

			// If the default port hasn't been set yet, or if the serial port object doesn't exist
			// (which means the attempt to open the last port has failed), get the user to configure
			// serial...

			//if ((Properties.Settings.Default.Port == null) ||
			//	(Properties.Settings.Default.Port.Length == 0) ||
			//	(serialPort == null))
			//{
				// Get the user to select a serial port *or* indicate that they wish to run in demo
				// mode. If the user wants demo mode (a false return value), we're done here. Note
				// that in this case, demo mode will be set up inside the below call.
			//if (!serialPortOpen)
			else
			{
				if (!ConfigSerialUntilPortGoodOrCancel())
				{
					return;
				}
			//}

				devInfo_.ConnectSequenceComplete = ConnectSequenceCompleted;
				StartNewSession();
			}

			flashTimer.Tick += flashTimer_Tick;
			flashTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
		}


		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			if (serialPort != null)
				serialPort.Close();
            serialListeningTimer.Stop();
            serialDetectingTimer.Stop();
            releaseMemoryTimer.Stop();
            mIdle.Stop();
		}


		private void Lock()
		{
            MetromVisible = Visibility.Hidden;
            BtnHiddenVisible = Visibility.Visible;

            WindowsAccess(false);
		}


		private void HiddenTouchDown()
		{
			dt = DateTime.Now;
		}


		private void HiddenTouchUp()
		{
			double interval = (DateTime.Now - dt).TotalSeconds;
			if (interval >= 1)
			{
				np = new NumKeyboardPopup();
				np.ConfirmContent = "Unlock";
				np.Title = "Enter Security Code";
				np.MaxLen = 4;
				np.textBoxEntry.MaxLength = 4;
				np.CancelContent = "Cancel";
				np.ID = String.Empty;
				GetSecurityCode();
				np.ShowDialog();

				if (np.confirmed)
				{
                    MetromVisible = Visibility.Visible;
                    BtnHiddenVisible = Visibility.Hidden;
                    Unlock(Convert.ToInt32(np.ID));
					WindowsAccess(true);				
				}
			}
		}


		private void btnHidden_Click(object sender, RoutedEventArgs e)
		{
			np = new NumKeyboardPopup();
			np.ConfirmContent = "Unlock";
			np.Title = "Enter Security Code";
			np.MaxLen = 4;
			np.textBoxEntry.MaxLength = 4;
			np.CancelContent = "Cancel";
			np.ID = String.Empty;
			GetSecurityCode();

			np.ShowDialog();

            //dispatcherTimerEnterCode.Stop();
            MetromVisible = Visibility.Visible;
            BtnHiddenVisible = Visibility.Hidden;
            np.ID = "4267";
			Unlock(Convert.ToInt32(np.ID));
			WindowsAccess(true);
            //WindowsAccess(true);
            //MainWindow.mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
        }


        private void Unlock(int code)
		{
			bool metromCode = code / 1000 > 0;
			int first  = metromCode ? code / 1000		: code / 100;
			int second = metromCode ? code / 100 % 10 : code / 10 % 10;
			int third  = metromCode ? code / 10 % 10	: code % 10;
			int fourth = metromCode ? code % 10 + 1	: 0;

			if (code == mCode && !hiddenMenuUnlocked)
			{
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_LEFT_DOUBLE_DOWN_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_UP_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CONFIRM_PRESSED));
				Thread.Sleep(50);

				EnterDigit(first);

				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_RIGHT_PRESSED));
				Thread.Sleep(50);

				EnterDigit(second);

				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_RIGHT_PRESSED));
				Thread.Sleep(50);

				EnterDigit(third);

				if (metromCode)
				{
					SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_RIGHT_PRESSED));
					Thread.Sleep(50);

					EnterDigit(fourth);
				}

				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CONFIRM_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
				Thread.Sleep(50);
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));

				hiddenMenuUnlocked = true;
			}
		}


		private void EnterDigit(int second)
		{
			for (int i = 0; i < second; i++)
			{
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_UP_PRESSED));
				Thread.Sleep(50);
			}
		}


		private void WindowsAccess(bool deny)
		{
			try
			{
				var wnd = MainWindow.mainWindow;// (MainWindow)Window.GetWindow(this);

				if (deny)
				{
					//wnd.IgnoreTaskbarOnMaximize = false;
					//wnd.ShowCloseButton = true;
					wnd.Topmost = false;

					wnd.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width + 2;
					wnd.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height + 2;
					wnd.WindowState = WindowState.Normal;
					btnClose.Visibility = Visibility.Visible;
					//wnd.ResizeMode = ResizeMode.CanResize;
					//wnd.WindowStyle = WindowStyle.SingleBorderWindow;
					//wnd.AllowMoving(true);
					//wnd.SourceInitialized += wnd.Window1_SourceInitialized;
				}
				else
				{
					//wnd.IgnoreTaskbarOnMaximize = true;
					//wnd.ShowMinButton = false;
					//wnd.ShowMaxRestoreButton = false;
					//wnd.ShowCloseButton = false;
					wnd.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height + 2;//SystemParameters.MaximizedPrimaryScreenHeight;
					wnd.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width + 2;//SystemParameters.MaximizedPrimaryScreenWidth;
					//wnd.ResizeMode = ResizeMode.NoResize;
					//wnd.WindowStyle = WindowStyle.None;
					//wnd.ShowTitleBar = true;
					//wnd.SourceInitialized -= wnd.Window1_SourceInitialized;
					//wnd.Deactivated -= wnd.Window_Deactivated;
					wnd.WindowState = WindowState.Maximized; ;
					wnd.Topmost = true;
					btnClose.Visibility = Visibility.Hidden;
					//wnd.AllowMoving(false);
				}
			}
			catch { }
		}


		private FWUpdatePopup fwUpdatePopup;


		private void UpdatePM()
		{
			try
			{
				fwUpdatePopup = new FWUpdatePopup(true);
				fwUpdatePopup.Closed += (s, e_) =>
				{
					//EnableButtons(true);
					fwUpdatePopup = null;
					mIdle.IsEnabled = false;
					mIdle.IsEnabled = true;
				};
				fwUpdatePopup.ShowDialog();
				//EnableButtons(false);
			}
			catch { }
		}


		private void UpdateCEM()
		{
			try
			{
				fwUpdatePopup = new FWUpdatePopup(false);
				fwUpdatePopup.Closed += (s, e_) =>
				{
					//EnableButtons(true);
					fwUpdatePopup = null;
					mIdle.IsEnabled = false;
					mIdle.IsEnabled = true;
				};
				fwUpdatePopup.ShowDialog();
				//EnableButtons(false);
			}
			catch { }
		}


		private void EnableButtons(bool enable)
		{
			BtnUpdateFWEnabled = enable;
			BtnUpdateFWCEMEnabled = enable;
			btnDisplayMainLogs.IsEnabled = enable;
			btnDisplayIncidentLogs.IsEnabled = enable;
			btnDisplayRecentMainLogs.IsEnabled = enable;
			btnDisplayRecentIncidentLogs.IsEnabled = enable;
			btnACU.IsEnabled = enable;
			btnConfig.IsEnabled = enable;
			btnCheckLists.IsEnabled = enable;
		}


		public string ReturnCleanASCII(string s)
		{
			StringBuilder sb = new StringBuilder(s.Length);
			foreach (char c in s)
			{
				if ((int)c == 10)
					continue;
				if ((int)c == 13)
					continue;
				if ((int)c > 127)
					continue;
				if ((int)c < 32 && (int)c > 13)
					continue;
				if ((int)c < 13 && (int)c > 10)
					continue;
				if ((int)c < 10)
					continue;
				sb.Append(c);
			}
			return sb.ToString();
		}


		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string name)
		{
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
		#endregion AURA UI


		#region SerialPort

		private bool ConfigSerialUntilPortGoodOrCancel()
		{
			bool result = false;
			bool again = false;
			portList = GetPortsList();
			do
			{
				again = false;
				bool update = false;

				if (!serialPortOpen)
				{
					//ConfigSerialPopUp popupConfig = new ConfigSerialPopUp();
					//update = popupConfig.ShowDialog() ?? false;
					//Port = popupConfig.Port;
					update = true;
					Port = portList[portIndex];
				}
				else
				{
					Port = Properties.Settings.Default.Port;
					update = true;
				}
				if (update)
				{
					// User clicked OK.
					if (!AttemptToOpenPort(Port))
					{
						//MessageBox.Show(string.Format("Could not open the selected port ({0}); please try again.", Port),
						//	"Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
						again = PortIsPresentCheck() ? true : false;
						serialPortOpen = again;
						isDeviceConnected = again;
						Port = String.Empty;
					}
					else
					{
                        serialDetectingTimer.Stop();
						result = true;
					}
				}
			} while (again);

			return result;
		}


		private List<string> GetPortsList()
		{
			portList = new List<string>();

			string[] ports = SerialPort.GetPortNames();

			foreach (string port in ports)
				portList.Add(port);

			portList.Sort(SortUtil.ComPortCompare);

			return portList;
		}


		private bool PortIsPresentCheck()
		{
			List<string> portList = new List<string>();

			string[] ports = SerialPort.GetPortNames();
			foreach (string port in ports)
				portList.Add(port);

			if ((Port != null) && (portList.Contains(Port)))
				return true;
			return false;
		}


		private bool AttemptToOpenPort(string portName, bool savePort = true)
		{
			SerialPort newPort = null;
			try
			{
				newPort = new SerialPort();
				if (portName == null)
					throw new ArgumentNullException("portName");
				newPort.PortName = portName;
				serialPortConfig.Apply(newPort);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			try
			{
				newPort.Open();

			}
			catch (Exception)
			{
				newPort.Dispose();
				newPort = null;
			}

			if (newPort != null)
				SetupNewPort(newPort, savePort);

			return (newPort != null);
		}


		private void SetupNewPort(SerialPort newPort, bool savePort = true)
		{
			if (newPort == null)
				throw new ArgumentNullException("newPort");

			CloseExistingPort();
			RegisterNewPort(newPort);
			ui_.PortIsOpen = true;
		}


		private void RegisterNewPort(SerialPort newPort)
		{
            serialPort = newPort ?? throw new ArgumentNullException("newPort");
			HookPort();
		}


		private void CloseExistingPort()
		{
			if (serialPort == null)
				return;  // EARLY RETURN!

			UnhookPort();

			try
			{
				serialPort.Dispose();
			}
			catch (IOException)
			{ }  // port may have been closed already (e.g., USB cable disconnected)

			serialPort = null;
		}


		private void HookPort()
		{
			if (serialPort == null)
				throw new InvalidOperationException("No saved port to hook");

			serialPort.DataReceived += serial_DataReceived;
		}


		private void UnhookPort()
		{
			if (serialPort != null)
				serialPort.DataReceived -= serial_DataReceived;
		}


		private void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
            try
            {
                int bytesToRead = serialPort.BytesToRead;

                if (bytesToRead == 0)
                {
                    return;
                }

                // Only consume as many bytes as will fit into our local buffer.
                if (bytesToRead > inBuf_.Length)
                    bytesToRead = inBuf_.Length;

                uint bytesIn = (uint)serialPort.Read(inBuf_, 0, bytesToRead);

                readMgr_.NewData(inBuf_, 0, bytesIn);

                //Logs -------- remove!
                //StringBuilder s = new StringBuilder();
                //s.AppendLine(DateTime.UtcNow.ToString("yyyy/MM/dd HH-mm-ss") + " PM");
                //for (int i = 0; i < bytesToRead; i++)
                //{ 
                //    if (inBuf_[i] == 170)
                //        s.AppendLine();
                //    s.Append(inBuf_[i] + " ");
                //}
                //s.AppendLine();
                //s.AppendLine();
                //File.AppendAllText(metromDir + "rawData.log", s.ToString());
            }
            catch (Exception ex) { }
		}


		private void readMgr_NewPacket(byte[] buf, ushort ofs, ushort len)
		{
			try
			{
                serialListeningTimer.Stop();
                serialListeningTimer.Start();
                serialDetectingTimer.Stop();

                if (buf[2] == 56)
				    len = 235;

                TransportProtocol.ValidatePacket(buf, ofs, len);

				byte seqNo = 0;
				byte opcode = 0;

				TransportProtocol.GetPacketHeaderInfo(buf, ofs, out seqNo, out opcode);

				if (lastInSeqNo_ != 0xffff)
				{
					byte checkSeqNo = (byte)(lastInSeqNo_ + 1);

					if (seqNo != checkSeqNo)
						Debug.WriteLine("########## Msg sequence error: expected {0}, got {1} ##########", checkSeqNo, seqNo);
				}
				lastInSeqNo_ = seqNo;
				auraMsgMgr_.DispatchMessage(buf, ofs, len);
			}
			catch (Exception ex)
			{
			}
		}


        public void SendMgmtMsg(AppMessage msg)
		{
			try
			{
                ushort msgFullLen = TransportProtocol.GetMessagePacketLength(msg);
                TransportProtocol.PackMessagePacket(++outSeqNo_, outBuf_, msg);
                Thread.Sleep(10);
                serialPort.Write(outBuf_, 0, msgFullLen);

                //Logs ------- remove!
                //StringBuilder s = new StringBuilder();
                //s.AppendLine(DateTime.UtcNow.ToString("yyyy/MM/dd HH-mm-ss") + " Tablet");
                //for (int i = 0; i < (int)msgFullLen; i++)
                //{
                //    if (outBuf_[i] == 170)
                //        s.AppendLine();
                //    s.Append(inBuf_[i] + " ");
                //}
                //s.AppendLine();
                //s.AppendLine();
                //File.AppendAllText(metromDir + "rawData.log", s.ToString());
            }
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


		private void StartNewSession()
		{
			try
			{
				devInfo_.Reset();

				//if (serialPortOpen)
				//Mouse.OverrideCursor = Cursors.Wait;

				devInfo_.ConnectSequenceStarting();

				SendMgmtMsg(new Msg_UIRequestPMVersion());
				SendMgmtMsg(new Msg_UIGetCEMConfigADU());
				SendMgmtMsg(new Msg_UIGetUIMConfig());
				//SendMgmtMsg(new Msg_UIGetCEMConfig());
				SendMgmtMsg(new Msg_UIADUConnect());
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));
				SendMgmtMsg(new Msg_UIData(UI_Enums.UIA_Primitive.UIA_BTN_CANCEL_PRESSED));

				if (LogInPage.machineType == "HCTREAS")
				{
					ConfigMachineNodes("hctreasConfig.json");
				}
				else if (LogInPage.machineType == "JUPITER")
				{
					ConfigMachineNodes("jupiterConfig.json");
				}

				SendMgmtMsg(new Msg_UIGetCEMConfigADU());

                serialListeningTimer.Start();
            }
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


        private void SerialListeningTimer_Tick(object sender, EventArgs e)
        {
            serialDetectingTimer.Start();
            serialListeningTimer.Stop();
        }


        private void SerialDetectingTimer_Tick(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
                serialDetectingTimer.Stop();
            ConfigSerial();         
        }


        private void ReleaseMemoryTimer_Tick(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            releaseMemoryTimer.Stop();
            releaseMemoryTimer.Start();
            Status = "GL..............";
        }

        public void ConfigMachineNodes(string jsonPath)
		{
			try
			{
                using (StreamReader r = new StreamReader(metromDir + jsonPath))
				{
					string json = r.ReadToEnd();
					List<MachineConfig> mConfigList = JsonConvert.DeserializeObject<List<MachineConfig>>(json);

					machineConfig = new MachineConfig(mConfigList.First());
                    //menuManager.DeviceInfo.Name = machineConfig.MachineName;

					SendMgmtMsg(new Msg_UISetMachineName(machineConfig.MachineName, machineConfig.Nodes.Count));

					foreach (Node n in machineConfig.Nodes)
					{
						foreach (NodePin p in n.Pin)
						{
							SendMgmtMsg(new Msg_UIConfigInput(n, p, n.Pin.Count()));
						}
					}

					SendMgmtMsg(new Msg_UIConfigEncoder(machineConfig.EncoderResolution, machineConfig.WheelDiameter, 
														machineConfig.Direction, machineConfig.ClarDistance));
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


		public void SetAppSettings()
		{
			try
			{
                using (StreamReader r = new StreamReader(metromDir + "AURASettings.json"))
				{
					string json = r.ReadToEnd();
					List<AppSettings> settingsList = JsonConvert.DeserializeObject<List<AppSettings>>(json);
					appSettings = new AppSettings(settingsList.First());
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


		private void ConnectSequenceCompleted()
		{
			Mouse.OverrideCursor = null;

			if (!isDeviceConnected)
			{
				portIndex++;
				if (portIndex < GetPortsList().Count)
                { 
					serialPortOpen = false;

					//if ((Properties.Settings.Default.Port == null) ||
					//	(Properties.Settings.Default.Port.Length == 0) ||
					//	(serialPort == null))
					//{
					if (!ConfigSerialUntilPortGoodOrCancel())
					{
						return;
					}
					//}

					devInfo_.ConnectSequenceComplete = ConnectSequenceCompleted;

					StartNewSession();
				}
				else
				{
					MessageBox.Show(string.Format("No AURA device detected"), "Connect Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					serialPortOpen = false;
				}
			}
			else
			{
				serialPortOpen = true;

				Properties.Settings.Default.Port = serialPort.PortName;
				Properties.Settings.Default.Save();
			}
		}

		#endregion SerialPort


		//private void LoadAURAFullProfile()
		//{
		//	string srcFileName = null;
		//	AURAFullProfile fp = ProfileIO.GetFullProfileFromFile(ref srcFileName);

		//	if (fp == null)
		//		return;  // EARLY RETURN!

		//	fullProfileToApply_ = fp;

		//	if (fullProfileToApply_ != null)
		//	{
		//		CEMConfig cemConfig_ = new CEMConfig();
		//		CEMInternals cemInternals_ = new CEMInternals();
		//		UIMConfig uimConfig_ = new UIMConfig();

		//		//if (devInfo_.ConfigVersion == null) devInfo_.ConfigVersion = ConfigVersion;

		//		FullProfileMapper mapper = new FullProfileMapper(cemConfig_, cemInternals_, uimConfig_, devInfo_.ConfigVersion);

		//		// Apply the profile data to the separate data objects and send the new configuration
		//		// to the device.
		//		mapper.ApplyProfile(fullProfileToApply_);

		//		SendMgmtMsg(new Msg_UISetCEMConfigADU(cemConfig_));
		//		SendMgmtMsg(new Msg_UISetCEMInternals(cemInternals_));
		//		SendMgmtMsg(new Msg_UISetUIMConfig(uimConfig_));
		//		SendMgmtMsg(new Msg_CEMHello());

		//		// Update DeviceInfo fields.
		//		devInfo_.NoteCEMConfigChange(cemConfig_);

		//		StartNewSession();

		//	}

		//	//MessageBox.Show("Active Config Template: " + System.IO.Path.GetFileName(srcFileName));
		//	profile = System.IO.Path.GetFileNameWithoutExtension(srcFileName);
		//}


		private void FlashON(bool on)
		{
			string flashColor = "#ff3f00", normColor = "#EEDB67";
			var converter = new System.Windows.Media.BrushConverter();
			var flashbrush = (System.Windows.Media.Brush)converter.ConvertFromString(flashColor);
			var normbrush = (System.Windows.Media.Brush)converter.ConvertFromString(normColor);

			if (on)
			{
				frontwardPath.Fill = flashbrush;
				frontwardFirstBorder.Background = flashbrush;
				frontwardLastBorder.BorderBrush = flashbrush;
				backwardPath.Fill = flashbrush;
				backwardFirstBorder.Background = flashbrush;
				backwardLastBorder.BorderBrush = flashbrush;
			}
			else
			{
				frontwardPath.Fill = normbrush;
				frontwardFirstBorder.Background = normbrush;
				frontwardLastBorder.BorderBrush = normbrush;
				backwardPath.Fill = normbrush;
				backwardFirstBorder.Background = normbrush;
				backwardLastBorder.BorderBrush = normbrush;
			}
		}


		private bool flashOn = false;
		private void flashTimer_Tick(object sender, EventArgs e)
		{
			FlashON(flashOn);
			flashOn = !flashOn;
		}


        void Idle_Tick(object sender, EventArgs e)
        {
            if (dlgViewLog2_ == null && fwUpdatePopup == null)
            {
                MetromVisible = Visibility.Hidden;
                BtnHiddenVisible = Visibility.Visible;
                //MainWindow.mainWindow.WindowStyle = WindowStyle.None;
                WindowsAccess(false);
            }
            else
            {
                mIdle.IsEnabled = false;
                mIdle.IsEnabled = true;
            }
        }


        void Idle_PreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            //mIdle.IsEnabled = false;
            //mIdle.IsEnabled = true;
        }


        private void GetMemoryUsage()
		{
			using (Process proc = Process.GetCurrentProcess())
			{
				string procName = proc.ProcessName;
				var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				var ramCounter = new PerformanceCounter("Memory", "Available MBytes", String.Empty);
				var pageCounter = new PerformanceCounter("Paging File", "% Usage", "_Total");
				var counter = new PerformanceCounter("Process", "Working Set - Private", procName);
				//proc.Refresh();

				StringBuilder sb = new StringBuilder();
				sb.AppendLine(DateTime.Now.ToString("MM/dd/yyyy, HH:mm"));
				sb.AppendLine(String.Format("Processor: {0:##0} %", cpuCounter.NextValue()));
				sb.AppendLine(String.Format("Available Memory: {0} MB", ramCounter.NextValue()));
				sb.AppendLine(String.Format("Usage: {0:##0} %", pageCounter.NextValue()));
				sb.AppendLine(String.Format("Memory (Pivate Working Set): {0} K", counter.RawValue / 1024));
				sb.AppendLine();

                File.AppendAllText(metromDir + "memoryUsage.log", sb.ToString());
			}
		}


		public int NumberOfChecked()
		{
			int checkedCount = 0;
			foreach (CheckList list in LogTasks)
			{
				foreach (Task t in list.Tasks)
					if (t.IsChecked)
						checkedCount++;
			}
			return checkedCount;
		}


		private void DisplayMainLogs()
		{
			ViewCASLog(AURALog.Primary, false, 0);
			auraLog = AURALog.Primary;
		}

		private void DisplayRecentMainLogs()
		{
			ViewCASLog(AURALog.Primary, false, 200);
			auraLog = AURALog.Primary;
		}

		private void DisplayIncidentLogs()
		{
			ViewCASLog(AURALog.Secondary, false, 0);
			auraLog = AURALog.Secondary;
		}

		private void DisplayRecentIncidentLogs()
		{
			ViewCASLog(AURALog.Secondary, false, 200);
			auraLog = AURALog.Secondary;
		}


		private void UpdateApp()
		{
            //TODO update metromTablet app
			Application.Current.Shutdown();
		}


		private void UploadNow(object sender, EventArgs e)
		{
			Upload();
		}


		private void Upload()
		{
			if (auraLog == AURALog.Primary)
			{
				mainMetaFileName = logDir + "EV_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
				string fileName = dlgViewLog2_.FileName;
				Thread t = new Thread(() =>
				{
					lock (locker)
					{
						try
						{
							GenerateMetaFile(
								mainMetaFileName,
								machineConfig.MachineName,
								DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								LongData.CasEventLog, latitude, longitude,
								dlgViewLog2_.startTime,
								DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								fileName.Substring(fileName.LastIndexOf("\\") + 1),
								File.ReadAllBytes(fileName).Length);

							SendCasLogZip(machineConfig.MachineName, fileName, LongData.CasEventLog);

							Thread.Sleep(60000);

							//dlgViewLog2_.FileName.Clear();
						}
						catch { }
					}
				});
				t.IsBackground = true;
				t.Start();

				mainMetaFileName = logDir + "EV_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
				string blfFileName = dlgViewLog2_.BinFile;
				Thread th = new Thread(() =>
				{
					lock (locker)
					{
						try
						{
							GenerateMetaFile(
								mainMetaFileName,
								machineConfig.MachineName,
								DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								LongData.CasEventLog, latitude, longitude,
								dlgViewLog2_.startTime,
								DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								blfFileName.Substring(blfFileName.LastIndexOf("\\") + 1),
								File.ReadAllBytes(blfFileName).Length);

							SendCasLogZip(machineConfig.MachineName, blfFileName, LongData.CasEventLog);

							Thread.Sleep(60000);

							//dlgViewLog2_.BinFile.Clear();
						}
						catch { }
					}
				});
				th.IsBackground = true;
				th.Start();
			}
			else
			{
				incidentMetaFileName = logDir + "IN_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
				string fileName = dlgViewLog2_.FileName;
				Thread t = new Thread(() =>
				{
					lock (locker)
					{
						try
						{
							GenerateMetaFile(
								incidentMetaFileName,
								machineConfig.MachineName,
								DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								LongData.CasIncidentLog, latitude, longitude,
								dlgViewLog2_.startTime,
								DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								fileName.Substring(fileName.LastIndexOf("\\") + 1),
								File.ReadAllBytes(fileName).Length);

							SendCasLogZip(machineConfig.MachineName, fileName, LongData.CasIncidentLog);

							Thread.Sleep(60000);
						}
						catch { }
						//dlgViewLog2_.FileName.Clear();
					}
				});
				t.IsBackground = true;
				t.Start();
			}
		}


		AURALog auraLog = AURALog.Primary;

		private void ViewCASLog(AURALog auraLog, bool daily, uint limit)
		{
			try
			{
				//uint limit = 200;
				if (limit != 0)
				{
					if (limit > 500)
						limit = 500;
				}

				dlgViewLog2_ = (limit == 0) ? new ViewLog2Dlg(this, auraLog, daily) : new ViewLog2Dlg(this, auraLog, limit);
				dlgViewLog2_.Owner = MainWindow.mainWindow;
				//dlgViewLog2_.Closed += new EventHandler(dlgViewLog2__Closed);
				//dlgViewLog2_.OnUpload += new ViewLog2Dlg.UploadHandler();
				dlgViewLog2_.OnUpload += UploadNow;
				dlgViewLog2_.Closed += (s, e2) =>
				{
					dlgViewLog2_ = null;
					mIdle.IsEnabled = false;
					//mIdle.Stop();
					mIdle.IsEnabled = true;
					//mIdle.Start();
				};

				if (devInfo_.GotCEMHello)
				{
					dlgViewLog2_.OverrideVehicleName(devInfo_.MachineName);
					if (devInfo_.CEMVersion.IsValid)
						dlgViewLog2_.OverrideCEMVersion(devInfo_.CEMVersion.ToString());
				}

				if (devInfo_.GotUIMHello)
				{
					if (devInfo_.UIMVersion.IsValid)
						dlgViewLog2_.OverrideUIMVersion(devInfo_.UIMVersion.ToString());
				}

				dlgViewLog2_.ShowDialog();
				canExec = false;
			}
			catch { }
		}



		private void GetCASLog(AURALog auraLog)
		{
			try
			{
				dlgViewLog2_ = new ViewLog2Dlg(this, auraLog, true);
				dlgViewLog2_.Owner = MainWindow.mainWindow;
				dlgViewLog2_.Closed += dlgViewLog2__Closed;


				if (devInfo_.GotCEMHello)
				{
					dlgViewLog2_.OverrideVehicleName(devInfo_.MachineName);
					if (devInfo_.CEMVersion.IsValid)
						dlgViewLog2_.OverrideCEMVersion(devInfo_.CEMVersion.ToString());
				}

				if (devInfo_.GotUIMHello)
				{
					if (devInfo_.UIMVersion.IsValid)
						dlgViewLog2_.OverrideUIMVersion(devInfo_.UIMVersion.ToString());
				}

				dlgViewLog2_.Show();
				canExec = true;

			}
			catch { }
		}

		

        void dlgViewLog2__Closed(object sender, EventArgs e)
		{
			if (canExec)
			{
				if (eventlog)
				{
					//DateTime dtFrom = Properties.Settings.Default.MainLogSentTime == "0001-01-01 00:00:00"
					//		? DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss")
					//		: Properties.Settings.Default.MainLogSentTime,
					if (!sentEventZip)
					{
						//for (int i = 0; i < dlgViewLog2_.FileName.Count; i++)
						//{

						mainMetaFileName = logDir + "EV_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
						string fileName = dlgViewLog2_.FileName;
						Thread t = new Thread(() =>
						{
							lock (locker)
							{
								try
								{
									GenerateMetaFile(
										mainMetaFileName,
										machineConfig.MachineName,
										DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
										LongData.CasEventLog, latitude, longitude,
										dlgViewLog2_.startTime,
										DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
										fileName.Substring(fileName.LastIndexOf("\\") + 1),
										File.ReadAllBytes(fileName).Length);

									SendCasLogZip(machineConfig.MachineName, fileName, LongData.CasEventLog);

									Thread.Sleep(60000);
								}
								catch { }
							}
						});
						t.IsBackground = true;
						t.Start();
						//t.Join();
						//Thread.Sleep(60000);
						//}
						//for (int i = 0; i < dlgViewLog2_.BinFile.Count; i++)
						//{
						mainMetaFileName = logDir + "EV_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
						string binFileName = dlgViewLog2_.BinFile;
						Thread th = new Thread(() =>
						{
							lock (locker)
							{
								try
								{
									GenerateMetaFile(
										mainMetaFileName,
										machineConfig.MachineName,
										DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
										LongData.CasEventLog, latitude, longitude,
										dlgViewLog2_.startTime,
										DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
										binFileName.Substring(binFileName.LastIndexOf("\\") + 1),
										File.ReadAllBytes(binFileName).Length);

									SendCasLogZip(machineConfig.MachineName, binFileName, LongData.CasEventLog);

									Thread.Sleep(60000);
								}
								catch { }
							}
						});
						th.IsBackground = true;
						th.Start();
						//t.Join();
						//Thread.Sleep(60000);
						//}
						//dlgViewLog2_.BinFile.Clear();

						sentEventZip = true;

						Properties.Settings.Default.MainLogSentTime =
						DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						Properties.Settings.Default.Save();
					}
					//dlgViewLog2_ = null;
				}

				else
				{
					//DateTime dtFrom = Properties.Settings.Default.IncidentLogSentTime == "0001-01-01 00:00:00"
					//		? DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss")
					//		: Properties.Settings.Default.IncidentLogSentTime;
					if (!sentIncidentZip)
					{
						//for (int i = 0; i < dlgViewLog2_.FileName.Count; i++)
						//{
						incidentMetaFileName = logDir + "IN_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
						string fileName = dlgViewLog2_.FileName;
						Thread t = new Thread(() =>
						{
							lock (locker)
							{
								try
								{
									GenerateMetaFile(
										incidentMetaFileName,
										machineConfig.MachineName,
										DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
										LongData.CasIncidentLog, latitude, longitude,
										dlgViewLog2_.startTime,
										DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
										fileName.Substring(fileName.LastIndexOf("\\") + 1),
										File.ReadAllBytes(fileName).Length);

									SendCasLogZip(machineConfig.MachineName, fileName, LongData.CasIncidentLog);

									Thread.Sleep(60000);
								}
								catch { }
							}
						});
						t.IsBackground = true;
						t.Start();

						//t.Join();

						//Thread.Sleep(60000);
						//}
						sentIncidentZip = true;
					}
					//dlgViewLog2_ = null;
				}
				//dlgViewLog2_ = null;
				//eventlog = !eventlog;
			}
		}


		private void ShowCalib()
		{
			if (calib != null)
			{
				calib.Close();
				calib = null;
			}

			calib = new CalibDataPopUp();
			calib.Owner = MainWindow.mainWindow;
			calib.Show();

			SendMgmtMsg(new Msg_UIGetCEMAccelCalibData());
		}


        private void btnUpdateApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void Page_TouchUp(object sender, TouchEventArgs e)
        {
            mIdle.IsEnabled = false;
            mIdle.IsEnabled = true;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigSerial();
        }

        #region IViewLogDlgHost Implementation


        public void ViewLog_UIFeedback(string text)
		{
			//MessageBox.Show(text);
		}


		public void ViewLog_SendGetLogStatus()
		{
			SendMgmtMsg(new Msg_UIGetLogStatus());
		}


		public void ViewLog_SendGetLogEntry(AURALog whichLog, uint entryIndex)
		{
			SendMgmtMsg(new Msg_UIGetLogEntry((byte)whichLog, entryIndex));
		}


        #endregion IViewLogDlgHost Implementation

        #endregion Methods

	}
}
