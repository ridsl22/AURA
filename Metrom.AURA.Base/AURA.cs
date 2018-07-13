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
  public static class AURA
  {
    public const uint BinaryLogFileMagicWord = 0x4c42524d;  // 'MRBL', swapped for endian-ness
    public const byte BinaryLogFileVersion = 1;
    public const byte LogEntryPayloadLen = 26;

    public const uint InvalidMACAddress = 0xffffff;  // three bytes / six hex digits

    public static int kDistanceZoneCount = 3;

    public static int kSpeedModeCount = 3;
  }


}
