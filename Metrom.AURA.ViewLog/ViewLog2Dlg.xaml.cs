//#define DEBUG_TIMEOUT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using IDI.Diagnostics;
using Metrom.AURA.Base;
using Metrom.AURA.Log;

using OfficeOpenXml;
using Metrom.AURA.Messaging;
using IDI.Logging;
using System.Windows.Interop;
using System.Windows.Threading;


namespace Metrom.AURA.ViewLog
{


  /// <summary>
  /// Interaction logic for ViewLog2Dlg.xaml
  /// </summary>
  /// 
  public partial class ViewLog2Dlg : Window
  {
    #region Types

    /// <summary>
    /// 
    /// </summary>
    /// 
    public enum MsgSource
    {
      USB,
      OTA
    }


    /// <summary>
    /// Download state.
    /// </summary>
    /// 
    private enum DLState
    {
      InvalidStateValue = 0,

      QueryStatus,
      GetEntry,
      ScanHead,
      ScanTail,
      UIRangeEntry,
      ProbeStart,
      ProbeEnd,
      Done
    };

    #endregion

    #region Constants

    private const int kUSBMsgTimeout = 2000;
    private const int kUSBRetryLimit = 3;

    private const int kOTAMsgTimeout = 200;
    private const int kOTARetryLimit = 20;

    private const uint kInvalidCount = 0xffffffff;
    private const uint kInvalidIndex = 0xffffffff;

    private const uint kTailAdjustThreshold = 128;
    private const uint kAutoDownloadLogCountThreshold = 200;

    #endregion

    #region Instance Fields

    // ===== Official instance fields (ones we're keeping and new ones):

    /// <summary>
    /// Holds a reference to the IViewLogDlgHost host object (likely a window, but not required).
    /// </summary>
    /// 
    private WeakReference host_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private DLState state_ = DLState.InvalidStateValue;

    /// <summary>
    /// Message send timeout timer (regardless of communication channel).
    /// </summary>
    /// 
    private TimeoutTimerLocal tmoTimer_ = new TimeoutTimerLocal();

    /// <summary>
    /// Current message send try count.
    /// </summary>
    /// 
    private int tryCount_;

    /// <summary>
    /// Total message send try limit.
    /// </summary>
    /// 
    private readonly int tryLimit_;

    /// <summary>
    /// The source of messages (communication method).
    /// </summary>
    /// 
    private MsgSource msgSource_ = MsgSource.USB;

    /// <summary>
    /// Which log we're interacting with (Main / primary or Incident / secondary).
    /// </summary>
    /// 
    private AURALog whichLog_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private LogNavigator logNav_;

    /// <summary>
    /// Current logical index to fetch from the log.
    /// </summary>
    /// 
    private uint curFetchNdx_;

    /// <summary>
    /// Ordinal number (zero based) of current entry being fetched from the log.
    /// </summary>
    /// 
    private uint curFetchOrdinal_;
    
    /// <summary>
    /// The last logical index to be fetched from the log.
    /// </summary>
    /// 
    private uint endingFetchNdx_;

    /// <summary>
    /// The current physical index being fetched from the log; used ONLY by retry code.
    /// </summary>
    /// 
    private uint curGetLogEntryNdx_ = kInvalidIndex;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private AURALogEntry goodHeadDateEntry_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint goodHeadDateNdx_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private AURALogEntry goodTailDateEntry_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint goodTailDateNdx_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private DateTime startDate_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private DateTime endDate_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint probeHeadNdx_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint probeTailNdx_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private DateTime targetDate_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint probeTestLeftNdx_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint probeTestRightNdx_;

    /// <summary>
    /// 
    /// </summary>
    /// 
    private bool probeTestAdjustMovesLeft_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint probeCandidateNdx_;

    /// <summary>
    /// Logical index.
    /// </summary>
    /// 
    private uint downloadStartNdx_ = kInvalidIndex;

    /// <summary>
    /// The list of downloaded log entries.
    /// </summary>
    /// 
    private List<AURALogEntry> entries_ = new List<AURALogEntry>();

    /// <summary>
    /// 
    /// </summary>
    /// 
    private Dictionary<uint, AURALogEntry> cache_ = new Dictionary<uint, AURALogEntry>();

    private uint tailAdjustThreshold_;

    private uint autoDownloadLogCountThreshold_;

    #region Development and Testing Only

    private bool logIsSimulated_;

    private uint simLogCapacity_;

    private uint simLogHead_;

    private uint simLogTail_;

    private uint simLogExpectedCount_;

    private DateTime simLogCurrentTimestamp_ = DateTime.MaxValue;

    private uint simLogCurrentIndex_;

    private uint entryLimit_;

    private bool useEntryLimit_ = false;

#if HOLD
    private string localName_ = "UnknownID";
    private bool foundLocalName_ = false;

    private string uimVersion_ = "Unknown";
    private bool foundUimVersion_ = false;

    private string cemVersion_ = "Unknown";
    private bool foundCemVersion_ = false;
#endif

    private LogSourceInfo infoForExport_ = new LogSourceInfo();

    private LogSourceInfo info_ = new LogSourceInfo();

    private bool dataWasImported_ = false;

    private CancellationTokenSource cancelExport_;

    private uint totalEntriesToDownload_ = 0;

    private DateTime lastPercentUpdate_ = DateTime.Now;

	private bool hidden_;

    private string logDir = @"C:\METROM\Logs\"; 

	public string FileName = String.Empty;// = new List<string>();

	public string BinFile = String.Empty;// = new List<string>();

	private int counter = 0;

	public string startTime;

	private DispatcherTimer mIdle;


    #endregion

#if DEBUG_TIMEOUT
    private bool wasTriggered_ = false;
    private int testLoopCount_ = 0;
#endif

    #endregion

    #region Properties

    /// <summary>
    /// 
    /// </summary>
    /// 
    private DLState State
    {
      get { return state_; }
      set
      {
        if (state_ != value)
        {
          Debug.WriteLine("VL2: [{0}]", value);

          state_ = value;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// 
    public bool DemoMode
    { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// 
    public bool IncidentLogIsGen1
    { get; set; }

    #endregion

    #region Lifetime Management

    /// <summary>
    /// Ctor; initializes a new instance of the ViewLog2Dlg dialog, to download all or a fixed
    /// number of log entries, or entries within a date range (depending on user choices).
    /// </summary>
    /// <param name="host"></param>
    /// <param name="which"></param>
    /// <param name="msgSource"></param>
    /// <param name="testLogFilename"></param>
    /// 
    public ViewLog2Dlg(IViewLogDlgHost host, AURALog which, bool hidden, MsgSource msgSource = MsgSource.USB, string testLogFilename = null)
    {
      // VL2: This is the new ctor. It's ready-to-go as it is, though additions may still be
      // made.

      // This instantiation of the View Log dialog will present the user with a date range
      // selection dialog (from which the user can also choose to download all or a fixed number
      // of entries instead of a date range) if there are more than a certain number of entries
      // in the targeted log - otherwise, all entries will be downloaded automatically.

      InitializeComponent();

	  //user inactivity
	  InputManager.Current.PreProcessInput += Idle_PreProcessInput;
	  mIdle = new DispatcherTimer();
	  mIdle.Interval = new TimeSpan(0, 0, 60);//appSettings.IdleIntervalSec);
	  mIdle.IsEnabled = true;
	  mIdle.Tick += Idle_Tick;

	  hidden_ = hidden;

	  if (hidden_)
	  {
		  MinHeight = 0;
		  MinWidth = 0;
		  Height = 0;
		  Width = 0;
		  this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
		  Left = -1000;
		  Top = -1000;
		  Visibility = System.Windows.Visibility.Hidden;
		  //autoDownloadLogCountThreshold_ = kAutoDownloadLogCountThreshold / 2;
	  }
	  //else
	  //{
		  autoDownloadLogCountThreshold_ = kAutoDownloadLogCountThreshold;
	  //}


      host_ = new WeakReference(host);

      tailAdjustThreshold_ = kTailAdjustThreshold;
    

      whichLog_ = which;
      msgSource_ = msgSource;
      DemoMode = false;

      Title = (which == AURALog.Primary) ? "View Main Log" : "View Incident Log";
      IncidentLogIsGen1 = false;  // not sure yet how this figures into the code...

	  startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      // Configure the timeout timer based on the underlying communication channel.

      tmoTimer_.Timeout = (msgSource == MsgSource.USB) ? kUSBMsgTimeout : kOTAMsgTimeout;
      tmoTimer_.Elapsed += (s, e) => { RetryOrFailMsg(); };  // will run on UI thread
      
      tryLimit_ = (msgSource == MsgSource.USB) ? kUSBRetryLimit : kOTARetryLimit;

      // Note that state_ will be set in the Loaded event handler, where we'll request the
      // first message to be sent.

      // Development / debug only:
#if DEBUG
      if (testLogFilename != null)
        SetupTestLogData(testLogFilename);
#endif
    }

	//public void InitHwnd()
	//{
	//	var helper = new WindowInteropHelper(this);
	//	helper.EnsureHandle();
	//}


    /// <summary>
    /// Ctor; initializes a new instance of the ViewLog2Dlg dialog which will download log data.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="which"></param>
    /// <param name="entryLimit">Maximum number of log entries to download; set to zero to
    /// download the entire log, or set to uint.MaxValue to enable the date-range download
    /// option.</param>
    /// <param name="msgSource">Indicates the underlying communication channel used for
    /// messaging (affects timeouts and retry counts).</param>
    /// 
    public ViewLog2Dlg(IViewLogDlgHost host, AURALog which, uint entryLimit, MsgSource msgSource = MsgSource.USB)
    {
      InitializeComponent();

	  //user inactivity
	  InputManager.Current.PreProcessInput += Idle_PreProcessInput;
	  mIdle = new DispatcherTimer();
	  mIdle.Interval = new TimeSpan(0, 0, 60);//appSettings.IdleIntervalSec);
	  mIdle.IsEnabled = true;
	  mIdle.Tick += Idle_Tick;

      host_ = new WeakReference(host);

      tailAdjustThreshold_ = kTailAdjustThreshold;
      autoDownloadLogCountThreshold_ = kAutoDownloadLogCountThreshold;

      whichLog_ = which;
      msgSource_ = msgSource;
      DemoMode = false;

      entryLimit_ = entryLimit;
      useEntryLimit_ = true;

      Title = (which == AURALog.Primary) ? "View Recent Main Log" : "View Recent Incident Log";
      IncidentLogIsGen1 = false;

      // Configure the timeout timer based on the underlying communication channel.

	  startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

      tmoTimer_.Timeout = (msgSource == MsgSource.USB) ? kUSBMsgTimeout : kOTAMsgTimeout;
      tmoTimer_.Elapsed += (s, e) => { RetryOrFailMsg(); };  // will run on UI thread

      tryLimit_ = (msgSource == MsgSource.USB) ? kUSBRetryLimit : kOTARetryLimit;
    }


    /// <summary>
    /// Ctor; initializes a new instance of the ViewLog2Dlg dialog for inspecting an imported log.
    /// </summary>
    /// <param name="data"></param>
    /// 
    public ViewLog2Dlg(ImportData data)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      InitializeComponent();

      whichLog_ = data.Header.WhichLog;
      DemoMode = false;

      Title = (whichLog_ == AURALog.Primary) ? "Imported Main Log" : "Imported Incident Log";
      IncidentLogIsGen1 = data.Header.IsGen1IncidentLog;

      dataWasImported_ = true;
	  startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

      // Save the incoming LogSourceInfo data.
      //
      // Note that we save the original incoming LogSourceInfo reference in originalInfo_ (only
      // used when dealing with imported data) but we make a copy of it to put into info_. We do
      // this because:
      //
      // 1) should the user choose to export the log data again after import, we don't want to
      //     write deduced name/versions data in the exported file's header (*); and
      //
      // 2) info_ potentially could be updated with deduced data and we don't want to
      //     inadvertently change its data while the user isn't looking.
      //
      // (*) - BLF rev. 1 didn't contain name/versions data in the header; if the imported data
      // came from a rev. 1 file, then when the user exports the data again, we fill in the
      // name/versions header fields with data from info_, which could be deduced (by the time
      // the export happens). We don't want to write deduced name/versions data in the header
      // because those fields are reserved for data that comes from a connected device. If we
      // did that, then afterward there would be no way of knowing that the machine name from
      // the header was deduced (and therefore *could* be incorrect).

      infoForExport_ = data.Info;
      info_ = new LogSourceInfo(data.Info);

      entries_ = data.Entries;

      tryLimit_ = 0;  // not used with imported log processing
    }
    
    #endregion

    #region UI Events

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      if (DemoMode)
      {
        // VL2: refactor

        AURALogEntry[] entries = AURALogEntry.ConstructDemoLogEntries(whichLog_);

        info_.LocalName = "DEMO012345";
        info_.UIMVersion = "1.00.02";
        info_.CEMVersion = "1.01.17";

        lblEntryCount_.Content = entries.Length.ToString();

        uint ndx = 0;
        foreach (AURALogEntry entry in entries)
        {
          AddLogEntry(entry, ndx, ndx);
          ++ndx;
        }
      }
      else
      {
        if (dataWasImported_)
        {
          // VL2: refactor

          if (entries_.Count == 0)
            lblLoadFeedback_.Content = "Log is empty";

          lblEntryCount_.Content = entries_.Count.ToString();

          uint ndx = 0;
          foreach (AURALogEntry entry in entries_)
          {
            AddLogEntry(entry, ndx, ndx, true);
            ++ndx;

            if (whichLog_ == AURALog.Primary)
            {
              if (entry.RawEntryType == (byte)CEMLogEntryType.Startup)
              {
                if (!info_.HasLocalName)
                  info_.LocalName = ((AURALogEntry_Startup)entry).Name;
                if (!info_.HasCEMVersion)
                  info_.CEMVersion = ((AURALogEntry_Startup)entry).VersionString;
              }
              else if ((entry.RawEntryType == (byte)CEMLogEntryType.UIMStartup) && !info_.HasUIMVersion)
                info_.UIMVersion = ((AURALogEntry_UIMStartup)entry).VersionString;
            }
            else
            {
              // Log is secondary / incident.

              if (entry.RawEntryType == (byte)CEMLogEntryType.Incident)
              {
                if (!info_.HasLocalName)
                  info_.LocalName = ((AURALogEntry_Incident)entry).Name;
              }
            }
          }
        }
        else
        {
          // We're downloading live log entries (not in demo mode, not importing data).

          InitiateLogInteraction();
        }
      }
    }

	void Idle_Tick(object sender, EventArgs e)
	{
		this.Close();
	}


	void Idle_PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		mIdle.IsEnabled = false;
		mIdle.Stop();
		mIdle.IsEnabled = true;
		mIdle.Start();
	}


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if (cancelExport_ != null)
        cancelExport_.Cancel();

      if (!dataWasImported_)
      {
        tmoTimer_.Stop();

#if DEBUG_TIMEOUT
//        if (!wasTriggered_)
//          ((MainWindow)Owner).SendMgmtMsg(new Msg_MgmtUIPCControl(1));  // disable data capture in UIM and report
#endif
      }
    }

	//public static string FN;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void btnSaveLog_Click(object sender, RoutedEventArgs e)
    {
      lblLoadFeedback_.Content = "";

      try
      {
        if (entries_.Count == 0)
        {
			MessageBox.Show("The log is empty.", "No Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;  // EARLY RETURN!
        }

        var dlg = new SaveFileDialog();

        dlg.DefaultExt = ".xlsx";
        dlg.Filter = "Log file|*.xlsx";

        // Generate a filename suggestion for the user.
        //
        // We wish to include in the suggestion the name of the machine from which the log was
        // downloaded. However:
        //
        //   1) the {UIM,CEM}Hello messages (from which explicit name/version data comes) did
        //     not exist prior to UIM 2.15.3 / CEM 2.12.3 (*);
        //
        //   2) the rev. 1 BLF file header (prior to ADU/ACU 2.0.0) did not record name/version
        //     data.
        //
        // [(*) - these messages were actually introduced in UIM 2.14.0 / CEM 2.11.0, but those
        // versions were never official release points. UIM 2.15.3 / CEM 2.12.3 are a released
        // pair.]
        //
        // Originally, the exported log spreadsheet didn't contain explicit (i.e., called-out)
        // name/version data - that information could be found by looking at Startup and
        // UIMStartup entries - and only contained a single worksheet.
        //
        // Then, an "Alarms" worksheet was added to present filtered alarm-only data (an export
        // from the ADU resulted in a spreadsheet with two tabs, "Main Log" and "Alarms"; an
        // export from the ACU, however, contained only the "Alarms" worksheet; exporting from
        // either the ADU or ACU also created (and still creates) a BLF file containing all raw
        // log data).
        //
        // When the Alarms worksheet was added (or shortly thereafter), we realized it would be
        // extremely useful to have the local machine name and current firmware version numbers
        // called out explicitly at the top of the worksheet, since after the elimination of the
        // Main Log worksheet from spreadsheets exported using the ADU, customers no longer had
        // any way of seeing that information themselves.
        //
        // Since the name/version data wasn't available explicitly at that time, we decided we
        // could simply scan for and pick out the relevant data from the log entries themselves
        // (scanning in reverse chronological order, of course, so that the most recent data
        // were chosen).
        //
        // This worked splendidly... unless the user chose to download only a portion of the
        // log data (and just happened to miss a startup, in which case neither name nor versions
        // might be known; or got a startup but in older data, in which case incorrect versions
        // might be deduced, possibly even an incorrect name), or downloaded an Incident log.
        //
        // Now, we have the LogSourceInfo class, which holds name/versions data. Two instances
        // exist: info_ and infoForExport_. Both are initially empty. If a "live" download is
        // being done, calling code may call Override{VehicleName,{UIM,CEM,RCM}Version}()
        // methods to indicate known data (that is, data from the connected device that is known
        // because the device's firmware supports the {UIM,CEM}Hello messages), and those
        // methods update both info_ and infoForExport_. If an import is being done, we'll get
        // a LogSourceInfo instance inside the ImportData instance passed to our
        // import-specific ctor, and we save its reference in infoForExport_ and save a copy
        // in info_. Note that in the import case, the LogSourceInfo will contain data from the
        // header of the imported BLF file; for BLF rev. 1, it will be empty because
        // name/versions data were added in BLF rev. 2.
        //
        // During log entry processing (either during actual log entry download or after an
        // import), if one or more name/versions fields are unknown, the code will deduce the
        // missing data from Startup and UIMStartup log entries and record them in the info_
        // instance.
        //
        // If the user chooses to export log data that came from an import operation, and the
        // version of the ADU/ACU being used is 2.0.0+, we want to make sure that the new BLF
        // file (which will be created along with the spreadsheet file) contains in its header
        // the same name/versions data as the original file. Even though in most cases it ends
        // up working out fine, if the source file is BLF rev. 1, we don't want the deduced
        // name/versions data finding their way into the new BLF rev. 2+ header. This is mainly
        // because these data in the header are meant to imply something "known" about the log
        // data - that is, where the data came from (what machine). Deduced data potentially
        // could have been deduced wrong (missed startup, or a startup from the past).

        // At this point in time, our info_ instance will contain a LocalName if:
        //   - data was just downloaded from a connected device that supports the
        //      {UIM,CEM}Hello messages; or
        //   - data was just downloaded from a connected device that does not support the
        //      {UIM,CEM}Hello messages, but was deduced from the newest Startup/UIMStartup
        //      log entries in the downloaded range (whether or not that was correct, please
        //      note!); or
        //   - data was imported from a BLF rev. 2 file and the name field in the header was
        //      set; or
        //   - data was imported from a BLF rev. 1 file, or from a rev. 2 file with an
        //      unspecified name field.
        //
        // The machine name in the suggested filename will be set to the LocalName in our info_
        // instance if present; otherwise, UnknownID is used. Note that we make this choice here
        // explicitly, rather than using the default value of info_.LocalName (which is
        // "<unknown>" when info_.HasLocalName is false, and the greater-than / less-than
        // symbols may not appear in file or directory names.

        string localName = info_.HasLocalName ? info_.LocalName : "UnknownID";
        dlg.FileName = string.Format("{0:yyyyMMdd}_{0:HHmm}_{1}_{2}", DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident");
		//FN = dlg.FileName + ".xlsx";
        if (!dlg.ShowDialog().GetValueOrDefault())
          return;  // EARLY RETURN!

        if (File.Exists(dlg.FileName))
        {
          try
          {
            File.Delete(dlg.FileName);
          }
          catch (IOException)
          {
            MessageBox.Show("The selected file cannot be accessed (is it open in another application?).", "File Access Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;  // EARLY RETURN!
          }
        }

        btnSaveLog_.IsEnabled = false;
        btnSaveLogCSV_.IsEnabled = false;
		btnUploadNow.IsEnabled = false;

        Mouse.OverrideCursor = Cursors.Wait;

        cancelExport_ = new CancellationTokenSource();

        Task exportTask = Task.Factory.StartNew(() => ExportLogData(cancelExport_.Token, whichLog_, entries_, infoForExport_, dlg.FileName), cancelExport_.Token);
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }


	private void SaveLogXlsx()
	{
		lblLoadFeedback_.Content = "";
		//FileName.Clear();
		//BinFile.Clear();
		try
		{
			if (entries_.Count == 0)
			{
				if (!hidden_)
				MessageBox.Show("The log is empty.", "No Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;  // EARLY RETURN!
			}

			string localName = info_.HasLocalName ? info_.LocalName : "UnknownID";
			//dlg.FileName = string.Format("{0:yyyyMMdd}_{0:HHmm}_{1}_{2}", DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident");
			//FileName = System.Environment.CurrentDirectory + string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}.xlsx", DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident");
			//string chunkFileName = System.Environment.CurrentDirectory + string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}", DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident");
			////if (!dlg.ShowDialog().GetValueOrDefault())
			//	return;  // EARLY RETURN!

			//if (File.Exists(FileName))
			//{
			//	try
			//	{
			//		File.Delete(FileName);
			//	}
			//	catch (IOException)
			//	{
			//		if (!hidden_)
			//		MessageBox.Show("The selected file cannot be accessed (is it open in another application?).", "File Access Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			//		return;  // EARLY RETURN!
			//	}
			//}

			btnSaveLog_.IsEnabled = false;
			btnSaveLogCSV_.IsEnabled = false;
			btnUploadNow.IsEnabled = false;

			Mouse.OverrideCursor = Cursors.Wait;

			cancelExport_ = new CancellationTokenSource();

			if (whichLog_ == AURALog.Secondary)
			{
				//if (entries_.Count > 50)
				//{
				//	List<AURALogEntry> chunkEntries = new List<AURALogEntry>();
				//	int chunkCount = entries_.Count / 50;
				//	int remainingEntries = entries_.Count % 50;
				//	for (int i = 0; i < chunkCount; i++)
				//	{
				//		for (int j = 0; j < 50; j++)
				//		{
				//			chunkEntries.Add(entries_[j + 50 * i]);
				//		}
				//		FileName.Add(System.Environment.CurrentDirectory +
				//			string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}" + "_" + i.ToString() + ".xlsx",
				//			DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident"));
				//		//FileName[i] += "_" + i.ToString() + ".xlsx";
				//		Task exportTask = Task.Factory.StartNew(()
				//			=> ExportLogData(cancelExport_.Token, whichLog_, chunkEntries, infoForExport_, FileName[i]), cancelExport_.Token);
				//		Thread.Sleep(2000);
				//		chunkEntries.Clear();
				//	}
				//	for (int j = 0; j < remainingEntries; j++)
				//	{
				//		chunkEntries.Add(entries_[j + 50 * chunkCount]);
				//	}
				//	FileName.Add(System.Environment.CurrentDirectory +
				//			string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}_" + chunkCount.ToString() + ".xlsx",
				//			DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident"));
				//	//FileName[chunkCount] += "_" + chunkCount.ToString() + ".xlsx");

				//	Task exportRemainingTask = Task.Factory.StartNew(()
				//		=> ExportLogData(cancelExport_.Token, whichLog_, chunkEntries, infoForExport_,
				//		FileName[chunkCount]), cancelExport_.Token);
				//	Thread.Sleep(2000);
				//	chunkEntries.Clear();
				//}
				//else
				//{
					//FileName.Add(System.Environment.CurrentDirectory + string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}.xlsx",
					//	DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident"));
					FileName = logDir + string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}.xlsx", DateTime.Now, localName, "Incident");
					Task exportTask = Task.Factory.StartNew(() => ExportLogData(cancelExport_.Token, whichLog_, entries_, infoForExport_, FileName), cancelExport_.Token);
				//}
			}
			else
			{
				//FileName.Add(System.Environment.CurrentDirectory + string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}.xlsx",
				//		DateTime.Now, localName, "Main"));
				FileName = logDir + string.Format("\\{0:yyyyMMdd}_{0:HHmm}_{1}_{2}.xlsx", DateTime.Now, localName, "Main");
				Task exportTask = Task.Factory.StartNew(() => ExportLogData(cancelExport_.Token, whichLog_, entries_, infoForExport_, FileName), cancelExport_.Token);

			}

		}
		catch (Exception ex)
		{
			string[] msg = ExceptionUtil.ExceptionReport(ex);
			AppLogger.WriteToLog(msg);
		}
	}


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancelToken"></param>
    /// <param name="whichLog"></param>
    /// <param name="entries"></param>
    /// <param name="info"></param>
    /// <param name="fileName"></param>
    /// 
    private void ExportLogData(
      CancellationToken cancelToken,
      AURALog whichLog,
      List<AURALogEntry> entries,
      LogSourceInfo info,
      string fileName)
    {
      if (entries == null)
        throw new ArgumentNullException("entries");
      if (info == null)
        throw new ArgumentNullException("info");
      if (fileName == null)
        throw new ArgumentNullException("fileName");

      Exception savedEx = null;

      try
      {
        AURALogIO.WriteLogToExcelFile(whichLog, entries, info, fileName, (ph, pct) =>
          {
            Dispatcher.BeginInvoke((Action)(() =>
              {
                string text = "";

                switch (ph)
                {
                case 0:
                  text = string.Format("Main tab: {0:f1} % done...", 100.0 * pct);
                  break;

                case 1:
                case 3:
                  text = "Adjusting column widths...";
                  break;

                case 2:
                  text = string.Format("Activity tab: {0:f1} % done...", 100.0 * pct);
                  break;

                case 4:
                  text = "Saving XLSX file...";
                  break;
                }

                lblLoadFeedback_.Content = text;
              }));
          }, cancelToken);

        // FOR DEVELOPMENT TESTING!

        //string filepath = System.IO.Path.ChangeExtension(dlg.FileName, ".csv");
        //AURALogIO.WriteLogToCsvFile(whichLog_, entries_, localName_, filepath);

        // Prior to v. 1.16.2, only the ACU would save the main log as a binary image (in
        // addition to the simplified, filtered .xlsx spreadsheet). Starting with v. 1.16.2,
        // the ADU will save the main log as a binary image as well (in addition to the
        // usual unfiltered .xlsx spreadsheet).

        if (whichLog == AURALog.Primary)
        {
			//int limit = 1500;
			//if (entries.Count <= limit)
			//{
				//BinFile.Add(System.IO.Path.GetDirectoryName(fileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".blf");
				//BinFile = System.IO.Path.GetDirectoryName(fileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".blf";
			BinFile = logDir + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".blf";
				Dispatcher.BeginInvoke((Action)(() =>
				{
					lblLoadFeedback_.Content = "Writing binary (BLF) file...";
				}));

				AURALogIO.WriteLogToBinaryFile(whichLog, entries, info, IncidentLogIsGen1, BinFile);
			//}
			//else
			//{
					
			//	List<AURALogEntry> chunkEntries = new List<AURALogEntry>();
			//	int chunkCount = entries_.Count / limit;
			//	int remainingEntries = entries_.Count % limit;
			//	for (int i = 0; i < chunkCount; i++)
			//	{
			//		for (int j = 0; j < limit; j++)
			//		{
			//			chunkEntries.Add(entries_[j + limit * i]);
			//		}
			//		BinFile.Add(System.IO.Path.GetDirectoryName(fileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + i.ToString() + ".blf");
						
			//		Dispatcher.BeginInvoke((Action)(() =>
			//		{
			//			lblLoadFeedback_.Content = "Writing binary (BLF) file...";
			//		}));

			//		AURALogIO.WriteLogToBinaryFile(whichLog, chunkEntries, info, IncidentLogIsGen1, BinFile[i]);
					
			//		Thread.Sleep(2000);
			//		chunkEntries.Clear();
			//	}

			//	for (int j = 0; j < remainingEntries; j++)
			//	{
			//		chunkEntries.Add(entries_[j + limit * chunkCount]);
			//	}
			//	BinFile.Add(System.IO.Path.GetDirectoryName(fileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + chunkCount.ToString() + ".blf");

			//	Dispatcher.BeginInvoke((Action)(() =>
			//	{
			//		lblLoadFeedback_.Content = "Writing binary (BLF) file...";
			//	}));

			//	AURALogIO.WriteLogToBinaryFile(whichLog, chunkEntries, info, IncidentLogIsGen1, BinFile[chunkCount]);

			//	Thread.Sleep(1000);
			//	chunkEntries.Clear();
			//}
        }
      }
      catch (OperationCanceledException)
      {
        throw;
      }
      catch (Exception ex)
      {
        savedEx = ex;
      }
	  try
	  {
		  Dispatcher.BeginInvoke((Action)(() =>
			{
				if (savedEx == null)
				{
					lblLoadFeedback_.Content = "File saved.";
				}
				else
				{
					lblLoadFeedback_.Content = "Export error.";
					if (!hidden_)
						MessageBox.Show(string.Format("An exception was thrown during log export:\n{0}: {1}", savedEx.GetType(), savedEx.Message), "Export Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				Mouse.OverrideCursor = Cursors.Arrow;

				btnSaveLog_.IsEnabled = true;
				btnSaveLogCSV_.IsEnabled = true;
				btnUploadNow.IsEnabled = true;
				mIdle.IsEnabled = false;
				mIdle.IsEnabled = true;
				if (hidden_)
				this.Close();
			}));
	  }
	  catch { }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void btnSaveLogCSV_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        lblLoadFeedback_.Content = "";

        if (entries_.Count == 0)
        {
          MessageBox.Show("The log is empty.", "No Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
          return;  // EARLY RETURN!
        }

        var dlg = new SaveFileDialog();

        dlg.DefaultExt = ".csv";
        dlg.Filter = "CSV Log File|*.csv";

        string localName = info_.HasLocalName ? info_.LocalName : "UnknownID";
        dlg.FileName = string.Format("{0:yyyyMMdd}_{0:HHmm}_{1}_{2}", DateTime.Now, localName, (whichLog_ == AURALog.Primary) ? "Main" : "Incident");

        if (!dlg.ShowDialog().GetValueOrDefault())
          return;  // EARLY RETURN!

        if (File.Exists(dlg.FileName))
        {
          try
          {
            File.Delete(dlg.FileName);
          }
          catch (IOException)
          {
            MessageBox.Show("The selected file cannot be accessed (is it open in another application?).", "File Access Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;  // EARLY RETURN!
          }
        }

        lblLoadFeedback_.Content = "Saving CSV...";

        AURALogIO.WriteLogToCsvFile(whichLog_, entries_, infoForExport_, dlg.FileName);

        lblLoadFeedback_.Content = "File saved.";
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }

    #endregion

    #region Operations

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// 
    public void OverrideVehicleName(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      Debug.WriteLine("VL2: OverrideVehicleName(" + name + ")");

      info_.LocalName = name;
      infoForExport_.LocalName = name;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    /// 
    public void OverrideCEMVersion(string version)
    {
      if (version == null)
        throw new ArgumentNullException("version");

      Debug.WriteLine("VL2: OverrideCEMVersion(" + version + ")");

      info_.CEMVersion = version;
      infoForExport_.CEMVersion = version;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    /// 
    public void OverrideUIMVersion(string version)
    {
      if (version == null)
        throw new ArgumentNullException("version");

      Debug.WriteLine("VL2: OverrideUIMVersion(" + version + ")");

      info_.UIMVersion = version;
      infoForExport_.UIMVersion = version;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    /// 
    public void OverrideRCMVersion(string version)
    {
      if (version == null)
        throw new ArgumentNullException("version");

      Debug.WriteLine("VL2: OverrideRCMVersion(" + version + ")");

      info_.RCMVersion = version;
      infoForExport_.RCMVersion = version;
    }
    
    #endregion

    #region CEM Message Handling

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// 
    public void LogStatusFromCEM(CEMLogStatus data)
    {
      // VL2: code complete

      try
      {
        // We've gotten a log status msg. Make sure that's what we're expecting: if so, stop the
        // timeout timer and process the msg; otherwise, just drop it and leave the timeout timer
        // alone.

        if (State != DLState.QueryStatus)
        {
          Debug.WriteLine("VL2MSG: Got a log status msg, but state ({0}) != QueryStatus; dropping msg.", State);
          return;  // EARLY RETURN!
        }

        // Implementation Note:
        //
        // The log retrieval process was designed to be stateless; that is, the CEM maintains no
        // knowledge of (log retrieval) state during the process, allowing it to be interrupted
        // at any point without consequence (e.g., the user can close the View Log dialog in the
        // middle of the download of a large number of entries; also, a bug could turn up that
        // terminates the download process early, or even causes the ADU to crash).
        //
        // This is achieved by implementing two stateless messages: get log status and get log
        // entry. The get log status message causes the CEM to send back a log status message
        // containing the maximum capacity (in entries) of and the current head and tail indices
        // for both the primary and secondary logs. After receiving the log status message and
        // picking out the appropriate head and tail indices, the log download code simply sends
        // one get log entry message for every desired entry, starting at the head index (newest)
        // and ending at the tail index (oldest), wrapping values if necessary.
        //
        // For a naive implementation of log retrieval: since the CEM maintains no state related
        // to log download, and considering that the retrieval of log status data and the requests
        // for each desired log entry are separated in time, it's possible that by the time the
        // log entry requests get to the end of the log (the tail index, as reported by the CEM
        // when it sent its log status message), new entries could actually appear (those new
        // entries being ones that overwrote what were the oldest entries at the time the log
        // status was reported). Worse, for Gen II devices - wherein logging is done to flash, and
        // when the head reaches the tail, the tail is moved ahead a full sector's worth of
        // entries due to the sector limitation on erase operations - these oldest entries could
        // actually be blank, freshly erased entries.
        //
        // To avoid this scenario, after we fetch the current log status, we will adjust the tail
        // index if the head index is within some number of entries of reaching the tail.

        // Stop the timeout timer and process the message.

        tmoTimer_.Stop();

        Debug.WriteLine("VL2: Got LogStatus msg! P max entries = {0}, head 0x{1:x4}, tail 0x{2:x4}; S max entries = {3}, head 0x{4:x4}, tail 0x{5:x4}",
          data.PLogMaxEntryCount, data.PLogHead, data.PLogTail, data.SLogMaxEntryCount, data.SLogHead, data.SLogTail);

        uint logHead, logTail, capacity;

        if (whichLog_ == AURALog.Primary)
        {
          logHead = data.PLogHead;
          logTail = data.PLogTail;
          capacity = data.PLogMaxEntryCount;
        }
        else
        {
          logHead = data.SLogHead;
          logTail = data.SLogTail;
          capacity = data.SLogMaxEntryCount;
        }

        GotLogStatus(capacity, logHead, logTail);
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="head"></param>
    /// <param name="tail"></param>
    /// 
    private void GotLogStatus(uint capacity, uint head, uint tail)
    {
      logNav_ = new LogNavigator(capacity, head, tail);

      Debug.WriteLine("VL2: Total entries in log = {0}", logNav_.TotalEntries);

      // Initialize total entries to download - this may change if the user restricts the
      // number of entries to download (by downloading a fixed count or using a date range).
      // This value is used to calculate the "% complete" feedback text displayed to the user.

      totalEntriesToDownload_ = logNav_.TotalEntries;

      // If we're looking at the Main log, adjust the tail index if necessary. Note that for
      // the Incident log, we won't adjust the tail index. It's extremely unlikely that new
      // data will be logged *during the incident log download*.

      if (whichLog_ == AURALog.Primary)
      {
        // If the head is within a certain number of entries of the tail, adjust our idea of
        // the tail forward by a fixed amount (see Implementation Note, above). The threshold
        // value is the number of entries contained in one sector of flash.
        //
        // Note that this adjustment is done even if the connected unit is Gen I (which uses
        // EEPROM for logging and has a maximum log capacity that's smaller than in Gen II),
        // because new log entries could be written during download just as with flash. The
        // only difference is that with EEPROM, we should never see blank (erased) entries
        // as we might with flash.

        if ((logNav_.Capacity - logNav_.TotalEntries) < tailAdjustThreshold_)
        {
          tail += tailAdjustThreshold_;

          if (tail >= logNav_.Capacity)
            tail -= logNav_.Capacity;

          // Recreate the log navigator with the adjusted tail.

          logNav_ = new LogNavigator(capacity, head, tail);

          Debug.WriteLine("VL2: Total entries is less than threshold ({0}) of 'log full' headroom. Adjusting tail; total entries is now ({1}).", tailAdjustThreshold_, logNav_.TotalEntries);

          // Re-init this value here (still may change later).

          totalEntriesToDownload_ = logNav_.TotalEntries;
        }
      }

      // TODO: Don't want to set this value *here*...
      lblEntryCount_.Content = logNav_.TotalEntries.ToString();

      if (logNav_.TotalEntries == 0)
        lblLoadFeedback_.Content = "Log is empty";
      else
      {
        // If the log contains less than a certain small number of entries, download all of
        // them automatically; otherwise, try to determine the date range spanned by the log
        // and let the user choose what they want to do.

        if ((logNav_.TotalEntries <= autoDownloadLogCountThreshold_ / 2) || useEntryLimit_)
        {
          if (useEntryLimit_)
            Debug.WriteLine("VL2: Downloading entries with limit = {0}", entryLimit_);
          else
            Debug.WriteLine("VL2: Log contains ({0}) or fewer entries; automatically downloading full log.", autoDownloadLogCountThreshold_);

          // Initialize fields used during actual download of log entries.

          curFetchNdx_ = logNav_.TotalEntries - 1;
          endingFetchNdx_ = 0;
          curFetchOrdinal_ = 1;

          if (useEntryLimit_)
            totalEntriesToDownload_ = entryLimit_;

          // Note that if we're here because the entry limit ctor was used, entryLimit_ will
          // already be set properly.

          State = DLState.GetEntry;

          // Keeping this code around strictly for reference, in case some similar debugging
          // work needs to be done:
#if DEBUG_TIMEOUT
  //          Debug.WriteLine("Requesting index {0}", curFetchNdx_);
            tmoTimer_.Interval = 10000;  // set a 10 s timeout for the very first log entry request
#endif
        }
        else
        {
          curFetchNdx_ = logNav_.TotalEntries - 1;
          curFetchOrdinal_ = 1;

          Debug.WriteLine("VL2: Log contains more than ({0}) entries; scanning for head with valid date; start ndx = {1}.", autoDownloadLogCountThreshold_, curFetchNdx_);

          State = DLState.ScanHead;
        }

        RequestLogEntry(curFetchNdx_);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// 
    public void LogEntryFromCEM(CEMLogEntry data)
    {
      try
      {
        // We've gotten a log entry msg. Make sure that's what we're expecting and verify that
        // the returned index is the one we're waiting on, then process the msg; otherwise, just
        // drop it and leave the timeout timer alone.

        if ((State != DLState.GetEntry) &&
            (State != DLState.ScanHead) &&
            (State != DLState.ScanTail) &&
            (State != DLState.ProbeStart) &&
            (State != DLState.ProbeEnd))
        {
          Debug.WriteLine("VL2MSG: Got a log entry msg, but state ({0}) isn't GetEntry, ProbeStart/End, or ScanHead/Tail; dropping msg.", State);
          return;  // EARLY RETURN!
        }

        if (data.EntryIndex != curGetLogEntryNdx_)
        {
          Debug.WriteLine("VL2MSG: Got a log entry msg, but index ({0}) isn't the one we requested ({1}); dropping msg.", data.EntryIndex, curGetLogEntryNdx_);
          return;  // EARLY RETURN!
        }

        if (logNav_ == null)
        {
          Debug.WriteLine("ViewLogDlg.LogEntryFromCEM: Got msg for log entry {0} before getting log status msg!", data.EntryIndex);
          return;  // EARLY RETURN!
        }

        tmoTimer_.Stop();

        // Create a log entry object from the returned data.

        // VL2: Should really include a caching mechanism here, so that entries retrieved during
        // scan or probe steps aren't requested again... For now, though, we'll just create them
        // all the time.

        bool isGen1IncidentLogEntry = (whichLog_ == AURALog.Secondary) && IncidentLogIsGen1;

        AURALogEntry entry = AURALogEntry.CreateLogEntry(data.TimestampS, data.Timestamp10Ms, data.EntryPayload, isGen1IncidentLogEntry);

        // If we're in a preprocessing state, add the entry to the cache.

        if ((State == DLState.ScanHead) ||
            (State == DLState.ScanTail) ||
            (State == DLState.ProbeStart) ||
            (State == DLState.ProbeEnd))
        {
          // Sanity first.

          if (cache_.ContainsKey(curGetLogEntryNdx_))
            Debug.WriteLine("VL2: ##### Adding entry {0} to cache, but the index key already exists!!");

          cache_[curGetLogEntryNdx_] = entry;
        }

        GotLogEntry(entry);
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// 
    private void GotLogEntry(AURALogEntry entry)
    {
      try
      {
        switch (State)
        {
        case DLState.GetEntry:
			ProcessLogEntry(entry);
			counter++;

          break;

        case DLState.ScanHead:
          ProcessLogEntryForHeadScan(entry);
          break;

        case DLState.ScanTail:
          ProcessLogEntryForTailScan(entry);
          break;

        case DLState.ProbeStart:
          ProcessLogEntryForStartProbe(entry);
          break;

        case DLState.ProbeEnd:
          ProcessLogEntryForEndProbe(entry);
          break;

        default:
          Debug.WriteLine("VL2: Got log entry msg from CEM but state is unexpected ({0})! Stopping download...", State);
          State = DLState.Done;
          break;
        }

        if (State == DLState.Done)
        {
          Debug.WriteLine("VL2: ========== At conclusion of search, total entries retrieved = {0} ==========", entries_.Count);

          Debug.WriteLine("   : logNav_ says total entries in log is {0}", logNav_.TotalEntries);

          if (entries_.Count < logNav_.TotalEntries)
            Debug.WriteLine("   : Showing {0} of {1} entries.", entries_.Count, logNav_.TotalEntries);
          else
            Debug.WriteLine("   : Showing all {0} entries.", entries_.Count);

          if (entries_.Count < logNav_.TotalEntries)
            lblLoadFeedback_.Content = "Shown: " + entries_.Count;
          else
            lblLoadFeedback_.Content = "";

        }
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    public void OTASendFailed()
    {
      if (msgSource_ != MsgSource.OTA)
        throw new InvalidOperationException("Message source is not OTA");

      tmoTimer_.Stop();

      RetryOrFailMsg(true);
    }

    #endregion

    #region Implementation

    /// <summary>
    /// 
    /// </summary>
    /// 
    private void RequestLogStatus()
    {
      if (!logIsSimulated_)
      {
        // Reset our retry counter, start the timeout timer, and send the get log status msg.

        tryCount_ = 0;
        tmoTimer_.Start();

        Host_SendGetLogStatus();
      }
      else
      {
        Dispatcher.BeginInvoke((Action)(() => { GotLogStatus(simLogCapacity_, simLogHead_, simLogTail_); }));
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entryNdx"></param>
    /// 
    private void RequestLogEntry(uint entryNdx)
    {
      curGetLogEntryNdx_ = logNav_.GetPhysicalIndex(entryNdx);

      // Check the cache first.

      if (cache_.ContainsKey(curGetLogEntryNdx_))
      {
        AURALogEntry entry = cache_[curGetLogEntryNdx_];

        Dispatcher.BeginInvoke((Action)(() => { GotLogEntry(entry); }));
      }
      else
      {
        // Reset our retry counter, start the timeout timer, and send the get log status msg.

        tryCount_ = 0;
        tmoTimer_.Start();

        Host_SendGetLogEntry(whichLog_, curGetLogEntryNdx_);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    private void InitiateLogInteraction()
    {
      // VL2: code complete (no old, unexamined code hanging around; additions ok)

      btnSaveLog_.IsEnabled = false;  // save function is disabled until we complete download
      btnSaveLogCSV_.IsEnabled = false;
	  btnUploadNow.IsEnabled = false;

      // First thing to do is to get the log status so that we know the log's capacity and the
      // value of the head and tail indices.

      State = DLState.QueryStatus;

      RequestLogStatus();

      // Held over from old code strictly for reference:
#if DEBUG_TIMEOUT
//      ((MainWindow)Owner).SendMgmtMsg(new Msg_MgmtUIPCControl(0));  // enable data capture in UIM
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// 
    private void ProcessLogEntry(AURALogEntry entry)
    {
      Debug.WriteLine("VL2: Got log entry! (physical index {0}, logical index {1})", curGetLogEntryNdx_, curFetchNdx_);

      // If necessary, look for and pull out local vehicle name from Startup or Incident
      // (start) entry, CEM version number from Startup entry, and UIM version number from
      // UIMStartup entry - *if* we run across those entries in the ones we download.

      if (!info_.HasAllData)
      {
        if (whichLog_ == AURALog.Primary)
        {
          if (entry.RawEntryType == (byte)CEMLogEntryType.Startup)
          {
            if (!info_.HasLocalName)
              info_.LocalName = ((AURALogEntry_Startup)entry).Name;
            if (!info_.HasCEMVersion)
              info_.CEMVersion = ((AURALogEntry_Startup)entry).VersionString;
          }
          else if ((entry.RawEntryType == (byte)CEMLogEntryType.UIMStartup) && (!info_.HasUIMVersion))
            info_.UIMVersion = ((AURALogEntry_UIMStartup)entry).VersionString;
        }
        else if ((whichLog_ == AURALog.Secondary) && (entry.RawEntryType == (byte)CEMLogEntryType.Incident))
        {
          if (((((AURALogEntry_Incident)entry).Flags & CEMIncidentEntryFlag.IncidentStart) != 0) && (!info_.HasLocalName))
			  info_.LocalName = ((AURALogEntry_Incident)entry).Name;
        }
      }

      AddLogEntry(entry, curFetchOrdinal_, curGetLogEntryNdx_);

      // If not done, decrement fetch ndx with wrap and request next entry.
      //
      // If a fetch limit has been specified, check to see if we've downloaded the (limited)
      // max number of entries; otherwise, check to see if the current fetched index is
      // equal to the ending index.

      if (((entryLimit_ > 0) && (curFetchOrdinal_ == entryLimit_)) || (curFetchNdx_ == endingFetchNdx_))
      {
        if ((entryLimit_ > 0) && (logNav_.TotalEntries > entryLimit_))
          lblLoadFeedback_.Content = "Shown: " + entryLimit_;
        else
          lblLoadFeedback_.Content = "";

        // Hold this code for just a little longer in case we need to use it again...
#if DEBUG_TIMEOUT
          ++testLoopCount_;
          ((MainWindow)Owner).AddToOutput("Cycles completed: " + testLoopCount_.ToString());
          // debug: repeat!
          ((MainWindow)Owner).SendMgmtMsg(new Msg_MgmtUIGetLogStatus());
#endif
        State = DLState.Done;

        btnSaveLog_.IsEnabled = true;
        btnSaveLogCSV_.IsEnabled = true;
		btnUploadNow.IsEnabled = true;
		mIdle.IsEnabled = false;
		mIdle.IsEnabled = true;
		if (hidden_)
		{
			SaveLogXlsx();
		}
      }
      else
      {
        DateTime now = DateTime.Now;
        if ((now - lastPercentUpdate_).TotalMilliseconds > 50)
        {
          //double totalAndScale = 100.0 / (((entryLimit_ > 0) && (entryLimit_ < logNav_.TotalEntries)) ? entryLimit_ : logNav_.TotalEntries);
          double totalAndScale = 100.0 / totalEntriesToDownload_;
          double percentDone = curFetchOrdinal_ * totalAndScale;

          lblLoadFeedback_.Content = "Loading: " + percentDone.ToString("f1") + "% (of " + totalEntriesToDownload_ + ")";
        }

        ++curFetchOrdinal_;
        --curFetchNdx_;

        // Hold this code for just a little longer in case we need to use it again...
#if DEBUG_TIMEOUT
  //        Debug.WriteLine("Requesting index {0}", curFetchNdx_);
          tmoTimer_.Interval = (curFetchOrdinal_ < 10) ? 10000 : 5000;
#endif

        RequestLogEntry(curFetchNdx_);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// 
    private void ProcessLogEntryForHeadScan(AURALogEntry entry)
    {
      Debug.WriteLine("VL2: Head scan: got log entry!");

      // VL2: NOTE: Not entirely sure yet whether checking the timestamp is as simple as
      // inspecting the TimestampIsValid field, or if we'll need to do more work to validate
      // the timestamp...

      if (entry.TimestampIsValid)
      {
        Debug.WriteLine("VL2: Got valid timestamp; head scan ended. Doing tail scan next; start ndx = 0.");

        goodHeadDateEntry_ = entry;
        goodHeadDateNdx_ = curFetchNdx_;

        curFetchNdx_ = 0;
        curFetchOrdinal_ = 1;

        State = DLState.ScanTail;
      }
      else
      {
        // Move to the next entry (toward the tail).

        ++curFetchOrdinal_;
        --curFetchNdx_;

        // VL2: Add sanity check here, in case we reach the tail!
      }

      RequestLogEntry(curFetchNdx_);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// 
    private void ProcessLogEntryForTailScan(AURALogEntry entry)
    {
      Debug.WriteLine("VL2: Tail scan: got log entry!");

      // VL2: NOTE: See comment about timestamp in ProcessLogEntryForHeadScan().

      if (entry.TimestampIsValid)
      {
        // VL2 NOTE: Before doing the scans, we've checked to see whether the total number of
        // entries is less than some threshold (e.g., 200), and if so, we won't do the scans
        // (and so will never get here) - we'll download all entries automatically.
        //
        // [Keep in mind that the head and tail scans locate the headmost (latest) and tailmost
        // (oldest) entries having valid dates; these may be different from the actual head and
        // tail indices.]
        //
        // However, after we do the head and tail scans, it's possible that we could have
        // reduced the number of entries that could potentially be downloaded (e.g., if there
        // are a few entries with no valid timestamps at the very beginning of the log - on,
        // say, a Gen I device that has no GPS fix - then the scanned head will not be the
        // same as the absolute head).
        //
        // So we should check the total number of entries between the scanned head and the
        // scanned tail (inclusive), and if that number is less than the same threshold, we'll
        // just download all covered entries automatically, and won't bother to present the
        // range selection dialog to the user.

        Debug.WriteLine("VL2: Got valid timestamp; tail scan ended.");

        goodTailDateEntry_ = entry;
        goodTailDateNdx_ = curFetchNdx_;

        if ((goodHeadDateNdx_ - goodTailDateNdx_ + 1) <= autoDownloadLogCountThreshold_)
        {
          Debug.WriteLine("VL2: After head/tail scan, inclusive contained range < {0} entries; automatically downloading full log.", autoDownloadLogCountThreshold_);

          // Initialize fields used during download of log entries.

          endingFetchNdx_ = 0;
          curFetchNdx_ = logNav_.TotalEntries - 1;
          curFetchOrdinal_ = 1;

          State = DLState.GetEntry;
        }
        else
        {
          Debug.WriteLine("VL2: Got valid timestamp; tail scan ended. Presenting dialog to user...");

          State = DLState.UIRangeEntry;

          SelectRangeDlg dlg = new SelectRangeDlg(hidden_);

          dlg.Owner = this;

          // Set start/end date defaults first.

          dlg.StartDate = goodHeadDateEntry_.UTCTimestamp.Date;
          dlg.EndDate = goodTailDateEntry_.UTCTimestamp.Date;

          if (!dlg.ShowDialog().GetValueOrDefault())
          {
            State = DLState.Done;
            Close();
            return;  // EARLY RETURN!
          }

          if (dlg.UseCount)
          {
            curFetchNdx_ = logNav_.TotalEntries - 1;
            curFetchOrdinal_ = 1;
            endingFetchNdx_ = 0;
            entryLimit_ = dlg.Count;
            if (entryLimit_ > 0)
              totalEntriesToDownload_ = entryLimit_;

            State = DLState.GetEntry;
          }
          else
          {
            // Remember the target date range for download.

            startDate_ = dlg.StartDate;
            endDate_ = dlg.EndDate;

            Debug.WriteLine("VL2: Download range: From {0:yyyy/MM/dd} thru {1:yyyy/MM/dd}.", startDate_, endDate_);
            Debug.WriteLine("VL2: Setting up Start probe...");

            // (Note that the below two comment blocks are being left in but need to be reworded.
            // They refer to field initialization that has been moved into SetupBinarySearch().)

            // Initialize the binary search parameters: targetDate_ is the date we're looking
            // for (the State will determine whether we look for newest equal or oldest equal).
            // The probe{Head,Tail>Index_ values hold the (inclusive) logical index extents for
            // each pass of the search.

            // Note that the "probe test" entry is the "midpoint" chosen during each step of the
            // binary search. However, due to the need to skip entries with invalid dates, we
            // keep track of "left" and "right" indices: if no invalid dates are found, they're
            // always equal, and we just (arbitrarily) use the "left" index; once we land on an
            // entry with an invalid date, we'll adjust the left index toward the tail until we
            // find a valid date, and if necessary (if the left index reaches the probe tail)
            // adjust the right index toward the head until we find a valid date. If an invalid
            // date has been found (and left and right aren't equal), then if the next binary
            // search step moves to the left half, we adjust the probe head to just past the
            // left test index, and if it moves to the right half, we adjust the probe tail to
            // just past the right test index - this eliminates from consideration the entire
            // contiguous stretch of entries with invalid dates.

            Debug.WriteLine("VL2: Probe head = {0} ({1} phys), tail = {2} ({3} phys).",
              goodHeadDateNdx_,
              logNav_.GetPhysicalIndex(goodHeadDateNdx_),
              goodTailDateNdx_,
              logNav_.GetPhysicalIndex(goodTailDateNdx_));

            if (!SetupBinarySearch(startDate_, goodHeadDateNdx_, goodTailDateNdx_))
            {
              Debug.WriteLine("VL2: When setting up initial start probe conditions, SetupBinarySearch() returned false!");
              return;  // EARLY RETURN!
            }

            // When starting off, probe left/right indices are the same, set by
            // PrepareNextSearchPass() which is called from SetupBinarySearch().

            curFetchNdx_ = probeTestLeftNdx_;

            Debug.WriteLine("   : Probing index {0} (phys {1}).", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));

            State = DLState.ProbeStart;
          }
        }
      }
      else
      {
        // Move to the next entry (toward the head).

        ++curFetchOrdinal_;
        ++curFetchNdx_;
      }

      RequestLogEntry(curFetchNdx_);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// 
    private void ProcessLogEntryForStartProbe(AURALogEntry entry)
    {
      // During the Start probe, we're trying to find the head-most entry having a date that
      // matches the user-selected starting date (or, if no entries have that date exactly,
      // the head-most entry having a date that is more recent than the user-selected ending
      // date).
      //
      // The probe process is a simple binary search algorithm, with a modification that
      // excludes contiguous runs of entries with invalid timestamps when they are discovered.
      //
      // The search starts off with the probe head and tail set to the first (most recent)
      // and last (oldest) entry with valid dates. At each cycle in the search, the midpoint
      // entry is requested; depending on the search sense (start or end probe), the head or
      // tail will be adjusted just past the tested midpoint and the next cycle will be run.
      // The search terminates when the head and tail indices match.
      //
      // The invalid timestamp modification works like this: if the requested midpoint has an
      // invalid timestamp, the code will request the next left or right entry (toward the
      // tail and head, respectively) and examine it instead. Entries to the left are checked
      // first; if the search reaches the tail, entries to the right are checked next, until
      // reaching the head.
      //
      // Once a valid timestamp is found, if the next search cycle chooses the left subset for
      // further examination, the new head will be moved just past the leftmost entry with an
      // invalid timestamp; if it chooses the right subset, the new tail will be moved just
      // past the rightmost entry with an invalid timestamp.

      if (entry.TimestampIsValid)
      {
        // For this binary search cycle, the selected midpoint entry has a good date. Choose
        // whether to continue the probe (and if so in what direction) or terminate the search,
        // and record a new candidate entry if appropriate.

        if (entry.UTCTimestamp.Date > targetDate_)
        {
          Debug.WriteLine("VL2: Probe (start) found good date; date > target, so probing left (no change to C)...");

          // We're probing for the starting entry, and the date of the test entry is greater
          // than (more recent than) the target: this excludes it from consideration (it is not
          // a candidate), and we'll attempt to probe left.

          // T = probe tail, S = test entry, H = probe head
          //
          // Current configuration:
          //    T           H     |     T         H       |     T   H             |     T H               |    T,H
          // +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+
          // | | | | | | | | | |  |  | | | | | | | | | |  |  | | | | | | | | | |  |  | | | | | | | | | |  |  | | | | | | | | | |
          // +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+
          //          S           |         S             |       S               |     S                 |     S
          //                      Or                      Or                      |                       |
          // After setup:         |                       |                       |                       |
          //    T   H             |     T H               |    T,H                |                       |
          // +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  Test S == Tail,      |  Test S == Tail,
          // | | | | | | | | | |  |  | | | | | | | | | |  |  | | | | | | | | | |  |  stop.                |  stop.
          // +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |  +-+-+-+-+-+-+-+-+-+  |                       |
          //      S               |     S                 |     S                 |                       |

          // To set up the left probe, we need to move the probe head to the left of the left
          // test index (recall that during any binary search cycle, the decision point may
          // be the midpoint between head and tail if its date is valid, but if it's not, it
          // may be a range of contiguous entries with invalid dates). If the left test index
          // is at the probe tail, there are no more entries to search - we're done.

          if (probeTestLeftNdx_ > probeTailNdx_)
          {
            // There are more entries to search to the left. Move the probe head to the left
            // of the left test index and execute another search cycle.

            probeHeadNdx_ = probeTestLeftNdx_ - 1;

            PrepareNextSearchPass();  // note that this call will adjust the test indices

            Debug.WriteLine("   : Probing index {0} (phys {1}). (start; left)", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));

            RequestLogEntry(probeTestLeftNdx_);
          }
          else
          {
            // There are no more entries to the left to search. If no candidate index was
            // recorded, there are no entries in the log in the requested date range, and we're
            // done; otherwise, record the candidate as the starting download index and probe
            // for the ending entry.

            Debug.WriteLine("   : No more entries to probe to the left!");

            SwitchToEndProbeOrStop();
          }
        }  // end if (entry date > target)
        else
        {
          // We're probing for the starting entry, and the date of the test entry is less than
          // (older than) or equal to the target.

          // Sanity note: the target date *should* never be less than (older than) the scanned
          // end date, but who knows what GPS / RTC weirdness we might encounter. We don't
          // handle this condition directly: instead, we continue the binary search (but we
          // don't record a candidate).

          // If the entry date is greater than (more recent than) or equal to the ending date,
          // record the entry as a candidate starting date.

          if (entry.UTCTimestamp.Date >= endDate_)
            probeCandidateNdx_ = probeTestLeftNdx_;

          // Now we probe right.

          if (probeTestRightNdx_ < probeHeadNdx_)
          {
            probeTailNdx_ = probeTestRightNdx_ + 1;

            PrepareNextSearchPass();

            Debug.WriteLine("   : Probing index {0} (phys {1}). (start; right)", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));

            RequestLogEntry(probeTestLeftNdx_);
          }
          else
          {
            Debug.WriteLine("   : No more entries to probe to the right!");

            SwitchToEndProbeOrStop();
          }

        }  // end if (entry date <= target)
      }
      else
      {
        Debug.WriteLine("VL2: Probe (start) found invalid date!");

        // The probed entry's timestamp is invalid.

        PerformAdjustmentStep(true);

      }  // end else (timestamp is invalid)
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// 
    private void ProcessLogEntryForEndProbe(AURALogEntry entry)
    {
      if (entry.TimestampIsValid)
      {
        if (entry.UTCTimestamp.Date < targetDate_)
        {
          Debug.WriteLine("VL2: Probe (end) found good date; date < target, so probing right (no change to C)...");

          if (probeTestRightNdx_ < probeHeadNdx_)
          {
            // There are more entries to search to the right. Move the probe tail to the right
            // of the right test index and execute another search cycle.

            probeTailNdx_ = probeTestRightNdx_ + 1;

            PrepareNextSearchPass();  // note that this call will adjust the test indices

            // Reminder: PrepareNextSearchPass() will calculate the midpoint between
            // probeHeadNdx_ and probeTailNdx_, and will initialize *both* probeTestRightNdx_
            // *and* probeTestLeftNdx_ to that value.

            Debug.WriteLine("   : Probing index {0} (phys {1}). (end; right)", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));

            RequestLogEntry(probeTestLeftNdx_);
          }
          else
          {
            // There are no more entries to the right to search. If no candidate index was
            // recorded, there are no entries in the log in the requested date range, and we're
            // done; otherwise, set up a download of all log entries between the previously
            // identified download start entry and the current candidate (inclusive).

            Debug.WriteLine("   : No more entries to probe to the right!");

            DownloadDateRangeOrStop();
          }
        }  // end if (entry date < target)
        else
        {
          if (entry.UTCTimestamp.Date <= startDate_)
            probeCandidateNdx_ = probeTestLeftNdx_;

          // Now we probe left.

          if (probeTestLeftNdx_ > probeTailNdx_)
          {
            probeHeadNdx_ = probeTestLeftNdx_ - 1;

            PrepareNextSearchPass();

            Debug.WriteLine("   : Probing index {0} (phys {1}). (end; left)", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));

            RequestLogEntry(probeTestLeftNdx_);
          }
          else
          {
            Debug.WriteLine("   : No more entries to probe to the left!");

            DownloadDateRangeOrStop();
          }
        }  // end else (entry date >= target)
      }  // end if (timestamp is valid)
      else
      {
        Debug.WriteLine("VL2: Probe (end) found invalid date!");

        // The probed entry's timestamp is invalid.

        PerformAdjustmentStep(false);

      }  // end else (timestamp is invalid)
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dateTarget"></param>
    /// <param name="searchHeadNdx"></param>
    /// <param name="searchTailNdx"></param>
    /// <returns></returns>
    /// 
    private bool SetupBinarySearch(DateTime dateTarget, uint searchHeadNdx, uint searchTailNdx)
    {
      targetDate_ = dateTarget;

      probeHeadNdx_ = searchHeadNdx;
      probeTailNdx_ = searchTailNdx;

      probeCandidateNdx_ = kInvalidCount;
      probeTestLeftNdx_ = kInvalidIndex;
      probeTestRightNdx_ = kInvalidIndex;
      probeTestAdjustMovesLeft_ = false;

      return PrepareNextSearchPass();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// 
    private bool AdjustDeadZoneLeftAndGetEntry()
    {
      if (probeTestLeftNdx_ > probeTailNdx_)
      {
        --probeTestLeftNdx_;

        RequestLogEntry(probeTestLeftNdx_);

        return true;
      }
      else
        return false;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// 
    private bool AdjustDeadZoneRightAndGetEntry()
    {
      if (probeTestRightNdx_ < probeHeadNdx_)
      {
        ++probeTestRightNdx_;

        RequestLogEntry(probeTestRightNdx_);

        return true;
      }
      else
        return false;
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    private void PerformAdjustmentStep(bool inStartProbe)
    {
      // Adjust the probed left or right index based on the current probe conditions. Note that
      // that if this call is made for the first invalid timestamp for the current binary search
      // step, the probe fields will already be set up to adjust left (no special case for the
      // first invalid timestamp).

      if (probeTestAdjustMovesLeft_)
      {
        // We're in the "left" adjust phase. If the left test index is not at the tail, adjust
        // it left and try again. Otherwise, we'll switch to the "right" adjust phase: if the
        // right test index is not at the head, adjust it right and try again. Otherwise, the
        // current binary search step has failed to find a candidate, and we'll stick with the
        // last candidate or fail entirely.

        if (AdjustDeadZoneLeftAndGetEntry())
          Debug.WriteLine("   : in left adjust phase, left test index is not at tail, so adjusting to {0} (phys {1}) and probing.", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));
        else
        {
          Debug.WriteLine("   : in left adjust phase, and left test index is at probe tail; switching to right adjust phase.");

          probeTestAdjustMovesLeft_ = false;

          if (AdjustDeadZoneRightAndGetEntry())
            Debug.WriteLine("   : right test index is not at head, so adjusting to {0} (phys {1}) and probing.", probeTestRightNdx_, logNav_.GetPhysicalIndex(probeTestRightNdx_));
          else
          {
            Debug.WriteLine(string.Format("   : ...but right test index is at probe head. The {0} probe has ended.", inStartProbe ? "start" : "end"));

            if (inStartProbe)
              SwitchToEndProbeOrStop();
            else
              DownloadDateRangeOrStop();
          }
        }  // end (probe test left is at probe tail)
      }  // end if (probe adjust is moving left)
      else
      {
        Debug.WriteLine("   : adjusting right (toward head).");

        if (AdjustDeadZoneRightAndGetEntry())
          Debug.WriteLine("   : right test index is not at head, so adjusting to {0} (phys {1}) and probing.", probeTestRightNdx_, logNav_.GetPhysicalIndex(probeTestRightNdx_));
        else
        {
          Debug.WriteLine("   : ...but right test index is at probe head. The end probe has ended.");

          if (inStartProbe)
            SwitchToEndProbeOrStop();
          else
            DownloadDateRangeOrStop();
        }
      }  // end else (probe adjust is moving right)
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    private bool PrepareNextSearchPass()
    {
      // targetDate_: date we're searching for (State will determine whether we look for newest
      //   equal or oldest equal)
      // probeHeadIndex_, probeTailIndex_: current index range in which to search

//      if (probeHeadNdx_ == probeTailNdx_)
//        return false;

      // Left == toward tail; right == toward head.

      probeTestLeftNdx_ = probeTailNdx_ + ((probeHeadNdx_ - probeTailNdx_) / 2);
      probeTestRightNdx_ = probeTestLeftNdx_;

      return true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    private void SwitchToEndProbeOrStop()
    {
      // We've completed a binary search, either by a normal binary search termination (last
      // midpoint examined) or by running out of entries with valid dates during the search,
      // and we're ready to switch to the end probe phase.
      //
      // If no candidate was found during the search, there's nothing more to do - stop the
      // search. If a candidate was found but it's the original search tail, it's the only
      // entry in the date range - skip the end probe phase and download that single entry.
      // Otherwise, switch to the end probe phase.

      if (probeCandidateNdx_ == kInvalidIndex)
      {
        Debug.WriteLine("   : Probe has failed - no candidate found!");

        State = DLState.Done;
      }
      else
      {
        Debug.WriteLine("   : Got start candidate at index {0} (phys {1}).", probeCandidateNdx_, logNav_.GetPhysicalIndex(probeCandidateNdx_));

        downloadStartNdx_ = probeCandidateNdx_;

        if (downloadStartNdx_ == goodTailDateNdx_)
        {
          endingFetchNdx_ = downloadStartNdx_;
          curFetchNdx_ = downloadStartNdx_;
          curFetchOrdinal_ = 1;

          State = DLState.GetEntry;

          RequestLogEntry(curFetchNdx_);
        }
        else
        {
          SetupBinarySearch(endDate_, downloadStartNdx_, goodTailDateNdx_);

          Debug.WriteLine("   : Switching to end probe, starting at {0} (phys {1}).", probeTestLeftNdx_, logNav_.GetPhysicalIndex(probeTestLeftNdx_));

          probeCandidateNdx_ = kInvalidIndex;

          State = DLState.ProbeEnd;

          RequestLogEntry(probeTestLeftNdx_);
        }
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    private void DownloadDateRangeOrStop()
    {
      if (probeCandidateNdx_ == kInvalidIndex)
      {
        Debug.WriteLine("   : Probe has failed - no candidate found!");

        State = DLState.Done;
      }
      else
      {
        Debug.WriteLine("   : Got end candidate at index {0} (phys {1}). Downloading entries in date range...", probeCandidateNdx_, logNav_.GetPhysicalIndex(probeCandidateNdx_));

        endingFetchNdx_ = probeCandidateNdx_;
        curFetchNdx_ = downloadStartNdx_;
        curFetchOrdinal_ = 1;

        totalEntriesToDownload_ = (downloadStartNdx_ - endingFetchNdx_) + 1;

        State = DLState.GetEntry;

        RequestLogEntry(curFetchNdx_);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="ordinal"></param>
    /// <param name="ndx"></param>
    /// 
    private void AddLogEntry(AURALogEntry entry, uint ordinal, uint ndx, bool wasImported = false)
    {
#if DEBUG_TIMEOUT
      // debug: ease up on memory consumption by not doing this.
      return;
#endif
      if (!wasImported)
        entries_.Add(entry);

#if IS_ACU
      if ((entry.RawEntryType != (byte)CEMLogEntryType.AlarmEvent) &&
          (entry.RawEntryType != (byte)CEMLogEntryType.AlarmPeerData) &&
          (entry.RawEntryType != (byte)CEMLogEntryType.AlarmPeerData2) &&
          (entry.RawEntryType != (byte)CEMLogEntryType.AlarmPeerData3) &&
          (entry.RawEntryType != (byte)CEMLogEntryType.AlarmSilencedOld) &&
          (entry.RawEntryType != (byte)CEMLogEntryType.NewAlarmOldEntry))
        return;
#endif

      StringBuilder sb = new StringBuilder();

#if IS_ACU
      sb.AppendFormat("[{0}] ts ", ordinal);
#else
      sb.AppendFormat("[{0}] ndx {1}: ts ", ordinal, ndx);
#endif

      sb.Append(entry.TimestampIsValid ?
        entry.LocalTimestamp.ToString("yyyy/MM/dd HH:mm:ss.ff ")
        : "----/--/-- --:--:--.-- ");

      entry.GetDisplayString(sb);

      lbLog_.Items.Add(sb.ToString());
    }


    /// <summary>
    /// Timeout timer expiration method; called on the UI thread when a timeout occurs.
    /// </summary>
    /// <param name="wasFailureNotTimeout"></param>
    /// 
    private void RetryOrFailMsg(bool wasFailureNotTimeout = false)
    {
		try
		{
			++tryCount_;

			// Now tryCount_ will equal the number of tries so far (e.g.: since tryCount_ is always
			// initialized to zero, if the first message send times out,

			// Note that passing true for wasFailureNotTimeout only makes sense for OTA attempts.
			// We won't validate that case, however.

			string condition = wasFailureNotTimeout ? "not ACKed" : "timeout";

			if (tryCount_ < tryLimit_)
			{
				bool restartTimeout = true;

				switch (State)
				{
					case DLState.QueryStatus:
						Host_UIFeedback(string.Format("Log status message {0}! Resending... (try #{1})", condition, tryCount_ + 1));
						Host_SendGetLogStatus();
						break;

					case DLState.GetEntry:
					case DLState.ScanHead:
					case DLState.ScanTail:
					case DLState.ProbeStart:
					case DLState.ProbeEnd:
						// VL2: Will probably remove the reference to fetch ordinal here...
						Host_UIFeedback(string.Format("Log entry message {0}! Ordinal {1}, index {2}, resending... (try #{3})", condition, curFetchOrdinal_, curGetLogEntryNdx_, tryCount_ + 1));
						Host_SendGetLogEntry(whichLog_, curGetLogEntryNdx_);
						break;

					default:
						Host_UIFeedback(string.Format("RetryOrFailMsg() called while in unexpected state ({0})!", State));
						restartTimeout = false;
						break;
				}

				if (restartTimeout)
					tmoTimer_.Start();
			}
			else
			{
				string msg = (State == DLState.QueryStatus) ? "status" : "entry";

				Host_UIFeedback(string.Format("Log {0} message {1}! Retries exhausted ({2}), download failed.", msg, condition, tryLimit_));
				if (!hidden_)
				MessageBox.Show(string.Format("Log {0} message {1}, and message retry limit reached ({2}). The download attempt failed.", msg, condition, tryLimit_),
				  "Log Download Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);

				Close();
			}
		}
		catch { }
    }

    #endregion

    #region Host Interface Helpers

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// 
    private void Host_UIFeedback(string text)
    {
      try
      {
        ((IViewLogDlgHost)host_.Target).ViewLog_UIFeedback(text);
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    private void Host_SendGetLogStatus()
    {
      try
      {
        ((IViewLogDlgHost)host_.Target).ViewLog_SendGetLogStatus();
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="whichLog"></param>
    /// <param name="entryIndex"></param>
    /// 
    private void Host_SendGetLogEntry(AURALog whichLog, uint entryIndex)
    {
      try
      {
        ((IViewLogDlgHost)host_.Target).ViewLog_SendGetLogEntry(whichLog, entryIndex);
		Thread.Sleep(4);
      }
      catch (Exception ex)
      {
        string[] msg = ExceptionUtil.ExceptionReport(ex);
        AppLogger.WriteToLog(msg);
      }
    }

    #endregion

    #region Development Test and Debug

    /// <summary>
    /// 
    /// </summary>
    /// 
    private void SetupTestLogData(string testDataFile)
    {
      if (testDataFile == null)
        throw new ArgumentNullException("testDataFile");

      uint lineNum = 0;

      logIsSimulated_ = true;

      bool firstDataLine = true;

      try
      {
        foreach (string rawLine in File.ReadLines(testDataFile))
        {
          ++lineNum;

          string line = rawLine.Trim();

          // Eliminate blanks and comments (both full lines and at end of data lines).

          if (line.Length > 0)
          {
            int commentNdx = line.IndexOf("//");

            if (commentNdx >= 0)
              line = line.Substring(0, commentNdx).Trim();
          }

          if (line.Length == 0)
            continue;

          // Split on commas.

          string[] fields = line.Split(',');

          // Process.

          if (firstDataLine)
          {
            GetTestLogParams(lineNum, fields);
              firstDataLine = false;
          }
          else
            GetTestLogDataSpec(lineNum, fields);
        }

        if (cache_.Count < simLogExpectedCount_)
          throw new InvalidOperationException(string.Format("{0} entries total generated from file; however, log params spec in file indicates at least {1} entries expected", cache_.Count, simLogExpectedCount_));
      }
      catch (Exception ex)
      {
		if (!hidden_)
        MessageBox.Show(string.Format("Line {0}: {1}: {2}.", lineNum, ex.GetType(), ex.Message), "Test Data File Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        cache_.Clear();  // technically not necessary, since we won't re-run the search at this point...
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="line"></param>
    /// 
    private bool GetTestLogParams(uint lineNum, string[] fields)
    {
      if (fields == null)
        throw new ArgumentNullException("fields");

      if (fields.Length < 3)
        throw new InvalidOperationException("Log params must have at least 3 fields");
      if (fields.Length > 5)
        throw new InvalidOperationException("Log params must have no more than 5 fields");

      simLogCapacity_ = uint.Parse(fields[0].Trim());
      simLogHead_ = uint.Parse(fields[1].Trim());
      simLogTail_ = uint.Parse(fields[2].Trim());

      if (simLogCapacity_ > 65536)
        throw new InvalidOperationException("Log params header: capacity cannot exceed 64K");

      if (simLogHead_ >= simLogCapacity_)
        throw new InvalidOperationException("Log params header: head index must be less than capacity value");

      if (simLogTail_ >= simLogCapacity_)
        throw new InvalidOperationException("Log params header: tail index must be less than capacity value");

      simLogCurrentIndex_ = simLogHead_;

      simLogExpectedCount_ = (simLogHead_ < simLogTail_) ? simLogHead_ + (simLogCapacity_ - simLogTail_) + 1 : simLogHead_ - simLogTail_ + 1;

      if (simLogExpectedCount_ > simLogCapacity_)
        throw new InvalidOperationException("Log params header: head-to-tail inclusive count is greater than specified capacity");

      if (fields.Length > 3)
      {
        uint val = 0;

        if (uint.TryParse(fields[3].Trim(), out val))
          autoDownloadLogCountThreshold_ = val;
        else
          throw new InvalidOperationException("Log params header: Auto Download Log Count Threshold must be a nonnegative integer value");
      }

      if (fields.Length > 4)
      {
        uint val = 0;

        if (uint.TryParse(fields[4].Trim(), out val))
          tailAdjustThreshold_ = val;
        else
          throw new InvalidOperationException("Log params header: Tail Adjust Threshold must be a nonnegative integer value");
      }

      Debug.WriteLine("SIMLOG: capacity = {0}; head = {1}, tail = {2}, spanning {3} entries.", simLogCapacity_, simLogHead_, simLogTail_, simLogExpectedCount_);
      Debug.WriteLine("SIMLOG: auto download log count threshold = {0}", autoDownloadLogCountThreshold_);
      Debug.WriteLine("SIMLOG: tail adjust threshold = {0}", tailAdjustThreshold_);

      return true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="line"></param>
    /// 
    private void GetTestLogDataSpec(uint lineNum, string[] fields)
    {
      if (fields == null)
        throw new ArgumentNullException("fields");

      if (fields.Length != 2)
        throw new InvalidOperationException("Log entry spec must have 2 fields");

      string date = fields[0].Trim();

      DateTime dateForEntry;

      if (date == "-")
        dateForEntry = DateTime.MaxValue;
      else
      {
        DateTime entryDate;
        int day = 0;

        string[] formats = new string[]
        {
          "yyyy/MM/dd",
          "yyyy/MM/d",
          "yyyy/M/dd",
          "yyyy/M/d"
        };

        if (DateTime.TryParseExact(date, formats, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out entryDate))
        {
          if (entryDate != simLogCurrentTimestamp_.Date)
            simLogCurrentTimestamp_ = new DateTime(entryDate.Year, entryDate.Month, entryDate.Day, 12, 0, 0);
          else
            simLogCurrentTimestamp_ = simLogCurrentTimestamp_.AddSeconds(-1);
        }
        else if (int.TryParse(date, out day))
        {
          if (simLogCurrentTimestamp_ == DateTime.MaxValue)
            throw new InvalidOperationException("First log entry spec with valid date must have full date");

          if (day != simLogCurrentTimestamp_.Day)
            simLogCurrentTimestamp_ = new DateTime(simLogCurrentTimestamp_.Year, simLogCurrentTimestamp_.Month, day, 12, 0, 0);
          else
            simLogCurrentTimestamp_ = simLogCurrentTimestamp_.AddSeconds(-1);
        }
        else
          throw new InvalidOperationException("First field of log entry spec with valid date must be a full date yyyy/MM/dd or an integer day");

        dateForEntry = simLogCurrentTimestamp_;
      }

      byte token = 0;

      if (!byte.TryParse(fields[1].Trim(), out token))
        throw new InvalidOperationException("Second field of log entry spec must be a byte (checkpoint token value)");

      AURALogEntry entry = new AURALogEntry_Checkpoint(dateForEntry, token);

      cache_.Add(simLogCurrentIndex_, entry);

      Debug.Write(string.Format("SIMLOG: Entry: ord {0} ndx {1}: ", cache_.Count, simLogCurrentIndex_));

      if (dateForEntry == DateTime.MaxValue)
        Debug.Write("<invalid>");
      else
        Debug.Write(string.Format("{0:yyyy/MM/dd HH:mm:ss}", entry.UTCTimestamp));

      Debug.WriteLine(" - {0}", token);

      if (simLogCurrentIndex_ > 0)
        --simLogCurrentIndex_;
      else
        simLogCurrentIndex_ = simLogCapacity_ - 1;
    }

    #endregion

	public delegate void UploadHandler(object sender, EventArgs e);
	public event UploadHandler OnUpload;


	private void Upload()
	{
		// Make sure someone is listening to event
		if (OnUpload == null) return;

		EventArgs args = new EventArgs();
		OnUpload(this, args);
	}


	public void Func()
	{
		//SaveLogXlsx();
		Upload();
	}

	private void btnUploadNow_Click(object sender, RoutedEventArgs e)
	{
		SaveLogXlsx();
		Thread.Sleep(5000);
		Upload();
	}
  }
}
