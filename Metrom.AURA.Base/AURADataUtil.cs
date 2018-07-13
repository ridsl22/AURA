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
  public static class AURADataUtil
  {
    public static string UnpackName(byte[] buf, ushort ofs)
    {
      return UnpackName(BitConverter.ToUInt32(buf, ofs));
    }


    public static string UnpackName(uint packedName)
    {
      char[] prefix = new char[2];

      prefix[0] = DecodeNameChar((byte)((packedName >> 22) & 0x1f));
      prefix[1] = DecodeNameChar((byte)((packedName >> 27) & 0x1f));

      return new string(prefix) + (packedName & 0x000fffff).ToString("d6");
    }


    public static string UnpackName(uint packedName, byte nameChar3, byte nameChar4)
    {
      char[] prefix = new char[4];
      int len = 2;

      prefix[0] = DecodeNameChar((byte)((packedName >> 22) & 0x1f));
      prefix[1] = DecodeNameChar((byte)((packedName >> 27) & 0x1f));

      //if ((nameChar3 != 26) && (nameChar3 <= 27))
      //  prefix[len++] = DecodeNameChar(nameChar3);
      if (nameChar3 != 26)
        prefix[len++] = DecodeNameChar(nameChar3);

      //if ((nameChar4 != 26) && (nameChar4 <= 27))
      //  prefix[len++] = DecodeNameChar(nameChar4);
      if (nameChar4 != 26)
        prefix[len++] = DecodeNameChar(nameChar4);

      return new string(prefix, 0, len) + (packedName & 0x000fffff).ToString("d6");
    }


    public static bool PackedNameHas4CharIndicator(uint packedName)
    {
      return (packedName & (1 << 20)) != 0;
    }


    public static uint PackName(byte[] prefix, uint number)
    {
      if (prefix == null)
        throw new ArgumentNullException("prefix");
      if (prefix.Length < 2)
        throw new ArgumentException("prefix must be length >= 2");

      uint packedName = number & 0x000fffff;

      packedName |= (1 << 20);

      packedName |= ((uint)EncodeNameChar(prefix[0]) << 22);
      packedName |= ((uint)EncodeNameChar(prefix[1]) << 27);

      return packedName;
    }


    public static uint UnpackDNTAddress(byte[] buf, ushort ofs)
    {
      return (uint)(buf[ofs] | (buf[ofs + 1] << 8) | (buf[ofs + 2] << 16));
    }


    public static void PackDNTAddress(byte[] buf, ushort ofs, uint addr)
    {
      buf[ofs++] = (byte)(addr & 0xff);
      buf[ofs++] = (byte)((addr >> 8) & 0xff);
      buf[ofs++] = (byte)((addr >> 16) & 0xff);
    }


    public static char DecodeNameChar(byte val)
    {
      if (val < 26)
        val += 0x41;  // 'A' + (0 to 25)
      else if (val == 26)
        val = 0x20;  // space ('not used' symbol)
      else if (val == 27)
        val = 0x26;  // ampersand '&'
      else if (val == 28)
        val = 0x5f;  // underscore '_'
      else if (val == 29)
        val = 0x2d;  // dash '-'
      else if (val == 30)
        val = 0x2e;  // period '.'
      else if (val == 31)
        val = 0x23;  // hash '#'
      else
        val = 0x3f;  // unknown - question mark - should never occur

      return ASCIIEncoding.ASCII.GetChars(new byte[] { val })[0];
    }


    public static byte EncodeNameChar(byte val)
    {
      byte eVal;

      if ((val >= 0x41) && (val <= 0x5a))  // 'A' - 'Z'
        eVal = (byte)(val - 0x41);
      else if (val == 0x20)  // space ('not used' symbol)
        eVal = 26;
      else if (val == 0x26)  // ampersand '&'
        eVal = 27;
      else if (val == 0x5f)  // underscore '_'
        eVal = 28;
      else if (val == 0x2d)  // dash '-'
        eVal = 29;
      else if (val == 0x2e)  // period '.'
        eVal = 30;
      else if (val == 0x23)  // hash '#'
        eVal = 31;
      else
        eVal = 26;  // unknown - space / unused symbol - should never occur

      return eVal;
    }


    /// <summary>
    /// Packs string data into a fixed-width field in a byte buffer. If isNullTerminated is true,
    /// max string length is maxFieldLen minus one (due to the null termination), otherwise it's
    /// maxFieldLen. If less data is supplied than the maximum, the remainder of the field will
    /// be zero-filled.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="maxFieldLen"></param>
    /// <param name="buf"></param>
    /// <param name="ofs"></param>
    /// <param name="isNullTerminated"></param>
    /// 
    public static void PackString(string data, ushort maxFieldLen, byte[] buf, ushort ofs, bool isNullTerminated = false)
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (maxFieldLen == 0)
        throw new ArgumentException("maxFieldLen must be positive", "maxFieldLen");
      if (buf == null)
        throw new ArgumentNullException("buf");
      if ((ofs + maxFieldLen) > buf.Length)
        throw new ArgumentException(string.Format("Length of supplied buffer is insufficient: ofs ({0}) + maxFieldLen ({1}) > buf.Length ({2})", ofs, maxFieldLen, buf.Length));

      // The maximum permitted string data length depends on the state of the isNullTerminated
      // option: if it's false, the maximum length is the size of the field, maxFieldLen; if
      // it's true, the maximum length is one less than the field size (to accommodate the
      // null terminator).

      int maxStringLen = isNullTerminated ? maxFieldLen - 1 : maxFieldLen;

      // Calculate the number of bytes to copy: if the string data length exceeds the number of
      // ASCII characters we can store, cap the data length to that maximum; otherwise, the
      // number of bytes to copy is the same as the string length.

      int copyLen = (data.Length > maxStringLen) ? maxStringLen : data.Length;

      // Encode the supplied Unicode string data and save in the supplied buffer as ASCII.

      ASCIIEncoding.ASCII.GetBytes(data, 0, copyLen, buf, ofs);

      // Pad any remaining space in the buffer with zeroes.

      while (copyLen < maxFieldLen)
        buf[ofs + copyLen++] = 0;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxFieldLen"></param>
    /// <param name="buf"></param>
    /// <param name="ofs"></param>
    /// <param name="isNullTerminated"></param>
    /// <returns></returns>
    /// 
    public static string UnpackString(ushort maxFieldLen, byte[] buf, ushort ofs, bool isNullTerminated = false)
    {
      if (maxFieldLen == 0)
        throw new ArgumentException("maxFieldLen must be positive", "maxFieldLen");
      if (buf == null)
        throw new ArgumentNullException("buf");
      if ((ofs + maxFieldLen) > buf.Length)
        throw new ArgumentException(string.Format("Length of supplied buffer is insufficient: ofs ({0}) + maxFieldLen ({1}) > buf.Length ({2})", ofs, maxFieldLen, buf.Length));

      // Figure out how much data there is to copy.

      int dataLen = 0;
      while ((buf[ofs + dataLen] != 0) && (dataLen < maxFieldLen))
        ++dataLen;

      return ASCIIEncoding.ASCII.GetString(buf, ofs, dataLen);
    }

  }


}
