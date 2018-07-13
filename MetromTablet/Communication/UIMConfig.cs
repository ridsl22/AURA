using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Metrom.AURA.Base;


namespace MetromTablet.Communication
{


	public class UIMConfigMode
	{
		private double[] limits_ = new double[AURADef.kDistanceZoneCount];

		public double EntrySpeed
		{ get; set; }

		public double Hysteresis
		{ get; set; }

		public double this[DistanceZone zone]
		{
			get
			{
				if (zone == DistanceZone.None)
					throw new InvalidOperationException("Cannot specify None as a distance zone");

				return limits_[(int)zone - 1];
			}
			set
			{
				if (zone == DistanceZone.None)
					throw new InvalidOperationException("Cannot specify None as a distance zone");

				limits_[(int)zone - 1] = value;
			}
		}
	}


	/// <summary>
	/// 
	/// </summary
	/// >
	public class UIMConfig
	{
		public const ushort kUIMConfigSize = 62;

		private UIMConfigMode[] zones_ = new UIMConfigMode[AURADef.kSpeedModeCount];

		public uint ConfigurationPresentMarker
		{ get; private set; }

		public int Version
		{ get; private set; }

		public UIMConfigMode this[SpeedMode mode]
		{
			get
			{
				return zones_[(int)mode];
			}
		}

		public int AlarmToneRepeatTime
		{ get; set; }

		public byte AlarmFlags
		{ get; set; }

		#region AlarmFlags Breakout Fields

		public int AF_ExtAudioSetting
		{
			// Bits 0:1
			get { return AlarmFlags & 0x03; }
			set { AlarmFlags = (byte)((AlarmFlags & 0xfc) | (value & 0x03)); }
		}

		public int AF_AudioLevel
		{
			// Bits 2:3
			get { return (AlarmFlags >> 2) & 0x03; }
			set { AlarmFlags = (byte)((AlarmFlags & 0xf3) | ((value & 0x03) << 2)); }
		}

		public int AF_EnableWorkMode
		{
			// Bit 4
			get { return (AlarmFlags >> 4) & 0x03; }////) != 0; }
			set { AlarmFlags = (byte)((AlarmFlags & 0xcf) | ((value & 0x03) << 4)); }// AlarmFlags = (byte)((AlarmFlags & ~(1 << 4)) | (value ? (1 << 4) : 0)); }
		}

		public bool AF_DisplayWorkMode
		{
			// Bit 5
			get { return (AlarmFlags & (1 << 5)) != 0; }
			set { AlarmFlags = (byte)((AlarmFlags & ~(1 << 5)) | (value ? (1 << 5) : 0)); }
		}

		public bool AF_EnableCrawlMode
		{
			// Bit 6
			get { return (AlarmFlags & (1 << 6)) != 0; }
			set { AlarmFlags = (byte)((AlarmFlags & ~(1 << 6)) | (value ? (1 << 6) : 0)); }
		}

		public bool AF_EnableCrawlModeAlarms
		{
			// Bit 7
			get { return (AlarmFlags & (1 << 7)) != 0; }
			set { AlarmFlags = (byte)((AlarmFlags & ~(1 << 7)) | (value ? (1 << 7) : 0)); }
		}

		#endregion

		public byte MiscFlags
		{ get; set; }

		#region MiscFlags Breakout Fields

		public bool MF_GPSShowMinutes
		{
			// Bit 0
			get { return (MiscFlags & (1 << 0)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 0)) | (value ? (1 << 0) : 0)); }
		}

		public bool MF_GPSShowSeconds
		{
			// Bit 1
			get { return (MiscFlags & (1 << 1)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 1)) | (value ? (1 << 1) : 0)); }
		}

		public bool MF_DisplayInMetric  // not to be trusted
		{
			// Bit 2
			get { return (MiscFlags & (1 << 2)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 2)) | (value ? (1 << 2) : 0)); }
		}

		public bool MF_AlertOnlyInCrawlMode
		{
			// Bit 3
			get { return (MiscFlags & (1 << 3)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 3)) | (value ? (1 << 3) : 0)); }
		}

		public bool MF_NoGpsDefaultToWorkMode
		{
			// Bit 4
			get { return (MiscFlags & (1 << 4)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 4)) | (value ? (1 << 4) : 0)); }
		}

		public bool MF_EnableSelectableProfiles
		{
			// Bit 5, present in config v. 1.1+ (UIM v. 2.17.0+).
			get { return (MiscFlags & (1 << 5)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 5)) | (value ? (1 << 5) : 0)); }
		}

		public bool MF_SilenceWorkMode
		{
			// Bit 6, present in config v. 1.1+ (UIM v. 2.17.0+).
			get { return (MiscFlags & (1 << 6)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 6)) | (value ? (1 << 6) : 0)); }
		}

		public bool MF_SilenceCrawlMode
		{
			// Bit 7, present in config v. 1.1+ (UIM v. 2.17.0+).
			get { return (MiscFlags & (1 << 7)) != 0; }
			set { MiscFlags = (byte)((MiscFlags & ~(1 << 7)) | (value ? (1 << 7) : 0)); }
		}

		#endregion

		public bool AutoBrakeEnable
		{ get; set; }

		public int AutoBrakeDelay
		{ get; set; }

		public UIMConfig()
		{
			for (int i = 0; i < AURADef.kSpeedModeCount; ++i)
				zones_[i] = new UIMConfigMode();
		}

		public UIMConfig(byte[] buf, ushort ofs, ushort len)
		{
			if (buf == null)
				throw new ArgumentNullException("buf");

			for (int i = 0; i < AURADef.kSpeedModeCount; ++i)
				zones_[i] = new UIMConfigMode();

			ushort ndx = ofs;

			ConfigurationPresentMarker = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			Version = buf[ndx++];

			this[SpeedMode.Work].EntrySpeed = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Work].Hysteresis = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Travel][DistanceZone.Approach] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Travel][DistanceZone.Inside] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Travel][DistanceZone.Critical] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Work][DistanceZone.Approach] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Work][DistanceZone.Inside] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Work][DistanceZone.Critical] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AlarmToneRepeatTime = buf[ndx++];
			AlarmFlags = buf[ndx++];
			MiscFlags = buf[ndx++];

			this[SpeedMode.Crawl].EntrySpeed = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Crawl].Hysteresis = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Crawl][DistanceZone.Approach] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Crawl][DistanceZone.Inside] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			this[SpeedMode.Crawl][DistanceZone.Critical] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AutoBrakeEnable = (buf[ndx] == 0) ? false : true;
			ndx++;

			AutoBrakeDelay = buf[ndx++];
		}

		public void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			BitConverter.GetBytes(ConfigurationPresentMarker).CopyTo(buf, ndx);
			ndx += 4;

			buf[ndx++] = (byte)Version;

			BitConverter.GetBytes((float)this[SpeedMode.Work].EntrySpeed).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Work].Hysteresis).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Travel][DistanceZone.Approach]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Travel][DistanceZone.Inside]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Travel][DistanceZone.Critical]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Work][DistanceZone.Approach]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Work][DistanceZone.Inside]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Work][DistanceZone.Critical]).CopyTo(buf, ndx);
			ndx += 4;

			buf[ndx++] = (byte)AlarmToneRepeatTime;
			buf[ndx++] = AlarmFlags;
			buf[ndx++] = MiscFlags;

			BitConverter.GetBytes((float)this[SpeedMode.Crawl].EntrySpeed).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Crawl].Hysteresis).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Crawl][DistanceZone.Approach]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Crawl][DistanceZone.Inside]).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((float)this[SpeedMode.Crawl][DistanceZone.Critical]).CopyTo(buf, ndx);
			ndx += 4;

			buf[ndx++] = (byte)(AutoBrakeEnable ? 1 : 0);  // UIM doesn't like val > 1 (unsigned)
			buf[ndx++] = (byte)AutoBrakeDelay;

			if ((ndx - ofs) != kUIMConfigSize)
				throw new ApplicationException(string.Format("UIMConfig.PackBuffer(): packed len {0} does not match expected {1}", ndx - ofs, kUIMConfigSize));
		}
	}


}

