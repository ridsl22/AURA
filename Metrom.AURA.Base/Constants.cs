using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Metrom.AURA.Base
{


  /// <summary>
  /// Peer direction relative to vehicle reference frame.
  /// </summary>
  /// 
  public enum SenseSide
  {
    /// <summary>
    /// Peer is to the front of the vehicle.
    /// </summary>
    Front = 0,

    /// <summary>
    /// Peer is to the rear of the vehicle.
    /// </summary>
    Rear = 1
  }


  public enum DistanceZone
  {
    None = 0,

    Approach = 1,

    Inside = 2,

    Critical = 3
  }


  public enum SpeedMode
  {
    Travel = 0,

    Work = 1,

    Crawl = 2
  }


  [Flags]
  public enum AURACaps : ushort
  {
    FourCharName = (1 << 0)
  }


  public static class UIM
  {
    public static string AlarmLevelToString(UIMAlarmLevel level)
    {
      string txt = "<??>";

      switch (level)
      {
      case UIMAlarmLevel.None:
        txt = "Cleared";
        break;

      case UIMAlarmLevel.Alert:
        txt = "Alert";
        break;

      case UIMAlarmLevel.InsideAlert:
        txt = "Inside Alert";
        break;

      case UIMAlarmLevel.Alarm:
        txt = "Alarm";
        break;

      case UIMAlarmLevel.Acknowledged:  // USED ONLY FOR LOGGING!
        txt = "Silenced";
        break;

      case UIMAlarmLevel.Incident:  // USED ONLY FOR LOGGING!
        txt = "Incident";
        break;

      case UIMAlarmLevel.Gps:  // USED ONLY FOR XLSX EXPORT!
        txt = "-";
        break;
      }

      return txt;
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public static class CEM
  {
    public const ushort LogEntryPayloadSize = 26;

    public const ushort LogEntryTypeLen = 1;

    public static string LogEntryTypeToString(CEMLogEntryType entryType)
    {
      string txt = "<??>";

      switch (entryType)
      {
      case CEMLogEntryType.Unknown:
        txt = "<unknown>";
        break;

      case CEMLogEntryType.Startup:
        txt = "Startup";
        break;

      case CEMLogEntryType.StartupFirstFix:
        txt = "GPS TTFF";
        break;

      case CEMLogEntryType.SubSystemStatus:
        txt = "SubSys Status";
        break;

      case CEMLogEntryType.StartupFirstFixV2:
        txt = "GPS TTFF";
        break;

      case CEMLogEntryType.LogCleared:
        txt = "Log Cleared";
        break;

      case CEMLogEntryType.GeneralStatus:
        txt = "General Status";
        break;

      case CEMLogEntryType.GPSData:
        txt = "GPS Data";
        break;

      case CEMLogEntryType.Checkpoint:
        txt = "Checkpoint";
        break;

      case CEMLogEntryType.NewAlarmOldEntry:
        txt = "New Alarm (old)";
        break;

      case CEMLogEntryType.AlarmSilencedOld:
        txt = "Alarm Silenced (old)";
        break;

      case CEMLogEntryType.UIMStartup:
        txt = "UIMStartup";
        break;

      case CEMLogEntryType.AlarmSettings_Old:
        txt = "Alarm Settings";
        break;

      case CEMLogEntryType.AlarmEvent:
        txt = "Alarm Event";
        break;

      case CEMLogEntryType.AlarmPeerData:
        txt = "Alarm Peer Data";
        break;

      case CEMLogEntryType.AlarmPeerData2:
        txt = "Alarm Peer Data 2";
        break;

      case CEMLogEntryType.AlarmPeerData3:
        txt = "Alarm Peer Data 3";
        break;

      case CEMLogEntryType.AlarmSettingsCurrent_Old:
        txt = "Alarm Settings Current (Old)";
        break;

      case CEMLogEntryType.AlarmSettingsNew_Old:
        txt = "Alarm Settings New (Old)";
        break;

      case CEMLogEntryType.UnitConfigCurrent_Old:
        txt = "Unit Config Current (Old)";
        break;

      case CEMLogEntryType.UnitConfigNew_Old:
        txt = "Unit Config New (Old)";
        break;

      case CEMLogEntryType.UnitConfigCurrent:
        txt = "Unit Config Current";
        break;

      case CEMLogEntryType.UnitConfigNew:
        txt = "Unit Config New";
        break;

      case CEMLogEntryType.PPSStateChange:
        txt = "PPS State Change";
        break;

      case CEMLogEntryType.AutoBrakeEngage:
        txt = "AUTOBRAKE ENGAGED";
        break;

      case CEMLogEntryType.AutoBrakeRelease:
        txt = "Autobrake Acknowledged";
        break;

      case CEMLogEntryType.AlarmSettingsCurrent:
        txt = "Alarm Settings Current";
        break;

      case CEMLogEntryType.AlarmSettingsNew:
        txt = "Alarm Settings New";
        break;

      case CEMLogEntryType.IncidentNote:
        txt = "Incident Note";
        break;

      case CEMLogEntryType.UIMDebug:
        txt = "UIM Debug";
        break;

      case CEMLogEntryType.Fault:
        txt = "Fault";
        break;

      case CEMLogEntryType.Incident:
        txt = "Incident";
        break;

      case CEMLogEntryType.IncidentData1:
        txt = "Incident Data 1";
        break;

      case CEMLogEntryType.IncidentData2:
        txt = "Incident Data 2";
        break;

      case CEMLogEntryType.IncidentPeerData:
        txt = "Incident Peer Data";
        break;
      }

      return txt;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    /// 
    public static string ToText(CEMCommsStatus status)
    {
      string txt = "<??>";

      switch (status)
      {
      case CEMCommsStatus.NotReady:
        txt = "Not Ready";
        break;

      case CEMCommsStatus.Ready:
        txt = "Ready";
        break;

      case CEMCommsStatus.InvalidStatusValue:
        txt = "";
        break;
      }

      return txt;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    /// 
    public static string ToText(CEMGPSStatus status)
    {
      string txt = "<??>";

      switch (status)
      {
      case CEMGPSStatus.NoComm:
        txt = "No Comm";
        break;

      case CEMGPSStatus.CommOk:
        txt = "Comm Ok";
        break;

      case CEMGPSStatus.FixLost:
        txt = "No Fix";
        break;

      case CEMGPSStatus.FixGood:
        txt = "Fix Good";
        break;

      case CEMGPSStatus.InvalidStatusValue:
        txt = "";
        break;
      }

      return txt;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    /// 
    public static string ToText(CEMRCMStatus status)
    {
      string txt = "<??>";

      switch (status)
      {
      case CEMRCMStatus.NoComm:
        txt = "No Comm";
        break;

      case CEMRCMStatus.CommOk:
        txt = "Comm Ok";
        break;

      case CEMRCMStatus.InvalidStatusValue:
        txt = "";
        break;
      }

      return txt;
    }


  }


  public enum CEMLogEntryType : byte
  {
    Unknown = 0xff,
    // Main Log ---------------------------------
    // CEM:
    Startup = 0x00,
    StartupFirstFix = 0x01,
    SubSystemStatus = 0x02,
    StartupFirstFixV2 = 0x03,
    LogCleared = 0x10,
    GeneralStatus = 0x20,
    GPSData = 0xa0,
    //
    Checkpoint = 0x70,
    // UIM:
    NewAlarmOldEntry = 0x80,  // deprecated
    AlarmSilencedOld = 0x81,  // deprecated
    UIMStartup = 0x82,
    AlarmSettings_Old = 0x83,  // deprecated
    AlarmEvent = 0x84,
    AlarmPeerData = 0x85,
    AlarmPeerData2 = 0x86,
    IncidentNote = 0x87,
    UIMDebug = 0x88,
    AlarmPeerData3 = 0x89,
    AlarmSettingsCurrent_Old = 0x8a,  // deprecated
    AlarmSettingsNew_Old = 0x8b,  // deprecated
    UnitConfigCurrent_Old = 0x8c,  // deprecated
    UnitConfigNew_Old = 0x8d,  // deprecated
    AutoBrakeEngage = 0x8e,
    AutoBrakeRelease = 0x8f,
    AlarmSettingsCurrent = 0x90,
    AlarmSettingsNew = 0x91,
    UnitConfigCurrent = 0x92,
    UnitConfigNew = 0x93,
    //
    PPSStateChange = 0xb0,
    //
    // Incident Log -----------------------------
    Fault = 0xd0,
    Incident = 0xd1,
    IncidentData1 = 0xd2,
    IncidentData2 = 0xd3,
    IncidentPeerData = 0xd4,
    //
    // Gen 1 Backward compatibility
    g1Fault = 0x00,
    g1Incident = 0x01,
    g1IncidentData1 = 0x02,
    g1IncidentData2 = 0x03,
    g1IncidentPeerData = 0x04
  }


  [Flags]
  public enum CEMIncidentEntryFlag : byte
  {
    IncidentStart = (1 << 0),
    HasDetail = (1 << 1)
  }


  public enum CEMIncidentTrigger : byte
  {
    InvalidTriggerValue = 0xff,

    None = 0,
    Peak = 1,
    Duration = 2
  }


  //-----------------------------------------------------------------------------------------------


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum RCMRangeResult : byte
  {
    Timeout = 0,
    Failed = 1,
    GotCoarseRange = 2,
    GotRange = 3,
    LostComm = 100,
    InternalError = 101,
    InvalidRangeResultValue = 0xff
  }


  //-----------------------------------------------------------------------------------------------


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum CEMSubSystem : byte
  {
    InvalidSubSystemValue = 0xff,

    Comms = 0,
    GPS = 1,
    RCM = 2
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum CEMCommsStatus : byte
  {
    InvalidStatusValue = 0xff,

    NotReady = 0,
    Ready = 1
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum CEMGPSStatus : byte
  {
    InvalidStatusValue = 0xff,

    NoComm = 0,
    CommOk = 1,
    FixGood = 2,
    FixLost = 3
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum CEMRCMStatus : byte
  {
    InvalidStatusValue = 0xff,

    NoComm = 0,
    CommOk = 1
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum SeparationSource
  {
    Unknown = 0,
    RCM = 1,
    GPS = 2
  }


  /// <summary>
  /// For '0 distance' issue debug only!
  /// </summary>
  public static class RCMCheckToken
  {
    public const uint DataOk = 0x193b5d7f;
    public const uint DataIsZero = 0x082a4c6e;
    public const uint DataIsHuge = 0xdeadbeef;
    public const uint DataTypeUnexpected = 0xfedcba98;
    public const uint NoData = 0x01234567;
    //
    // Set in Main task, not RCM task:
    public const uint DataDiscarded_HighREE = 0x11335577;
    public const uint DataDiscarded_UnexpectedRangeChange = 0x22446688;
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum UISecurityOption : byte
  {
    UnlockPrivateMenus = 1,
    UnlockCustomerMenus = 2
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum UIMAlarmLevel : byte
  {
    None = 0,
    Alert,
    InsideAlert,
    Alarm,
    Acknowledged,  // USED ONLY FOR LOGGING!
    Incident,  // USED ONLY FOR LOGGING!
    Gps,  // USED ONLY FOR XLSX EXPORT!
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  [Flags]
  public enum CEMDiagOption : uint
  {
    None = 0,
    EnableAccelCapture = (1 << 0),
    EnablePeerDataOutput = (1 << 1),
    EnableRangeDataOutput = (1 << 2),
    EnableRangeDebugOutput = (1 << 3),
    EnableUIDataUpdateOutput = (1 << 4),  // DEPRECATED
    EnableGPSDataOutput = (1 << 5),
    EnableKillGPS = (1 << 6),
    EnableOADataOutput = (1 << 7),
    PinGPS = (1 << 8),  // CEM v. 2.9.0+
    KillBeacon = (1 << 9),  // CEM v. 2.11.0+
    IgnoreBeacons = (1 << 10),  // CEM v. 2.11.0+
    DoNotDiscardRangeData = (1 << 11),  // CEM v. 2.11.0+
    SendRCMScanData = (1 << 12),  // CEM v. 2.11.0+
    ExerciseGPSMath = (1 << 13)  // CEM v. 2.15.0+
  }


  /// <summary>
  /// 
  /// </summary>
  /// 
  [Flags]
  public enum UIMDiagOption : uint
  {
    None = 0,
    EnableDataUpdatePassThru = (1 << 0),
    DisableNoMotionNAS = (1 << 1)  // UIM 2.17.0+
  }


}
