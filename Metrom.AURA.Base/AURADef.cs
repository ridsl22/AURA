using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Metrom.AURA.Base
{


  /// <summary>
  /// Various and sundry AURA constants.
  /// </summary>
  /// 
  public static class AURADef
  {
    public const uint BinaryLogFileMagicWord = 0x4c42524d;  // 'MRBL', swapped for endian-ness
    public const byte BinaryLogFileVersion = 2;  // 0 is invalid

    public const uint BinaryProfileSetMagicWord = 0x5350524d;  // 'MRPS', swapped for endian-ness

    // Note that log entry opcode is actually part of the entry payload.
    public const byte LogEntryPayloadLen = 26;

    // Size of the header written with each log entry in BLF files - this is NOT identical to
    // the log entry header length used in AURA firmware!
    public const int BLFLogEntryHeaderLen = 5;

    public const uint InvalidMACAddress = 0xffffff;  // three bytes / six hex digits

    public static int kDistanceZoneCount = 3;

    public static int kSpeedModeCount = 3;
  }


}
