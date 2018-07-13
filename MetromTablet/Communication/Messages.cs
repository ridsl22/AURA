using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using Metrom.Base;
using IDI.Math;
using Metrom.GPS;
using Metrom.AURA.Base;
using MetromTablet.Models;
using Metrom.AURA.Configuration;
using Metrom.AURA.Configuration.Mapper;



namespace MetromTablet.Communication
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	public enum AURAMsgOpcode : byte
	{
		InvalidOpcode = 0xff,  // DO NOT USE for a message!

		//------------------------------------------------------------
		// Radio Messages (OTA)
		//------------------------------------------------------------

		BcastBeacon = 0x10,
		SimBcastBeacon = 0x11,
		PinGPS = 0x12,
		GetLogStatus = 0x20,
		LogStatus = 0x21,
		GetLogEntry = 0x22,
		LogEntry = 0x23,
		QueryPeer = 0xe0,
		PerfData = 0xe1,


		//------------------------------------------------------------
		// UI Messages
		//------------------------------------------------------------

		UIGetJSONData = 0x30,		//PM 48
		UIGetEncoderConfig = 0x31,	//Tablet 49
		UIGetDiagnostics = 0x32,	//PM 
		UIGetConfigLog = 0x33,		//PM
		UIGetLogAcknowledge = 0x34,	//PM
		UIPMVersion = 0x35,
		UIOTAinit = 0x37,//PM		
		UIOTAdata = 0x38,//PM
		UIOTAack = 0x39, //Tablet
        UIOTAcomplete = 0x40, //PM

		UISetMachineName = 0x50,	//Tablet
		UIConfigInput = 0x51,		//Tablet
		UIConfigEncoder = 0x52,		//Tablet
		UIPMFWUpdate = 0x53,		//Tablet
		UILogInfo = 0x54,			//Tablet
		UISendLogZip = 0x55,		//Tablet
		UIRequestPMVersion = 0x56,
		UITabletConnected = 0x57,

		UICEMReady = 0x60,
		UICEMReadyAck = 0x61,
		UICEMConfigInfo = 0x62,
		UIGetCEMConfig = 0x63,
		UISetCEMConfig = 0x64,
		UISubSystemStatus = 0x65,
		UIGPSUpdate = 0x66,			//102
		UIDataUpdate = 0x67,		//103
		UICalibOp = 0x68,
		UICalibOpResult = 0x69,
		UICEMInfo = 0x6a,
		UIExternalAcknowledge = 0x6b,
		UISetTimeDate = 0x6c,		//108
		UICEMUIReady = 0x6d,
		UICEMUIReadyAck = 0x6e,

		UIExtAudio = 0x72,
		UISetExtAudio = 0x76,

		UICEMReflashInit = 0x80,
		UICEMReflashData = 0x81,
		UICEMReflashFini = 0x82,
		UISecurity = 0x83,

		UIGPSUpdateMirror = 0x84,  // DEPRECATED
		UIDataUpdateMirror = 0x85,  // DEPRECATED
		UISepDataDiag = 0x86,

		UIGetUIMConfig = 0x87,		//135
		UIUIMConfig = 0x88,
		UISetUIMConfig = 0x89,
		UIMHello = 0x8a,
		UIGetUIMDiagConfig = 0x8b,
		UIUIMDiagConfig = 0x8c,
		UISetUIMDiagConfig = 0x8d,

		UIGetProfileSummary = 0x90,
		UIProfileSummary = 0x91,
		UISetProfileSummary = 0x92,
		UIGetProfile = 0x93,
		UIProfile = 0x94,
		UISetProfile = 0x95,
		UISetProfileResult = 0x96,

		//------------------------------------------------------------
		// PC->UIM Messages
		//------------------------------------------------------------
		// testing purposes
		UIResetUIM = 0xb8, // PC
		UISetAlarmLimits = 0xb9, // PC

		//------------------------------------------------------------
		// UIM Management Messages (PC serial port)
		//------------------------------------------------------------

		UIMgmtText = 0xd0,			//208
		UIGetLogStatus = 0xd1,
		UILogStatus = 0xd2,
		UIGetLogEntry = 0xd3,
		UILogEntry = 0xd4,
		UICommandAck = 0xd5,
		UIRCMFWUpdate = 0xd6,
		UIRCMFWUpdateData = 0xd7,
		UIGetCEMInternals = 0xd8,
		UISetCEMInternals = 0xd9,
		UICEMInternals = 0xda,
		UIGetCEMDiagConfig = 0xdb,
		UISetCEMDiagConfig = 0xdc,
		UICEMDiagConfig = 0xdd,
		UIDiagAccelData = 0xde,
		UIGetCEMAccelCalibData = 0xdf,
		UICEMAccelCalibData = 0xe0,
		UICEMGetPeers = 0xe1,
		UICEMNewPeer = 0xe2,
		UICEMPeerFlushed = 0xe3,
		UICEMPeerUpdate = 0xe4,
		UICEMRangeData = 0xe5,
		UIADUConnect = 0xe6,
		UICEMSubSystemStatus = 0xe7,//231
		CEMHello = 0xe8,			//232
		UICEMConfigInfoADU = 0xe9,	//233
		UIGetCEMConfigADU = 0xea,
		UISetCEMConfigADU = 0xeb,
		UICEMFaultADU = 0xec,
		UICEMClearLog = 0xed,
		UIOAState = 0xee,			//238
		UIOAData = 0xef,

		UICEMTestWriteLog = 0xc0,  // debug, temporary
		UICEMResetRTC = 0xc1,  // debug, temporary
		UIGPSRTCData = 0xc2,  // debug, temporary

		UICEMQueryPeer = 0xc3,
		UICEMPerfData = 0xc4,
		UICEMExercise = 0xc5,
		UIPCFeedback = 0xc6,
		UIPCControl = 0xc7,
		UIData = 0xc8,

		//------------------------------------------------------------
		// CEM Test Fixture (CTF), CEM Under Test (CUT)
		//------------------------------------------------------------

		CtfStartTest = 0xf0,        // PC -> CTF diag port
		CtfEnterTestMode = 0xf1,    // CTF -> CUT
		CtfGetAnaDigInputs = 0xf2,  // CTF -> CUT
		CtfSendAnaDigInputs = 0xf3, // CUT -> CTF 
		CtfSendAccelData = 0xf4,    // CUT -> CTF
		CtfSetRelay = 0xf5,         // CTF -> CUT
		CtfAccelToggle = 0xf6,      // PC -> CTF -> CUT
		CtfReportGpsPulse = 0xf7,   // CTF -> CUT
		CtfReportingGpsPulse = 0xf8,// CUT -> CTF

		IoExpSetOutputs = 0xfa,     // PC -> CTF
		IoExpSetAnalog = 0xfb,      // PC -> CTF
		IoExpReadInputs = 0xfc      // PC -> CTF
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	public enum GPSStatus
	{
		InvalidValue = 0,
		NoComm,   // will never see
		Waiting,  // will never see
		NoFix,
		NonDifferentialFix,
		DifferentialFix,
		EstimatedFix
	};


	/// <summary>
	/// 
	/// </summary>
	/// 
	public enum RCMStatus
	{
		InvalidValue = 0,
		NoComm,   // will never see
		Waiting,  // will never see
		RangeLost,
		Ranging
	};


	public enum LongData
	{
		Binary = 0,
		Maint,
		CasEventLog,
		CasIncidentLog,
		ProductivityLog,
		ConfigurationLog
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	public class AURAAppMessage : AppMessage
	{
		public AURAMsgOpcode MsgOpcode
		{ get; private set; }

		public AURAAppMessage()
			: base((byte)AURAMsgOpcode.InvalidOpcode)
		{
			// Retrieve our opcode from the AURAMessage attribute decorating the leaf class.

			AURAMessageAttribute attr = (AURAMessageAttribute)Attribute.GetCustomAttribute(GetType(), typeof(AURAMessageAttribute));

			if (attr == null)
				throw new NotImplementedException("Message class must be decorated with an AURAMessage attribute");

			MsgOpcode = attr.Opcode;
			Opcode = (byte)MsgOpcode;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	// Radio Messages (OTA)
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	public class BcastBeaconData
	{
		public const ushort kBeaconPayloadSize = 25;

		public double Latitude
		{ get; set; }

		public double Longitude
		{ get; set; }

		public uint GPSFixTime
		{ get; set; }

		public double Velocity
		{ get; set; }

		public double FrontAntennaOffset
		{ get; set; }

		public double RearAntennaOffset
		{ get; set; }

		public double Orientation
		{ get; set; }

		public byte Status
		{ get; set; }

		public uint RawNameValue
		{ get; set; }

		public double Heading
		{ get; set; }

		public string Name
		{ get; set; }

		public string NamePrefix
		{ get; set; }

		public uint NameNumber
		{ get; set; }

		public BcastBeaconData()
		{
		}

		public BcastBeaconData(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Latitude = BitConverter.ToInt32(buf, ndx) * 1e-7;
			ndx += 4;

			Longitude = BitConverter.ToInt32(buf, ndx) * 1e-7;
			ndx += 4;

			GPSFixTime = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			Velocity = BitConverter.ToUInt16(buf, ndx) * 0.01;
			ndx += 2;

			FrontAntennaOffset = buf[ndx++] * 0.3;
			RearAntennaOffset = buf[ndx++] * 0.3;

			Orientation = buf[ndx++] * 2.0;

			Status = buf[ndx++];

			RawNameValue = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			Heading = buf[ndx++] * 2.0;

			byte[] name2 = new byte[2];

			if (len > 23)
			{
				name2[0] = buf[ndx++];
				name2[1] = buf[ndx++];
			}
			else
			{
				name2[0] = 0x20;
				name2[1] = 0x20;
			}

			if (AURADataUtil.PackedNameHas4CharIndicator(RawNameValue))
				Name = AURADataUtil.UnpackName(RawNameValue, name2[0], name2[1]);
			else
				Name = AURADataUtil.UnpackName(RawNameValue);
		}

		internal ushort Pack(byte[] buf, ushort ofs)
		{
			if ((NamePrefix == null) || (NamePrefix.Length < 2) || (NamePrefix.Length > 4))
				throw new InvalidOperationException("NamePrefix field must be a string between 2 and 4 characters in length");

			ushort ndx = ofs;

			BitConverter.GetBytes((int)(Latitude * 1e7)).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((int)(Longitude * 1e7)).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes(GPSFixTime).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((ushort)(Velocity * 100)).CopyTo(buf, ndx);
			ndx += 2;

			buf[ndx++] = (byte)Math.Round(FrontAntennaOffset / 0.3);
			buf[ndx++] = (byte)Math.Round(RearAntennaOffset / 0.3);

			buf[ndx++] = (byte)Math.Round(Orientation * 0.5);

			buf[ndx++] = Status;

			byte[] prefix = ASCIIEncoding.ASCII.GetBytes(NamePrefix);

			uint packedName = AURADataUtil.PackName(prefix, NameNumber);

			BitConverter.GetBytes(packedName).CopyTo(buf, ndx);
			ndx += 4;

			buf[ndx++] = (byte)Math.Round(Heading * 0.5);

			// Note: 26 (dec) is coding for blank / unused character.
			buf[ndx++] = (prefix.Length > 2) ? AURADataUtil.EncodeNameChar(prefix[2]) : (byte)26;
			buf[ndx++] = (prefix.Length > 3) ? AURADataUtil.EncodeNameChar(prefix[3]) : (byte)26;

			return (ushort)(ndx - ofs);
		}
	}



	[AURAMessage(AURAMsgOpcode.BcastBeacon)]
	public class Msg_BcastBeacon : AURAAppMessage
	{
		public BcastBeaconData Data
		{ get; private set; }

		public Msg_BcastBeacon(byte[] buf, ushort ofs, ushort len)
		{
			Data = new BcastBeaconData(buf, ofs, len);
		}

		public Msg_BcastBeacon(BcastBeaconData data)
		{
			Data = data;
		}

		public override ushort GetLength()
		{
			return 25;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			Data.Pack(buf, ofs);
		}
	}



	[AURAMessage(AURAMsgOpcode.SimBcastBeacon)]
	public class Msg_SimBcastBeacon : AURAAppMessage
	{
		public BcastBeaconData Data
		{ get; private set; }

		public uint Address
		{ get; private set; }

		public double RCMDist
		{ get; private set; }

		public Msg_SimBcastBeacon(BcastBeaconData data, uint addr, double rcmDist)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			Data = data;
			Address = addr;
			RCMDist = rcmDist;
		}

		public override ushort GetLength()
		{
			return BcastBeaconData.kBeaconPayloadSize + 7;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			ndx += Data.Pack(buf, ndx);

			AURADataUtil.PackDNTAddress(buf, ndx, Address);
			ndx += 3;

			BitConverter.GetBytes((float)RCMDist).CopyTo(buf, ndx);
			ndx += 4;
		}
	}


	[AURAMessage(AURAMsgOpcode.PinGPS)]
	public class Msg_PinGPS : AURAAppMessage
	{
		public double Latitude
		{ get; private set; }

		public double Longitude
		{ get; private set; }

		public Msg_PinGPS(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public override ushort GetLength()
		{
			return 8;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			BitConverter.GetBytes((int)(Latitude * 1e7)).CopyTo(buf, ndx);
			ndx += 4;

			BitConverter.GetBytes((int)(Longitude * 1e7)).CopyTo(buf, ndx);
			ndx += 4;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIDiagAccelData)]
	public class Msg_UIDiagAccelData : AURAAppMessage
	{
		/// <summary>
		/// Gets or sets (private) the raw acceleration vector read from the CEM accelerometer.
		/// (rawVec in CAS_Accel.c)
		/// </summary>
		///
		public Vector RawAccel
		{ get; private set; }

		/// <summary>
		/// Gets or sets (private) the unfiltered, gravitationally-corrected accel vector.
		/// (NEWcorrVec in CAS_Accel.c)
		/// </summary>
		///
		public Vector Accel
		{ get; private set; }

		/// <summary>
		/// Gets or sets (private) the unfiltered, F/R aligned corrected accel scalar.
		/// (rawCorrScalar in CAS_Accel.c)
		/// </summary>
		///
		public double AlignedScalar
		{ get; private set; }

		public double GPSVel
		{ get; private set; }

		public ushort GPSHeading
		{ get; private set; }

		public double Latitude
		{ get; private set; }

		public double Longitude
		{ get; private set; }

		/// <summary>
		/// Gets or sets (private) the magnitude of the projection of the unfiltered corrected
		/// accel vector onto the horizontal vehicle plane defined by the calibrated gravitational
		/// correction unit vector. Only present in CEM v. 2.2.7 and later; zero otherwise.
		/// (planarAccelMag in CAS_Accel.c)
		/// </summary>
		public double PlanarScalar
		{ get; private set; }

		/// <summary>
		/// Gets or sets (private) the projection of the unfiltered corrected accel vector onto the
		/// horizontal vehicle plane defined by the calibrated gravitational correction unit vector.
		/// Only present in CEM v. 2.2.7 and later; zero otherwise.
		/// (planarAccel in CAS_Accel.c)
		/// </summary>
		public Vector PlanarVec
		{ get; private set; }

		public Msg_UIDiagAccelData(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			RawAccel = new Vector(3);

			RawAccel[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			RawAccel[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			RawAccel[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			Accel = new Vector(3);

			Accel[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			Accel[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			Accel[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			AlignedScalar = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			GPSVel = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			GPSHeading = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			Latitude = BitConverter.ToInt32(buf, ndx) / 1e7;
			ndx += 4;

			Longitude = BitConverter.ToInt32(buf, ndx) / 1e7;
			ndx += 4;

			if (len > 42)
			{
				PlanarScalar = BitConverter.ToSingle(buf, ndx);
				ndx += 4;

				PlanarVec = new Vector(3);

				PlanarVec[0] = BitConverter.ToSingle(buf, ndx);
				ndx += 4;

				PlanarVec[1] = BitConverter.ToSingle(buf, ndx);
				ndx += 4;

				PlanarVec[2] = BitConverter.ToSingle(buf, ndx);
				ndx += 4;
			}
			else
			{
				PlanarScalar = 0;
				PlanarVec = new Vector(3);
			}
		}
	}



	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIMHello)]
	public class Msg_UIMHello : AURAAppMessage
	{
		public ThreePartVersion UIMVersion
		{ get; private set; }

		public uint Flags
		{ get; private set; }

		public Msg_UIMHello()
		{
		}

		public Msg_UIMHello(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			UIMVersion = new ThreePartVersion(buf[ndx + 2], buf[ndx + 1], buf[ndx]);
			ndx += 3;

			Flags = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMReady)]
	public class Msg_UICEMReady : AURAAppMessage
	{
		public byte CEMVersionMajor
		{ get; private set; }

		public byte CEMVersionMinor
		{ get; private set; }

		public byte CEMVersionRevision
		{ get; private set; }

		public Msg_UICEMReady(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			CEMVersionMajor = buf[ndx++];
			CEMVersionMinor = buf[ndx++];
			CEMVersionRevision = buf[ndx++];
		}
	}
	

	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMReadyAck)]
	public class Msg_UICEMReadyAck : AURAAppMessage
	{

	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMUIReady)]
	public class Msg_UICEMUIReady : AURAAppMessage
	{

	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIMgmtText)]
	public class Msg_UIMgmtText : AURAAppMessage
	{
		private string text_;

		public string Text
		{ get { return text_; } }

		public Msg_UIMgmtText(byte[] buf, ushort ofs, ushort len)
		{
			text_ = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGPSUpdate)]
	public class Msg_UIGPSUpdate : AURAAppMessage
	{
		public uint GPSTimeRef
		{ get; private set; }

		public double LocalLat
		{ get; private set; }

		public double LocalLng
		{ get; private set; }

		public float LocalVel
		{ get; private set; }

		public ushort LocalHeading
		{ get; private set; }

		public byte Status
		{ get; private set; }

		public Msg_UIGPSUpdate() { }

		public Msg_UIGPSUpdate(Msg_UIGPSUpdate msg) 
		{
			GPSTimeRef = msg.GPSTimeRef;
			LocalLat = msg.LocalLat;
			LocalLng = msg.LocalLng;
			LocalVel = msg.LocalVel;
			LocalHeading = msg.LocalHeading;
			Status = msg.Status;
		}

		public Msg_UIGPSUpdate(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			GPSTimeRef = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			LocalLat = BitConverter.ToDouble(buf, ndx);
			ndx += 8;

			LocalLng = BitConverter.ToDouble(buf, ndx);
			ndx += 8;

			LocalVel = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			LocalHeading = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			Status = buf[ndx++];
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMConfigInfoADU)]
	public class Msg_UICEMConfigInfoADU : AURAAppMessage
	{
		public CEMConfig Config
		{ get; private set; }

		public Msg_UICEMConfigInfoADU(byte[] buf, ushort ofs, ushort len)
		{
			Config = new CEMConfig(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetCEMInternals)]
	public class Msg_UIGetCEMInternals : AURAAppMessage
	{
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMConfigInfo)]
	public class Msg_UICEMConfigInfo : AURAAppMessage
	{
		public CEMConfig Config
		{ get; private set; }

		public Msg_UICEMConfigInfo(byte[] buf, ushort ofs, ushort len)
		{
			Config = new CEMConfig(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UISubSystemStatus)]
	public class Msg_UISubSystemStatus : AURAAppMessage
	{
		public byte SubSystem
		{ get; private set; }

		public byte Status
		{ get; private set; }

		public Msg_UISubSystemStatus(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			SubSystem = buf[ndx++];
			Status = buf[ndx++];
		}
	}



	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	public class DataUpdateMsgPeerData
	{
		public byte Flags
		{ get; private set; }

		public uint Address
		{ get; private set; }

		public string NamePrefix
		{ get; private set; }

		public uint NameNumber
		{ get; private set; }

		public string Name
		{ get; private set; }

		public SeparationSource SepSource
		{ get; private set; }

		public byte RawSepSource
		{ get; private set; }

		public double Separation
		{ get; private set; }

		public double Velocity
		{ get; private set; }

		public double DeltaSep
		{ get; private set; }

		public double RawRCMSep
		{ get; private set; }

		public double RawGPSSep
		{ get; private set; }

		public ushort Heading
		{ get; private set; }

		public GPSCoord GPSLoc
		{ get; private set; }

		public int RawLatData
		{ get; private set; }

		public int RawLngData
		{ get; private set; }

		public byte RCMRangeStatus
		{ get; private set; }

		public ushort RCMVPeak
		{ get; private set; }

		public uint RCMChannelRise
		{ get; private set; }

		public int RCMRangeErrEstimate
		{ get; private set; }

		public ushort RCMRequesterLEDFlags
		{ get; private set; }

		public ushort RCMResponderLEDFlags
		{ get; private set; }

		public void Unpack(byte[] buf, ref ushort ndx)
		{
			Flags = buf[ndx++];

			Address = AURADataUtil.UnpackDNTAddress(buf, ndx);
			ndx += 3;

			int prefixLen = 2;

			if ((buf[ndx + prefixLen] != 0) && (buf[ndx + prefixLen] != 0x20))
				++prefixLen;
			if ((buf[ndx + prefixLen] != 0) && (buf[ndx + prefixLen] != 0x20))
				++prefixLen;

			NamePrefix = ASCIIEncoding.ASCII.GetString(buf, ndx, prefixLen);
			ndx += 4;

			NameNumber = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			Name = NamePrefix + NameNumber.ToString("d6");

			RawSepSource = buf[ndx++];

			switch (RawSepSource)
			{
				case 1:
					SepSource = SeparationSource.GPS;
					break;
				case 2:
					SepSource = SeparationSource.RCM;
					break;
				default:
					SepSource = SeparationSource.Unknown;
					break;
			}

			Separation = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			Velocity = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			DeltaSep = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			RawRCMSep = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			RawGPSSep = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			Heading = (ushort)(buf[ndx++] * 2);

			RawLatData = BitConverter.ToInt32(buf, ndx);
			ndx += 4;

			RawLngData = BitConverter.ToInt32(buf, ndx);
			ndx += 4;

			GPSLoc = new GPSCoord(RawLatData * 1e-7, RawLngData * 1e-7);

			RCMRangeStatus = buf[ndx++];

			RCMVPeak = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			RCMChannelRise = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			RCMRangeErrEstimate = BitConverter.ToInt32(buf, ndx);
			ndx += 4;

			RCMRequesterLEDFlags = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			RCMResponderLEDFlags = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;
		}
	}


	[AURAMessage(AURAMsgOpcode.UIDataUpdate)]
	public class Msg_UIDataUpdate : AURAAppMessage
	{
		public byte UpdateFlags
		{ get; private set; }

		public DataUpdateMsgPeerData[] PeerData
		{ get; private set; }

		public Msg_UIDataUpdate(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			UpdateFlags = buf[ndx++];

			PeerData = new DataUpdateMsgPeerData[2] { new DataUpdateMsgPeerData(), new DataUpdateMsgPeerData() };

			PeerData[0].Unpack(buf, ref ndx);
			PeerData[1].Unpack(buf, ref ndx);
		}
	}


	[AURAMessage(AURAMsgOpcode.UIData)]
	public class Msg_UIData : AURAAppMessage
	{
		public UI_Enums.UIA_Primitive UiaPrimitive //1 byte
		{ get; private set; }

		public string Data //40 bytes
		{ get; private set; }

		public byte NotificationType //Alert, Alarm, etc... 1 byte
		{ get; private set; }

		public byte WhichPeer //1 byte
		{get; private set; }

		public int LineNumber //4 bytes
		{ get; private set;	}

		public byte SsdDisplaySpecialCodes //1 byte
		{ get; private set; }

		public int Value 
		{get; private set; } //2 bytes

		public byte Data1
		{get; private set; } //1 byte

		public byte Data2
		{get; private set; } //1 byte

		public byte Data3
		{get; private set; } //1 byte


		public Msg_UIData(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			UiaPrimitive = (UI_Enums.UIA_Primitive)buf[ndx++];
			
			Data =  ASCIIEncoding.ASCII.GetString(buf, ndx, 40);
			ndx += 40;

			NotificationType = buf[ndx++];

			WhichPeer = buf[ndx++];

			LineNumber = BitConverter.ToInt32(buf, ndx);
			ndx += 4;
			
			SsdDisplaySpecialCodes = buf[ndx++];

			Value = BitConverter.ToInt16(buf, ndx);
			ndx += 2;

			Data1 = buf[ndx++];
			Data2 = buf[ndx++];
			Data3 = buf[ndx++];
		}


		public Msg_UIData(UI_Enums.UIA_Primitive _uiaPrimitive, string _data, byte _notificationType, byte _whichPeer, int _lineNumber, 
			byte _ssdDisplaySpecialCodes, int _value,  byte _data1, byte _data2, byte _data3)
		{
			UiaPrimitive = _uiaPrimitive;
			Data = _data;
			NotificationType = _notificationType;
			WhichPeer = _whichPeer;
			LineNumber =_lineNumber;
			SsdDisplaySpecialCodes = _ssdDisplaySpecialCodes;
			Value = _value;
			Data1 = _data1;
			Data2 = _data2;
			Data3 = _data3;
		}

		public Msg_UIData(UI_Enums.UIA_Primitive _uiaPrimitive)
		{
			UiaPrimitive = _uiaPrimitive;
		}


		public override ushort GetLength()
		{
			return 58;
		}


		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			buf[ndx] = Convert.ToByte(UiaPrimitive);		
			ndx += 1;

			//ASCIIEncoding.ASCII.GetBytes(Data).CopyTo(buf, ndx);
			//ndx += 40;

			//buf[ndx] = NotificationType;
			//ndx += 1;

			//buf[ndx] = WhichPeer;
			//ndx += 1;

			//buf[ndx] = (byte)LineNumber;//to complete
			//ndx += 4;

			//buf[ndx] = SsdDisplaySpecialCodes;
			//ndx += 1;

			//buf[ndx] = (byte)Value;//to complete
			//ndx += 2;
			for (ndx = 6; ndx < ofs + GetLength(); ndx++)
				buf[ndx] = 0;

			//buf[ndx] = 0;
			//ndx += 1;

			//buf[ndx] = 0;// Data2;
			//ndx += 1;

			//buf[ndx] = 0;// Data3;
			//ndx += 1;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMInfo)]
	public class Msg_UICEMInfo : AURAAppMessage
	{
		public uint CEMAddress
		{ get; private set; }

		public byte RCMVersionMajor
		{ get; private set; }

		public byte RCMVersionMinor
		{ get; private set; }

		public byte RCMVersionRevision
		{ get; private set; }

		public Msg_UICEMInfo(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			CEMAddress = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			RCMVersionMajor = buf[ndx++];
			RCMVersionMinor = buf[ndx++];
			RCMVersionRevision = buf[ndx++];
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICalibOpResult)]
	public class Msg_UICalibOpResult : AURAAppMessage
	{
		public byte OpResult
		{ get; private set; }

		public byte OpError
		{ get; private set; }

		public Msg_UICalibOpResult(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			OpResult = buf[ndx++];
			OpError = buf[ndx++];
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UISetTimeDate)]
	public class Msg_UISetTimeDate : AURAAppMessage
	{
		public ushort Year
		{ get; private set; }

		public byte Month
		{ get; private set; }

		public byte Day
		{ get; private set; }

		public byte Hour
		{ get; private set; }

		public byte Minute
		{ get; private set; }

		public byte Second
		{ get; private set; }

		public DateTime UTCDateTime
		{ get; private set; }

		public Msg_UISetTimeDate(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Year = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			Month = buf[ndx++];
			Day = buf[ndx++];
			Hour = buf[ndx++];
			Minute = buf[ndx++];
			Second = buf[ndx++];

			UTCDateTime = new DateTime(Year, Month, Day, Hour, Minute, Second, DateTimeKind.Utc);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Originator can be PC *or* CEM (no payload if PC)</remarks>
	/// 
	[AURAMessage(AURAMsgOpcode.CEMHello)]
	public class Msg_CEMHello : AURAAppMessage
	{
		public ThreePartVersion CEMVersion
		{ get; private set; }

		public ThreePartVersion RCMVersion
		{ get; private set; }

		public TwoPartVersion ConfigVersion
		{ get; private set; }

		public string MachineName
		{ get; private set; }

		public uint Address
		{ get; private set; }

		public byte NetworkId
		{ get; private set; }

		public uint Flags
		{ get; private set; }

		public Msg_CEMHello()
		{
		}

		public Msg_CEMHello(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			CEMVersion = new ThreePartVersion(buf[ndx + 2], buf[ndx + 1], buf[ndx]);
			ndx += 3;

			RCMVersion = new ThreePartVersion(buf[ndx + 2], buf[ndx + 1], buf[ndx]);
			ndx += 3;

			ConfigVersion = new TwoPartVersion(buf[ndx + 1], buf[ndx]);
			ndx += 2;

			int nameLen = 0;

			while ((nameLen < 16) && (buf[ndx + nameLen] != 0))
				++nameLen;

			MachineName = (nameLen == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ndx, nameLen);
			ndx += 16;

			Address = AURADataUtil.UnpackDNTAddress(buf, ndx);
			ndx += 3;

			NetworkId = buf[ndx++];

			Flags = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIUIMConfig)]
	public class Msg_UIUIMConfig : AURAAppMessage
	{
		public UIMConfig1 Config
		{ get; private set; }

		public Msg_UIUIMConfig(byte[] buf, ushort ofs, ushort len)
		{
			Config = new UIMConfig1(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIOAState)]
	public class Msg_UIOAState : AURAAppMessage
	{
		public byte State
		{ get; private set; }

		public byte SubState
		{ get; private set; }

		public Msg_UIOAState(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			State = buf[ndx++];
			SubState = buf[ndx++];
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIADUConnect)]
	public class Msg_UIADUConnect : AURAAppMessage
	{
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UISetMachineName)]
	public class Msg_UISetMachineName : AURAAppMessage
	{
		public MachineConfig MType{ get; set; }
		public int NodesCount { get; set; }

		public Msg_UISetMachineName(string machineName, int nodesCount)
		{
			MType = new MachineConfig();
			MType.MachineName = machineName;
			NodesCount = nodesCount;
		}


		public override ushort GetLength()
		{
			return 33;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			ASCIIEncoding.ASCII.GetBytes(MType.MachineName).CopyTo(buf, ndx);
			ndx += (ushort)MType.MachineName.Length;

			for (int i = MType.MachineName.Length; i < 32; i++)
			{
				buf[ndx++] = 0;
			}
			//BitConverter.GetBytes(MType.NodeNumber).CopyTo(buf, ndx);
			buf[ndx] = (byte)NodesCount;
			ndx += 1;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



	[AURAMessage(AURAMsgOpcode.UIConfigInput)]
	public class Msg_UIConfigInput : AURAAppMessage
	{
		public Node CurrentNode
		{ get; set; }

		public NodePin CurrentPin
		{ get; set; }

		public int InputCnt { get; set; }

		public Msg_UIConfigInput(Node node, NodePin pin, int _inputCnt)
		{
			CurrentNode = new Node();
			CurrentPin = new NodePin();
			CurrentNode.Id = node.Id;
			CurrentPin.Id = pin.Id;
			CurrentPin.LogicLevel = pin.LogicLevel;
			CurrentPin.IdleTime = pin.IdleTime;
			CurrentPin.edge = pin.edge;
			CurrentPin.PinType = pin.PinType;
			CurrentPin.Gain = pin.Gain;
			CurrentPin.Offset = pin.Offset;
			CurrentPin.CycleType = pin.CycleType;
			InputCnt = _inputCnt;
			
		}


		public override ushort GetLength()
		{
			return 13;
		}


		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			//ASCIIEncoding.ASCII.GetBytes(CurrentNode.Name).CopyTo(buf, ndx);
			buf[ndx] = Convert.ToByte(CurrentNode.Id);		
			ndx += 1;

			buf[ndx] = Convert.ToByte(CurrentPin.Id);	
			ndx += 1;

			buf[ndx] = (byte)InputCnt;
			ndx += 1;

			buf[ndx] = (byte)CurrentPin.PinType;
			ndx += 1;

			BitConverter.GetBytes(CurrentPin.LogicLevel).CopyTo(buf, ndx);
			ndx += 4;

			buf[ndx] = (byte)CurrentPin.IdleTime;
			ndx += 1;

			buf[ndx] = (byte)CurrentPin.edge;
			ndx += 1;

			buf[ndx] = (byte)CurrentPin.Gain;
			ndx += 1;

			buf[ndx] = (byte)CurrentPin.Offset;
			ndx += 1;

			buf[ndx] = (byte)CurrentPin.CycleType;
			ndx += 1;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIConfigEncoder)]
	public class Msg_UIConfigEncoder : AURAAppMessage
	{
		public int EncResolution { get; set; }
		public float WheelDiameter { get; set; }
		public byte Direction { get; set; }
		public float ClarDistance { get; set; }

		public Msg_UIConfigEncoder(int _encResolution, float _wheelDiameter, byte _direction, float _clarDistance)
		{
			EncResolution = _encResolution;
			WheelDiameter = _wheelDiameter;
			Direction = _direction;
			ClarDistance = _clarDistance;
		}

		public override ushort GetLength()
		{
			return 11;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			byte[] intBytesER = BitConverter.GetBytes(EncResolution);
			buf[ndx] = intBytesER[0];
			buf[ndx + 1] = intBytesER[1];
			ndx += 2;

			BitConverter.GetBytes(WheelDiameter).CopyTo(buf, ndx);
			ndx += 4;

			buf[ndx] = Direction;
			ndx++;

			BitConverter.GetBytes(ClarDistance).CopyTo(buf, ndx);
			ndx += 4;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetEncoderConfig)]
	public class Msg_UIGetEncoderConfig : AURAAppMessage
	{
		private string _text;

		public string Text
		{
			get { return _text; }
		}

		public Msg_UIGetEncoderConfig(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;
			_text = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetJSONData)]
	public class Msg_UIGetJSONData : AURAAppMessage
	{
		private string _text;

		public string Text
		{ 
			get { return _text; } 
		}

		public Msg_UIGetJSONData(byte[] buf, ushort ofs, ushort len)
		{
			_text = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetDiagnostics)]
	public class Msg_UIGetDiagnostics : AURAAppMessage
	{
		private string _text;

		public string Text
		{
			get { return _text; }
		}

		public Msg_UIGetDiagnostics(byte[] buf, ushort ofs, ushort len)
		{
			_text = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetConfigLog)]
	public class Msg_UIGetConfigLog : AURAAppMessage
	{
		private string _text;

		public string Text
		{
			get { return _text; }
		}

		public Msg_UIGetConfigLog(byte[] buf, ushort ofs, ushort len)
		{
			_text = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ofs, len);
		}
	}

	//[AURAMessage(AURAMsgOpcode.UIPMFWVersion)]
	//public class Msg_UIPMFWVersion : AURAAppMessage
	//{
	//	//public string PMFWVersion{ get; set; }
	//	public int Length { get; set; }

	//	public Msg_UIPMFWVersion(int _length)
	//	{
	//		Length = _length;
	//	}

	//	public override ushort GetLength()
	//	{
	//		return 4;
	//	}

	//	public override void PackBuffer(byte[] buf, ushort ofs)
	//	{
	//		ushort ndx = ofs;

	//		byte[] intBytesLength = BitConverter.GetBytes(Length);
	//		buf[ndx] = intBytesLength[0];
	//		buf[ndx + 1] = intBytesLength[1];
	//		buf[ndx + 2] = intBytesLength[2];
	//		buf[ndx + 3] = intBytesLength[3];
	//		//ndx += 4;
	//	}
	//}


	[AURAMessage(AURAMsgOpcode.UIPMFWUpdate)]
	public class Msg_UIPMFWUpdate : AURAAppMessage
	{
		public byte[] Data
		{ get; private set; }

		public ushort Length
		{ get; private set; }

		public Msg_UIPMFWUpdate(uint ofs, ushort len, byte[] buf)
		{
			if (buf == null)
				throw new ArgumentNullException("buf");
			if ((len == 0) || (len > 240))
				throw new ArgumentException("len must be [1, 240]", "len");

			Data = new byte[len];
			Length = len;

			Array.Copy(buf, ofs, Data, 0, len);
		}

		public override ushort GetLength()
		{
			return Length;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			Array.Copy(Data, 0, buf, ndx, Length);
			ndx += Length;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UILogInfo)]
	public class Msg_UILogInfo : AURAAppMessage
	{
		public LongData LongDataType //1 byte
		{ get; private set; }
		
		//public short ChunkSize //2 byte
		public uint ChunkSize //2 byte
		{ get; private set; }
		
		public uint SizeOfAllData //2 bytes   ...t change to 4 bytes 
		{ get; private set; }
		
		//public short RemainingData //2 bytes
		public uint RemainingData //2 bytes
		{ get; private set; }

		public string ZipName //32 bytes
		{ get; private set; }

		public byte[] CRC // 4 bytes
		{ get; set; }


		public Msg_UILogInfo() { }


		public Msg_UILogInfo(LongData _longDataType, uint _chunkSize, uint _sizeOfAllData, uint _remainingData, string _zipName)
		{
			LongDataType = _longDataType;
			ChunkSize = _chunkSize;
			SizeOfAllData = _sizeOfAllData;
			RemainingData = _remainingData;
			ZipName = _zipName;
		}


		public Msg_UILogInfo(LongData _longDataType, uint _chunkSize, uint _sizeOfAllData, byte[] _crc)
		{
			LongDataType = _longDataType;
			ChunkSize = _chunkSize;
			SizeOfAllData = _sizeOfAllData;
			CRC = _crc;
		}

		//public Msg_UILogInfo(LongData _longDataType, short _chunkSize, uint _sizeOfAllData, string _zipName)
		//{
		//	LongDataType = _longDataType;
		//	ChunkSize = _chunkSize;
		//	SizeOfAllData = _sizeOfAllData;
		//	//RemainingData = _remainingData;
		//	ZipName = _zipName;
		//}


		//public Msg_UILogInfo(LongData _longDataType, short _chunkSize, uint _sizeOfAllData, byte[] _crc)
		//{
		//	LongDataType = _longDataType;
		//	ChunkSize = _chunkSize;
		//	SizeOfAllData = _sizeOfAllData;
		//	CRC = _crc;
		//}


		public override ushort GetLength()
		{
			return 39;
		}


		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			buf[ndx] = (byte)LongDataType;
			ndx += 1;

			byte[] leBytes = BitConverter.GetBytes(ChunkSize);
			buf[ndx] = leBytes[0];
			buf[ndx + 1] = leBytes[1];
			ndx += 2;

			if (LongDataType != LongData.Binary)
			{
				for (int i = 0; i < 4; i++)
					buf[ndx++] = 0;

				byte[] bytes = BitConverter.GetBytes(SizeOfAllData);
				// Reverse for proper endianness, and return
				//Array.Reverse(bytes);

				buf[ndx] = bytes[0];
				buf[ndx + 1] = bytes[1];
				buf[ndx + 2] = bytes[2];
				buf[ndx + 3] = bytes[3];
				ndx += 4;

				int index = (int)ndx;
				for (int i = index; i < index + 32; i++)
				{
					buf[i] = 0;
				}

				ASCIIEncoding.ASCII.GetBytes(ZipName).CopyTo(buf, ndx);
				ndx += 32;
			}
			else
			{
				buf[ndx] = CRC[0];
				buf[ndx + 1] = CRC[1];
				buf[ndx + 2] = CRC[2];
				buf[ndx + 3] = CRC[3];
				ndx += 4;

				byte[] bytes = BitConverter.GetBytes(SizeOfAllData);
				// Reverse for proper endianness, and return
				//Array.Reverse(bytes);

				buf[ndx] = bytes[0];
				buf[ndx + 1] = bytes[1];
				buf[ndx + 2] = bytes[2];
				buf[ndx + 3] = bytes[3];
				ndx += 4;
			}
		}


		//public override void PackBuffer(byte[] buf, ushort ofs)
		//{
		//	ushort ndx = ofs;

		//	buf[ndx] = (byte)LongDataType;
		//	ndx += 1;

		//	byte[] leBytes = BitConverter.GetBytes(ChunkSize);
		//	buf[ndx] = leBytes[0];
		//	buf[ndx + 1] = leBytes[1];
		//	ndx += 2;
		//	if (LongDataType != LongData.Binary)
		//	{

		//		leBytes = BitConverter.GetBytes(SizeOfAllData);
		//		buf[ndx] = leBytes[0];
		//		buf[ndx + 1] = leBytes[1];
		//		ndx += 2;

		//		leBytes = BitConverter.GetBytes(RemainingData);
		//		buf[ndx] = leBytes[0];
		//		buf[ndx + 1] = leBytes[1];
		//		ndx += 2;

		//		int index = (int)ndx;
		//		for (int i = index; i < index + 32; i++)
		//		{
		//			buf[i] = 0;
		//		}

		//		ASCIIEncoding.ASCII.GetBytes(ZipName).CopyTo(buf, ndx);
		//		ndx += 32;
		//	}
		//	else
		//	{
		//		for (int i = 0; i < 4; i++)
		//			buf[ndx++] = 0;

		//		byte[] bytes = BitConverter.GetBytes(SizeOfAllData);
		//		// Reverse for proper endianness, and return
		//		//Array.Reverse(bytes);

		//		buf[ndx] = bytes[3];
		//		buf[ndx + 1] = bytes[2];
		//		buf[ndx + 2] = bytes[1];
		//		buf[ndx + 3] = bytes[0];
		//		ndx += 4;

		//		buf[ndx] = CRC[0];
		//		buf[ndx + 1] = CRC[1];
		//		buf[ndx + 2] = CRC[2];
		//		buf[ndx + 3] = CRC[3];
		//		ndx += 4;
		//	}
		//}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UISendLogZip)]
	public class Msg_UISendLogZip : AURAAppMessage
	{
		public byte[] Data
		{ get; private set; }

		public ushort Length
		{ get; private set; }

		public Msg_UISendLogZip() { }


		public Msg_UISendLogZip(byte[] _data, ushort _length)
		{
			Data = _data;
			Length = _length;
		}


		public override ushort GetLength()
		{
			return Length;// + ProtocolConst.HeaderLen);
		}


		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			for (int i = 0; i < Length; i++)
			{
				buf[ndx++] = Data[i];
			}
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UISetExtAudio)]
	public class Msg_UISetExtAudio : AURAAppMessage
	{
		public ExtrnAudio ExtAudio {get; set;}

		public Msg_UISetExtAudio(ExtrnAudio _extAudio)
		{
			ExtAudio = _extAudio;
		}

		public override ushort GetLength()
		{
			return 1;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			buf[ndx] = (byte)ExtAudio;
		}
	}



	[AURAMessage(AURAMsgOpcode.UIExtAudio)]
	public class Msg_UIExtAudio : AURAAppMessage
	{
		public ExtrnAudio ExtAudio { get; set; }

		public Msg_UIExtAudio(ExtrnAudio _extAudio)
		{
			ExtAudio = _extAudio;
		}

		public override ushort GetLength()
		{
			return 1;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			buf[ndx] = (byte)ExtAudio;
		}
	}

	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

	
	[AURAMessage(AURAMsgOpcode.UIGetCEMConfig)]
	public class Msg_UIGetCEMConfig : AURAAppMessage
	{
		public CEMConfig Config
		{ get; private set; }

		public Msg_UIGetCEMConfig()
		{
		}

		public Msg_UIGetCEMConfig(byte[] buf, ushort ofs, ushort len)
		{
			Config = new CEMConfig(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetUIMConfig)]
	public class Msg_UIGetUIMConfig : AURAAppMessage
	{
		public UIMConfig Config
		{ get; private set; }

		public Msg_UIGetUIMConfig()
		{ 
		}

		public Msg_UIGetUIMConfig(byte[] buf, ushort ofs, ushort len)
		{
			Config = new UIMConfig(buf, ofs, len);
		}

	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetCEMConfigADU)]
	public class Msg_UIGetCEMConfigADU : AURAAppMessage
	{
		public CEMConfig Config
		{ get; private set; }

		public Msg_UIGetCEMConfigADU() { }

		public Msg_UIGetCEMConfigADU(byte[] buf, ushort ofs, ushort len)
		{
			//if (config == null)
			//	throw new ArgumentNullException("config");

			Config = new CEMConfig(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UISetUIMConfig)]
	public class Msg_UISetUIMConfig : AURAAppMessage
	{
		public UIMConfig Config
		{ get; private set; }

		public Msg_UISetUIMConfig(UIMConfig config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			Config = config;
		}

		public override ushort GetLength()
		{
			return UIMConfig.kUIMConfigSize;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			Config.PackBuffer(buf, ofs);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UISetCEMConfigADU)]
	public class Msg_UISetCEMConfigADU : AURAAppMessage
	{
		public CEMConfig Config
		{ get; private set; }

		public Msg_UISetCEMConfigADU(CEMConfig config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			Config = config;
		}

		public override ushort GetLength()
		{
			return CEMConfig.kCEMConfigSize;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			Config.PackBuffer(buf, ofs);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UICEMRangeData)]
	public class Msg_UICEMRangeData : AURAAppMessage
	{
		public uint Address
		{ get; private set; }

		public RCMRangeResult RangeResult
		{ get; private set; }

		public double Range
		{ get; private set; }

		public uint ChannelRiseOrNoise
		{ get; private set; }

		public int RangeErrEstimate
		{ get; private set; }

		public byte RCMRangeStatus
		{ get; private set; }

		public ushort RequesterLEDFlags
		{ get; private set; }

		public ushort ResponderLEDFlags
		{ get; private set; }

		public ushort VPeak
		{ get; private set; }

		public uint RCMTimestamp
		{ get; private set; }

		/// <summary>
		/// For '0 distance' issue debug only!
		/// </summary>
		public uint DataCheck
		{ get; private set; }

		public bool HasExpandedData
		{ get; private set; }

		public bool RCMIs2dot4OrLater
		{ get; private set; }

		public double SNR
		{ get; private set; }

		public bool RangeWasDiscarded
		{ get; private set; }

		public Msg_UICEMRangeData(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Address = AURADataUtil.UnpackDNTAddress(buf, ndx);
			ndx += 3;

			byte rr = buf[ndx++];
			RangeResult = Enum.IsDefined(typeof(RCMRangeResult), rr) ? (RCMRangeResult)rr : RCMRangeResult.InvalidRangeResultValue;

			Range = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			ChannelRiseOrNoise = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			RangeErrEstimate = BitConverter.ToInt32(buf, ndx);
			ndx += 4;

			RCMRangeStatus = buf[ndx++];

			RequesterLEDFlags = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			ResponderLEDFlags = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			VPeak = BitConverter.ToUInt16(buf, ndx);
			ndx += 2;

			RCMTimestamp = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			DataCheck = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			if (len > 31)
			{
				HasExpandedData = true;
				byte flags = buf[ndx++];

				RCMIs2dot4OrLater = ((flags & 1) != 0);

				if (RCMIs2dot4OrLater)
				{
					// Sanity, just in case...
					if ((VPeak == 0) || (ChannelRiseOrNoise == 0))
						SNR = 0.0;
					else
						SNR = 20.0 * Math.Log10((double)VPeak / ChannelRiseOrNoise);
				}

				// This flag appears in CEM v. 2.11.0+
				if ((flags & (1 << 1)) != 0)
					RangeWasDiscarded = true;
			}
			else
				HasExpandedData = false;
		}
	}


	[AURAMessage(AURAMsgOpcode.UICEMPeerUpdate)]
	public class Msg_UICEMPeerUpdate : AURAAppMessage
	{
		public uint Address
		{ get; private set; }

		public sbyte LastRSSI
		{ get; private set; }

		public string NamePrefix
		{ get; private set; }

		public uint NameNumber
		{ get; private set; }

		public string Name
		{ get; private set; }

		public bool ExtendedDataAvailable
		{ get; private set; }

		public double Lat
		{ get; private set; }

		public double Lng
		{ get; private set; }

		public uint GPSFixTime
		{ get; private set; }

		public double Vel
		{ get; private set; }

		public byte FrontOffset
		{ get; private set; }

		public byte RearOffset
		{ get; private set; }

		public ushort Orientation
		{ get; private set; }

		public ushort Status
		{ get; private set; }

		public ushort Heading
		{ get; private set; }

		public Msg_UICEMPeerUpdate(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Address = AURADataUtil.UnpackDNTAddress(buf, ndx);
			ndx += 3;

			LastRSSI = (sbyte)buf[ndx++];

			NamePrefix = ASCIIEncoding.ASCII.GetString(buf, ndx, 2);
			ndx += 2;

			NameNumber = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			// The message payload ended here (10 bytes) for CEM v. 2.0.12; for 2.0.13 and later,
			// it continues.

			if (len > (ndx - ofs))
			{
				ExtendedDataAvailable = true;

				Lat = BitConverter.ToInt32(buf, ndx) * 1e-7;
				ndx += 4;

				Lng = BitConverter.ToInt32(buf, ndx) * 1e-7;
				ndx += 4;

				GPSFixTime = BitConverter.ToUInt32(buf, ndx);
				ndx += 4;

				Vel = BitConverter.ToUInt16(buf, ndx) * 0.01;
				ndx += 2;

				FrontOffset = buf[ndx++];
				RearOffset = buf[ndx++];

				Orientation = (ushort)(buf[ndx++] * 2);

				Status = BitConverter.ToUInt16(buf, ndx);
				ndx += 2;

				Heading = (ushort)(buf[ndx++] * 2);

				// The payload ended here (30 bytes) thru CEM v. 2.13.2; for 2.13.3 and later, it
				// continues.

				if (len > (ndx - ofs))
				{
					if ((buf[ndx] != 0x20) && (buf[ndx] != 0))
					{
						int addlPrefixLen = ((buf[ndx + 1] != 0x20) && (buf[ndx + 1] != 0)) ? 2 : 1;

						NamePrefix += ASCIIEncoding.ASCII.GetString(buf, ndx, addlPrefixLen);
					}
					ndx += 2;

				}
			}
			else
				ExtendedDataAvailable = false;

			Name = NamePrefix + NameNumber.ToString("d6");
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UICEMSubSystemStatus)]
	public class Msg_UICEMSubSystemStatus : AURAAppMessage
	{
		public CEMSubSystem SubSystem
		{ get; private set; }

		public byte Status
		{ get; private set; }

		public CEMCommsStatus CommsStatus
		{
			get
			{
				if (SubSystem != CEMSubSystem.Comms)
					throw new InvalidOperationException("SubSystem is " + SubSystem.ToString() + ", not Comms");
				CEMCommsStatus status = CEMCommsStatus.InvalidStatusValue;
				if (Enum.IsDefined(typeof(CEMCommsStatus), Status))
					status = (CEMCommsStatus)Status;
				return status;
			}
		}

		public CEMGPSStatus GPSStatus
		{
			get
			{
				if (SubSystem != CEMSubSystem.GPS)
					throw new InvalidOperationException("SubSystem is " + SubSystem.ToString() + ", not GPS");
				CEMGPSStatus status = CEMGPSStatus.InvalidStatusValue;
				if (Enum.IsDefined(typeof(CEMGPSStatus), Status))
					status = (CEMGPSStatus)Status;
				return status;
			}
		}

		public CEMRCMStatus RCMStatus
		{
			get
			{
				if (SubSystem != CEMSubSystem.RCM)
					throw new InvalidOperationException("SubSystem is " + SubSystem.ToString() + ", not RCM");
				CEMRCMStatus status = CEMRCMStatus.InvalidStatusValue;
				if (Enum.IsDefined(typeof(CEMRCMStatus), Status))
					status = (CEMRCMStatus)Status;
				return status;
			}
		}

		public Msg_UICEMSubSystemStatus(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			CEMSubSystem subSystem = CEMSubSystem.InvalidSubSystemValue;
			if (Enum.IsDefined(typeof(CEMSubSystem), buf[ndx]))
				subSystem = (CEMSubSystem)buf[ndx];
			SubSystem = subSystem;
			++ndx;

			Status = buf[ndx++];
		}
	}



	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	public class CEMLogStatus
	{
		public uint PLogMaxEntryCount
		{ get; private set; }

		public uint PLogHead
		{ get; private set; }

		public uint PLogTail
		{ get; private set; }

		public uint SLogMaxEntryCount
		{ get; private set; }

		public uint SLogHead
		{ get; private set; }

		public uint SLogTail
		{ get; private set; }

		public CEMLogStatus(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			PLogMaxEntryCount = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			PLogHead = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			PLogTail = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			SLogMaxEntryCount = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			SLogHead = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			SLogTail = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.LogStatus)]
	public class Msg_LogStatus : AURAAppMessage
	{
		public CEMLogStatus Data
		{ get; private set; }

		public Msg_LogStatus(byte[] buf, ushort ofs, ushort len)
		{
			Data = new CEMLogStatus(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UIGetLogStatus)]
	public class Msg_UIGetLogStatus : AURAAppMessage
	{
		public Msg_UIGetLogStatus()
		{
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UILogStatus)]
	public class Msg_UILogStatus : AURAAppMessage
	{
		public Metrom.AURA.Messaging.CEMLogStatus Data
		{ get; private set; }

		public Msg_UILogStatus(byte[] buf, ushort ofs, ushort len)
		{
			Data = new Metrom.AURA.Messaging.CEMLogStatus(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UIGetLogEntry)]
	public class Msg_UIGetLogEntry : AURAAppMessage
	{
		public byte WhichLog
		{ get; private set; }

		public uint EntryIndex
		{ get; private set; }

		public Msg_UIGetLogEntry(byte whichLog, uint entryIndex)
		{
			WhichLog = whichLog;
			EntryIndex = entryIndex;
		}

		public override ushort GetLength()
		{
			return 5;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			buf[ndx++] = WhichLog;

			BitConverter.GetBytes(EntryIndex).CopyTo(buf, ndx);
			ndx += 4;

			if ((ndx - ofs) != GetLength())
				throw new ApplicationException(string.Format("Msg_MgmtUIGetLogEntry(): packed len {0} does not match GetLength() {1}", ndx - ofs, GetLength()));
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UILogEntry)]
	public class Msg_UILogEntry : AURAAppMessage
	{
		public Metrom.AURA.Messaging.CEMLogEntry Data
		{ get; private set; }

		public Msg_UILogEntry(byte[] buf, ushort ofs, ushort len)
		{
			Data = new Metrom.AURA.Messaging.CEMLogEntry(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	public class CEMLogEntry
	{
		public byte Status
		{ get; private set; }

		public byte WhichLog
		{ get; private set; }

		public uint EntryIndex
		{ get; private set; }

		public uint TimestampS
		{ get; private set; }

		public byte Timestamp10Ms
		{ get; private set; }

		public byte[] EntryPayload
		{ get; private set; }

		public CEMLogEntry(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Status = buf[ndx++];
			WhichLog = buf[ndx++];

			EntryIndex = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			TimestampS = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;

			Timestamp10Ms = buf[ndx++];

			EntryPayload = new byte[CEM.LogEntryPayloadSize];
			Array.Copy(buf, ndx, EntryPayload, 0, CEM.LogEntryPayloadSize);
			ndx += CEM.LogEntryPayloadSize;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.LogEntry)]
	public class Msg_LogEntry : AURAAppMessage
	{
		public CEMLogEntry Data
		{ get; private set; }

		public Msg_LogEntry(byte[] buf, ushort ofs, ushort len)
		{
			Data = new CEMLogEntry(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIGetProfile)]
	public class Msg_UIGetProfile : AURAAppMessage
	{
		public int Slot
		{ get; private set; }

		public Msg_UIGetProfile(int slot)
		{
			if (slot > 255)
				throw new ArgumentException("slot may not be greater than 255", "slot");

			Slot = slot;
		}

		public override ushort GetLength()
		{
			return 1;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			buf[ndx++] = (byte)Slot;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIProfile)]
	public class Msg_UIProfile : AURAAppMessage
	{
		public AURAProfile Profile
		{ get; private set; }

		public int Slot
		{ get; private set; }

		public Msg_UIProfile(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			int slot = 0;
			Profile = ProfileMapper.FetchProfileFromMsgPayload(GlobalData.ConfigVersion, buf, ndx, len, ref slot);

			Slot = slot;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UISetProfile)]
	public class Msg_UISetProfile : AURAAppMessage
	{
		public AURAProfile Profile
		{ get; private set; }

		public int Slot
		{ get; private set; }

		public Msg_UISetProfile(AURAProfile profile, int slot)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			Profile = profile;
			Slot = slot;
		}

		public override ushort GetLength()
		{
			// Selectable Profiles is a new feature. We don't as of yet have multiple message payloads
			// to manage.

			return 70;  // haven't figured out where to save this kind of constant...
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ProfileMapper.PackProfileIntoMsgPayload(GlobalData.ConfigVersion, Profile, Slot, buf, ofs);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UISetUIMDiagConfig)]
	public class Msg_UISetUIMDiagConfig : AURAAppMessage
	{
		public UIMDiagConfig DiagConfig
		{ get; private set; }

		public Msg_UISetUIMDiagConfig(UIMDiagConfig diagConfig)
		{
			if (diagConfig == null)
				throw new ArgumentNullException("diagConfig");

			DiagConfig = diagConfig;
		}

		public override ushort GetLength()
		{
			return UIMDiagConfig.kUIMDiagConfigSize;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			DiagConfig.PackBuffer(buf, ofs);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UISetCEMInternals)]
	public class Msg_UISetCEMInternals : AURAAppMessage
	{
		public CEMInternals Internals
		{ get; private set; }

		public Msg_UISetCEMInternals(CEMInternals internals)
		{
			if (internals == null)
				throw new ArgumentNullException("internals");

			Internals = internals;
		}

		public override ushort GetLength()
		{
			return CEMInternals.kCEMInternalsSize;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			Internals.PackBuffer(buf, ofs);
		}
	}



	[AURAMessage(AURAMsgOpcode.UIGetLogAcknowledge)]
	public class Msg_UIGetLogAcknowledge : AURAAppMessage
	{
		private string _text;

		public string Text
		{ 
			get { return _text; } 
		}

		public Msg_UIGetLogAcknowledge(byte[] buf, ushort ofs, ushort len)
		{
			_text = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ofs, len);
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMReflashInit)]
	public class Msg_UICEMReflashInit : AURAAppMessage
	{
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICommandAck)]
	public class Msg_UICommandAck : AURAAppMessage
	{
		public byte Source
		{ get; private set; }

		public byte Result
		{ get; private set; }

		public uint ResultData
		{ get; private set; }

		public Msg_UICommandAck(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Source = buf[ndx++];
			Result = buf[ndx++];

			ResultData = BitConverter.ToUInt32(buf, ndx);
			ndx += 4;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMReflashData)]
	public class Msg_UICEMReflashData : AURAAppMessage
	{
		public byte[] Data
		{ get; private set; }

		public ushort Length
		{ get; private set; }

		public Msg_UICEMReflashData(uint ofs, ushort len, byte[] buf)
		{
			if (buf == null)
				throw new ArgumentNullException("buf");
			if ((len == 0) || (len > 240))
				throw new ArgumentException("len must be [1, 240]", "len");

			Data = new byte[len];
			Length = len;

			Array.Copy(buf, ofs, Data, 0, len);
		}

		public override ushort GetLength()
		{
			return Length;
		}

		public override void PackBuffer(byte[] buf, ushort ofs)
		{
			ushort ndx = ofs;

			Array.Copy(Data, 0, buf, ndx, Length);
			ndx += Length;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UICEMReflashFini)]
	public class Msg_UICEMReflashFini : AURAAppMessage
	{
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIRequestPMVersion)]
	public class Msg_UIRequestPMVersion : AURAAppMessage
	{
	}

	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIPMVersion)]
	public class Msg_UIPMVersion : AURAAppMessage
	{
		public byte Major
		{ get; private set; }

		public byte Minor
		{ get; private set; }

		public byte Tweek
		{ get; private set; }

		public Msg_UIPMVersion(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Major = buf[ndx++];
			Minor = buf[ndx++];
			Tweek = buf[ndx++];
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UIGetCEMAccelCalibData)]
	public class Msg_UIGetCEMAccelCalibData : AURAAppMessage
	{
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	/// <summary>
	/// 
	/// </summary>
	/// 
	[AURAMessage(AURAMsgOpcode.UICEMAccelCalibData)]
	public class Msg_UICEMAccelCalibData : AURAAppMessage
	{
		public Vector GravityOffsetVec
		{ get; private set; }

		public Vector FrontUnitVec
		{ get; private set; }

		public Msg_UICEMAccelCalibData(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			GravityOffsetVec = new Vector(3);
			GravityOffsetVec[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
			GravityOffsetVec[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
			GravityOffsetVec[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;

			FrontUnitVec = new Vector(3);
			FrontUnitVec[0] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
			FrontUnitVec[1] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
			FrontUnitVec[2] = BitConverter.ToSingle(buf, ndx);
			ndx += 4;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIOTAinit)]
	public class Msg_UIOTAinit : AURAAppMessage
	{
		public int Length
		{ get; private set; }

		public string Name
		{ get; private set; }


		public Msg_UIOTAinit(byte[] buf, ushort ofs, ushort len)
		{
			ushort ndx = ofs;

			Length = BitConverter.ToInt32(buf, ndx);
			ndx += 4;

			Name = (len == 0) ? "" : ASCIIEncoding.ASCII.GetString(buf, ndx, len);
			ndx += 32;
		}
	}


	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIOTAdata)]
	public class Msg_UIOTAdata : AURAAppMessage
	{
		public byte[] Data
		{ get; private set; }


		public Msg_UIOTAdata(byte[] buf, ushort ofs, ushort len)
		{
            Data = new byte[230];//[2048];
			Array.Copy(buf, ofs, Data, 0, len);		
		}
	}

    
	// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


	[AURAMessage(AURAMsgOpcode.UIOTAack)]
	public class Msg_UIOTAack : AURAAppMessage
	{

	}


    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


    [AURAMessage(AURAMsgOpcode.UIOTAcomplete)]
    public class Msg_UIOTAcomplete : AURAAppMessage
    {
        public bool Success
        { get; private set; }

        public Msg_UIOTAcomplete(byte[] buf, ushort ofs, ushort len)
        {
            ushort ndx = ofs;
            Success = buf[ndx] == 0 ? true : false; // 0x00 or 0xFA
        }
    }
}