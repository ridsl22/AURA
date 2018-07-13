using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Metrom.AURA.Base
{


  public class CEMInternals
  {
    public const ushort kCEMInternalsSize = 32;

    public uint Cookie
    { get; set; }

    public float OAStartVelThreshold
    { get; set; }

    public float OAGoodDecelThreshold
    { get; set; }

    public byte OAGoodDecelCount
    { get; set; }

    public byte MAX_UNEXPECTED_HEADING_COUNT
    { get; set; }

    public float ACCEL_FILTER_WEIGHT
    { get; set; }

    public float ACCEL_FRONT_CALIB_MIN_MAG
    { get; set; }

    public byte ACCEL_FRONT_CALIB_COUNT
    { get; set; }

    public float ACCEL_INCIDENT_MAG
    { get; set; }

    public byte ACCEL_INCIDENT_COUNT
    { get; set; }

    public float ACCEL_INCIDENT_PEAK_MAG
    { get; set; }

    public CEMInternals()
    {
    }

    public CEMInternals(byte[] buf, ushort ofs, ushort len)
    {
      if (buf == null)
        throw new ArgumentNullException("buf");
      if (len != kCEMInternalsSize)
        throw new ArgumentException("Length must be " + kCEMInternalsSize.ToString());

      ushort ndx = ofs;

      Cookie = BitConverter.ToUInt32(buf, ndx);
      ndx += 4;

      OAStartVelThreshold = BitConverter.ToSingle(buf, ndx);
      ndx += 4;

      OAGoodDecelThreshold = BitConverter.ToSingle(buf, ndx);
      ndx += 4;

      OAGoodDecelCount = buf[ndx++];
      MAX_UNEXPECTED_HEADING_COUNT = buf[ndx++];
      ACCEL_FRONT_CALIB_COUNT = buf[ndx++];
      ACCEL_INCIDENT_COUNT = buf[ndx++];

      ACCEL_FILTER_WEIGHT = BitConverter.ToSingle(buf, ndx);
      ndx += 4;

      ACCEL_FRONT_CALIB_MIN_MAG = BitConverter.ToSingle(buf, ndx);
      ndx += 4;

      ACCEL_INCIDENT_MAG = BitConverter.ToSingle(buf, ndx);
      ndx += 4;

      ACCEL_INCIDENT_PEAK_MAG = BitConverter.ToSingle(buf, ndx);
      ndx += 4;

      if ((ndx - ofs) != kCEMInternalsSize)
        throw new ApplicationException(string.Format("CEMInternals(): unpacked {0} bytes, but should unpack {1} bytes", ndx - ofs, kCEMInternalsSize));
    }

    public void PackBuffer(byte[] buf, ushort ofs)
    {
      ushort ndx = ofs;

      // Set Cookie bytes to zero - the cookie field is IGNORED by the CEM when setting
      // Internals values from the PC.

      buf[ndx++] = 0;
      buf[ndx++] = 0;
      buf[ndx++] = 0;
      buf[ndx++] = 0;

      BitConverter.GetBytes(OAStartVelThreshold).CopyTo(buf, ndx);
      ndx += 4;

      BitConverter.GetBytes(OAGoodDecelThreshold).CopyTo(buf, ndx);
      ndx += 4;

      buf[ndx++] = OAGoodDecelCount;
      buf[ndx++] = MAX_UNEXPECTED_HEADING_COUNT;
      buf[ndx++] = ACCEL_FRONT_CALIB_COUNT;
      buf[ndx++] = ACCEL_INCIDENT_COUNT;

      BitConverter.GetBytes(ACCEL_FILTER_WEIGHT).CopyTo(buf, ndx);
      ndx += 4;

      BitConverter.GetBytes(ACCEL_FRONT_CALIB_MIN_MAG).CopyTo(buf, ndx);
      ndx += 4;

      BitConverter.GetBytes(ACCEL_INCIDENT_MAG).CopyTo(buf, ndx);
      ndx += 4;

      BitConverter.GetBytes(ACCEL_INCIDENT_PEAK_MAG).CopyTo(buf, ndx);
      ndx += 4;

      if ((ndx - ofs) != kCEMInternalsSize)
        throw new ApplicationException(string.Format("CEMInternals.PackBuffer(): packed len {0} does not match expected {1}", ndx - ofs, kCEMInternalsSize));
    }
  }


}
