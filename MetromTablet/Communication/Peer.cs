using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDI.WPF.Tools;
using Metrom.AURA.Base;

namespace MetromTablet.Communication
{
	public class Peer : PropChangedBase
	{
		private const double kRSSIAverageWeight = 0.4;
		private const int kMaxRangeEntries = 40;

		private string name_ = "-";
		private sbyte lastRSSI_ = 0;
		private double averageRSSI_ = 0;
		private bool isActive_ = false;
		private bool heardFromPeerHack_ = false;
		private byte frontOffset_ = 0;
		private byte rearOffset_ = 0;
		private ushort status_ = 255;  // to force an update on first set even if 0.

		private uint rcmData_ = 0;
		private int rcmDataCount_ = 0;

		private int remoteLastRSSI_ = 0;
		private double remoteAverageRSSI_ = 0.0;
		private double remoteRangeSuccessRate_ = 0.0;
		private ushort remoteLastVPeak_ = 0;
		private ushort remoteLastCQorNoise_ = 0;
		private double remoteLastSNR_ = 0.0;

		/// <summary>
		/// Gets the DNT radio MAC address (also RCM node ID).
		/// </summary>
		/// 
		public uint Address
		{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string AddressText
		{ get { return Address.ToString("x6"); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string Name
		{
			get { return name_; }
			set
			{
				name_ = value;
				PropChanged("Name");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public sbyte LastRSSI
		{
			get { return lastRSSI_; }
			set
			{
				lastRSSI_ = value;

				averageRSSI_ = (averageRSSI_ == 0.0) ? value : (int)(value * kRSSIAverageWeight + averageRSSI_ * (1.0 - kRSSIAverageWeight));

				PropChanged("LastRSSI");
				PropChanged("AverageRSSI");

				HeardFromPeerHack = !HeardFromPeerHack;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public int AverageRSSI
		{ get { return (int)Math.Round(averageRSSI_); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public int RemoteLastRSSI
		{ get { return remoteLastRSSI_; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public int RemoteAverageRSSI
		{ get { return (int)Math.Round(remoteAverageRSSI_); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public ushort RemoteLastVPeak
		{ get { return remoteLastVPeak_; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RemoteLastVPeakText
		{ get { return (remoteLastVPeak_ == 0) ? "-" : remoteLastVPeak_.ToString(); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public ObservableCollection<RangeData> RangeList
		{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public bool IsActive
		{
			get { return isActive_; }
			set
			{
				if (isActive_ != value)
				{
					isActive_ = value;
					PropChanged("IsActive");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public bool HeardFromPeerHack
		{
			get { return heardFromPeerHack_; }
			set
			{
				heardFromPeerHack_ = value;
				PropChanged("HeardFromPeerHack");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public bool HasExtendedData
		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public byte FrontOffset
		{
			get { return frontOffset_; }
			set
			{
				if (frontOffset_ != value)
				{
					frontOffset_ = value;
					PropChanged("FrontOffset");
					PropChanged("FrontOffsetText");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string FrontOffsetText
		{ get { return HasExtendedData ? frontOffset_.ToString() : "-"; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public byte RearOffset
		{
			get { return rearOffset_; }
			set
			{
				if (rearOffset_ != value)
				{
					rearOffset_ = value;
					PropChanged("RearOffset");
					PropChanged("RearOffsetText");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RearOffsetText
		{ get { return HasExtendedData ? rearOffset_.ToString() : "-"; } }

		public ushort Status
		{
			get { return status_; }
			set
			{
				if (status_ != value)
				{
					status_ = value;
					PropChanged("Status");
					PropChanged("HasGPSText");
					PropChanged("HasOrientationText");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public double RCMRangeSuccessRate
		{
			get
			{
				double rate = 0.0;

				if (rcmDataCount_ > 0)
				{
					int goodCount = 0;
					int bit = 0;
					do
					{
						if ((rcmData_ & (1 << bit)) != 0)
							++goodCount;
					} while (++bit < rcmDataCount_);

					rate = (double)goodCount / rcmDataCount_;
				}

				return rate;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RCMRangeSuccessRateText
		{ get { return (RCMRangeSuccessRate * 100.0).ToString("f0") + "%"; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public double RemoteRCMRangeSuccessRate
		{ get { return remoteRangeSuccessRate_; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RemoteRCMRangeSuccessRateText
		{ get { return (remoteRangeSuccessRate_ * 100.0).ToString("f0") + "%"; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string HasGPSText
		{ get { return HasExtendedData ? (((status_ & (1 << 0)) != 0) ? "Y" : "n") : "-"; } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string HasOrientationText
		{ get { return HasExtendedData ? (((status_ & (1 << 1)) != 0) ? "Y" : "n") : "-"; } }

		public ushort RemoteCQorN
		{ get { return remoteLastCQorNoise_; } }

		public string RemoteSNRText
		{ get { return remoteLastSNR_.ToString("f2"); } }


		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="address"></param>
		/// 
		public Peer(uint address)
		{
			Address = address;
			IsActive = true;
			RangeList = new ObservableCollection<RangeData>();
			HasExtendedData = false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// 
		public void NewRangeData(Msg_UICEMRangeData msg)
		{
			RangeData data = new RangeData(
			  msg.Address,
			  msg.RangeResult,
			  msg.Range,
			  msg.ChannelRiseOrNoise,
			  msg.RangeErrEstimate,
			  msg.RCMRangeStatus,
			  msg.RequesterLEDFlags,
			  msg.ResponderLEDFlags,
			  msg.VPeak,
			  msg.RCMTimestamp,
			  msg.SNR);

			RangeList.Insert(0, data);

			if (rcmDataCount_ < 32)
				++rcmDataCount_;

			rcmData_ <<= 1;

			if (msg.RangeResult == RCMRangeResult.GotRange)
				rcmData_ |= 1;

			PropChanged("RCMRangeSuccessRate");
			PropChanged("RCMRangeSuccessRateText");

			if (RangeList.Count > kMaxRangeEntries)
				RangeList.RemoveAt(kMaxRangeEntries - 1);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// 
		//public void NewPerfData(Msg_UICEMPerfData msg)
		//{
		//	remoteLastRSSI_ = msg.RemoteRSSI;
		//	remoteAverageRSSI_ = (remoteAverageRSSI_ == 0.0) ? msg.RemoteRSSI : (int)(msg.RemoteRSSI * kRSSIAverageWeight + remoteAverageRSSI_ * (1.0 - kRSSIAverageWeight));
		//	remoteRangeSuccessRate_ = (double)msg.RangeSuccessCount / msg.RangeCount;
		//	remoteLastVPeak_ = msg.LastVPeak;
		//	if (msg.HasExpandedData)
		//	{
		//		remoteLastCQorNoise_ = msg.ChannelRiseOrNoise;
		//		remoteLastSNR_ = msg.SNR;
		//	}

		//	PropChanged("RemoteLastRSSI");
		//	PropChanged("RemoteAverageRSSI");
		//	PropChanged("RemoteLastVPeak");
		//	PropChanged("RemoteLastVPeakText");
		//	PropChanged("RemoteRCMRangeSuccessRate");
		//	PropChanged("RemoteRCMRangeSuccessRateText");
		//	PropChanged("RemoteCQorN");
		//	PropChanged("RemoteSNRText");
		//}
	}

}
