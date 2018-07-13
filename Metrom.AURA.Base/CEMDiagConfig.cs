using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Metrom.AURA.Base
{


  /// <summary>
  /// 
  /// </summary>
  /// 
  public class CEMDiagConfig
  {
    public const ushort kCEMDiagConfigSize = 4;

    public CEMDiagOption Options
    { get; set; }

    public bool IsExtendedConfig
    { get; private set; }

    public CEMDiagConfig()
    {
      IsExtendedConfig = true;
    }

    public CEMDiagConfig(byte[] buf, ushort ofs, ushort len)
    {
      if (buf == null)
        throw new ArgumentNullException("buf");
      if ((len != kCEMDiagConfigSize) && (len != 2))
        throw new ArgumentException(string.Format("Length must be 2 (legacy) or {0} bytes", kCEMDiagConfigSize));

      ushort ndx = ofs;

      // Prior to CEM v. 2.11.1, payload was a single uint16_t.

      if (len == 2)
      {
        IsExtendedConfig = false;

        Options = (CEMDiagOption)BitConverter.ToUInt16(buf, ndx);
        ndx += 2;
      }
      else
      {
        IsExtendedConfig = true;

        Options = (CEMDiagOption)BitConverter.ToUInt32(buf, ndx);
        ndx += 4;
      }

      ushort expectedLen = IsExtendedConfig ? kCEMDiagConfigSize : (ushort)2;
      ushort calcLen = (ushort)(ndx - ofs);

      if (expectedLen != calcLen)
        throw new ApplicationException(string.Format("CEMDiagConfig(): unpacked {0} bytes, but expected {1} bytes", calcLen, expectedLen));
    }

    public CEMDiagConfig(CEMDiagConfig src)
    {
      if (src == null)
        throw new ArgumentNullException("src");

      Options = src.Options;
      IsExtendedConfig = src.IsExtendedConfig;
    }

    public void PackBuffer(byte[] buf, ushort ofs)
    {
      ushort ndx = ofs;

      if (IsExtendedConfig)
      {
        BitConverter.GetBytes((uint)Options).CopyTo(buf, ndx);
        ndx += 4;
      }
      else
      {
        BitConverter.GetBytes((ushort)Options).CopyTo(buf, ndx);
        ndx += 2;
      }

      ushort expectedLen = IsExtendedConfig ? kCEMDiagConfigSize : (ushort)2;
      ushort calcLen = (ushort)(ndx - ofs);

      if (expectedLen != calcLen)
        throw new ApplicationException(string.Format("CEMDiagConfig.PackBuffer(): packed len {0} does not match expected {1}", calcLen, expectedLen));
    }
  }
}
