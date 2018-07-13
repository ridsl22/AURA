using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IDI.Math;
using Metrom.AURA.Base;


namespace MetromTablet.Communication
{


	[Flags]
	public enum CEMConfigFlag1 : ushort
	{
		AuxConfirmEnabled = (1 << 0),
		UseAURAEnableInput = (1 << 1),
		GPSLoggingEnabled = (1 << 2),
		DEPRECATED_UIMOptional = (1 << 3),
		DEPRECATED_VehicleTurntable = (1 << 4),
		PII8DateUpdateDone = (1 << 5),
		TowModeEnabled = (1 << 6),
		TowFromFront = (1 << 7),
		//
		EnableRCMCRE = (1 << 14),
		ForceUnknownOrientation = (1 << 15)
	}


	public class CEMConfig1
	{
		public const ushort kCEMConfigSize = 92;

		public CEMConfigFlag1 Flags
		{ get; set; }

		public byte DNTNetworkId
		{ get; set; }

		public byte DNTXmitPower
		{ get; set; }

		public byte RCMXmitPower
		{ get; set; }

		public byte RCMPII
		{ get; set; }

		public AURACaps Caps
		{ get; private set; }

		public float FrontAntennaOffset
		{ get; set; }

		public float RearAntennaOffset
		{ get; set; }

		public string NamePrefix
		{ get; set; }

		public uint NameNumber
		{ get; set; }

		public string Name
		{ get { return NamePrefix + NameNumber.ToString("d6"); } }

		public Vector AccelOffset
		{ get; set; }

		public Vector AccelOffsetUnit
		{ get; set; }

		public Vector AccelFrontUnit
		{ get; set; }

		public string DualAURADisableAddr
		{ get; set; }

		public float TowingOffsetAdjustment
		{ get; set; }

		public CEMConfig1()
		{
			NamePrefix = "  ";

			AccelOffset = new Vector(3);
			AccelOffsetUnit = new Vector(3);
			AccelFrontUnit = new Vector(3);
		}

		public CEMConfig1(byte[] buf, ushort ofs, ushort len)
		{
			if (buf == null)
				throw new ArgumentNullException("buf");
			if (len != kCEMConfigSize)
				throw new ArgumentException("Length must be " + kCEMConfigSize.ToString());

			ushort ndx = ofs;

			Flags = (CEMConfigFlag1)BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			DNTNetworkId = buf[ndx++];
			DNTXmitPower = buf[ndx++];

			RCMXmitPower = buf[ndx++];
			RCMPII = buf[ndx++];

			Caps = (AURACaps)BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			FrontAntennaOffset = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
			if (FrontAntennaOffset > (250 * 0.3))
				FrontAntennaOffset = (float)(250 * 0.3);
			else if (FrontAntennaOffset < 0.0)
				FrontAntennaOffset = 0.0f;

			RearAntennaOffset = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
			if (RearAntennaOffset > (250 * 0.3))
				RearAntennaOffset = (float)(250 * 0.3);
			else if (RearAntennaOffset < 0.0)
				RearAntennaOffset = 0.0f;

			NamePrefix = ASCIIEncoding.ASCII.GetString(buf, ndx, 2);
			ndx += 2;

			if ((Caps & AURACaps.FourCharName) != 0)
			{
				// If the CEM has just been updated to a version that supports 4-char vehicle names,
				// the 3rd and 4th char in the CEM config structure will still contain zeroes. Change
				// these (if present) to spaces - unused chars in the name are represented by spaces
				// and aren't shown to the user anywhere (they're analagous to nulls in strings).

				if (buf[ndx] == 0)
					buf[ndx] = 32;  // ASCII space
				if (buf[ndx + 1] == 0)
					buf[ndx + 1] = 32;

				NamePrefix += ASCIIEncoding.ASCII.GetString(buf, ndx, 2).Trim();
			}

			ndx += 2;  // 2 bytes reserved or 2 add'l chars of name prefix

			NameNumber = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			AccelOffset = new Vector(3);

			AccelOffset[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelOffset[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelOffset[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelOffsetUnit = new Vector(3);

			AccelOffsetUnit[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelOffsetUnit[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelOffsetUnit[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelFrontUnit = new Vector(3);

			AccelFrontUnit[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelFrontUnit[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AccelFrontUnit[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			StringBuilder hexAddress = new StringBuilder(8);
			for (int i = ndx + 3 - 1; i >= ndx; i--)
				hexAddress.AppendFormat("{0:x2}", buf[i]);
			DualAURADisableAddr = hexAddress.ToString().ToUpper();// BitConverter.ToUInt32(buf, ndx);
			
			ndx += 4;

			TowingOffsetAdjustment = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			ndx += 24;  // 24 bytes reserved

			if ((ndx - ofs) != kCEMConfigSize)
				throw new ApplicationException(string.Format("CEMConfig(): unpacked {0} bytes, but should have unpacked {1} bytes", ndx - ofs, kCEMConfigSize));
		}

		public void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			BitConverter.GetBytes((ushort)Flags).CopyTo(buf, ndx);
			ndx += 2;

			buf[ndx++] = DNTNetworkId;
			buf[ndx++] = DNTXmitPower;

			buf[ndx++] = RCMXmitPower;
			buf[ndx++] = RCMPII;

			buf[ndx++] = 0;  // 2 bytes reserved
			buf[ndx++] = 0;

			BitConverter.GetBytes(FrontAntennaOffset).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes(RearAntennaOffset).CopyTo(buf, ndx);
			ndx += 4;

			byte[] prefixBytes = ASCIIEncoding.ASCII.GetBytes(NamePrefix);

			prefixBytes.CopyTo(buf, ndx);
			ndx += (ushort)prefixBytes.Length;

			if ((Caps & AURACaps.FourCharName) != 0)
			{
				if (prefixBytes.Length > 4)
					throw new InvalidOperationException("CEMConfig.PackBuffer: name prefix is " + prefixBytes.Length.ToString() + " bytes (> 4!)");

				if (prefixBytes.Length < 4)
				{
					for (int i = 4 - prefixBytes.Length; i > 0; --i)
					{
						buf[ndx++] = 0;
					}
				}
			}
			else
			{
				if (prefixBytes.Length > 2)
					throw new InvalidOperationException("CEMConfig.PackBuffer: name prefix is " + prefixBytes.Length.ToString() + " bytes (> 2!)");

				if (prefixBytes.Length < 2)
					buf[ndx++] = 0;
				if (prefixBytes.Length < 1)
					buf[ndx++] = 0;

				buf[ndx++] = 0;  // 2 bytes reserved
				buf[ndx++] = 0;
			}

			BitConverter.GetBytes(NameNumber).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)AccelOffset[0]).CopyTo(buf, ndx);
			ndx += 4;
			BitConverter.GetBytes((float)AccelOffset[1]).CopyTo(buf, ndx);
			ndx += 4;
			BitConverter.GetBytes((float)AccelOffset[2]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)AccelOffsetUnit[0]).CopyTo(buf, ndx);
			ndx += 4;
			BitConverter.GetBytes((float)AccelOffsetUnit[1]).CopyTo(buf, ndx);
			ndx += 4;
			BitConverter.GetBytes((float)AccelOffsetUnit[2]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)AccelFrontUnit[0]).CopyTo(buf, ndx);
			ndx += 4;
			BitConverter.GetBytes((float)AccelFrontUnit[1]).CopyTo(buf, ndx);
			ndx += 4;
			BitConverter.GetBytes((float)AccelFrontUnit[2]).CopyTo(buf, ndx);
			ndx += 4;

			byte[] address = Enumerable.Range(0, DualAURADisableAddr.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(DualAURADisableAddr.Substring(x, 2), 16))
                     .ToArray();
			for (int i = 2; i >= 0; i--)
				buf[ndx++] = address[i];
			buf[ndx++] = 0;
			//BitConverter.GetBytes(DualAURADisableAddr).CopyTo(buf, ndx);
			//ndx += 4;

			BitConverter.GetBytes(TowingOffsetAdjustment).CopyTo(buf, ndx);
			ndx += 4;

			// 24 bytes reserved

			for (int n = 0; n < 24; ++n)
				buf[ndx++] = 0;

			if ((ndx - ofs) != kCEMConfigSize)
				throw new ApplicationException(string.Format("CEMConfig.PackBuffer(): packed len {0} does not match expected {1}", ndx - ofs, kCEMConfigSize));
		}
	}


}
