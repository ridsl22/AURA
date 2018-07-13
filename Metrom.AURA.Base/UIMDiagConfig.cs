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
  public class UIMDiagConfig
  {
    public const ushort kUIMDiagConfigSize = 4;

    public UIMDiagOption Options
    { get; set; }

    public UIMDiagConfig()
    {
    }

    public UIMDiagConfig(byte[] buf, ushort ofs, ushort len)
    {
      if (buf == null)
        throw new ArgumentNullException("buf");
      if (len != kUIMDiagConfigSize)
        throw new ArgumentException("Length must be " + kUIMDiagConfigSize);

      ushort ndx = ofs;

      Options = (UIMDiagOption)BitConverter.ToUInt32(buf, ndx);
      ndx += 4;

      if ((ndx - ofs) != kUIMDiagConfigSize)
        throw new ApplicationException(string.Format("UIMDiagConfig(): unpacked {0} bytes, but should have unpacked {1} bytes", ndx - ofs, kUIMDiagConfigSize));
    }

    public UIMDiagConfig(UIMDiagConfig src)
    {
      if (src == null)
        throw new ArgumentNullException("src");

      Options = src.Options;
    }

    public void PackBuffer(byte[] buf, ushort ofs)
    {
      ushort ndx = ofs;

      BitConverter.GetBytes((uint)Options).CopyTo(buf, ndx);
      ndx += 4;

      if ((ndx - ofs) != kUIMDiagConfigSize)
        throw new ApplicationException(string.Format("UIMDiagConfig.PackBuffer(): packed len {0} does not match expected {1}", ndx - ofs, kUIMDiagConfigSize));
    }
  }
}
