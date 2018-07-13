using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDI.Scientific;
using Metrom.AURA.Base;

namespace MetromTablet.Communication
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	public class RangeData
	{
		#region Properties

		/// <summary>
		/// 
		/// </summary>
		/// 
		public uint Address
		{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public RCMRangeResult RangeResult
		{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public double Range
		{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public uint ChannelRise
		{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// 
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

		public double SNR
		{ get; private set; }

		public string SNRText
		{ get { return SNR.ToString("f2"); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RangeResultText
		{ get { return RangeResult.ToString(); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RangeText
		{
			get
			{
				if (RangeResult == RCMRangeResult.InvalidRangeResultValue)
					return "?";
				else
					return Range.ToString("f2");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RangeFeetText
		{
			get
			{
				if (RangeResult == RCMRangeResult.InvalidRangeResultValue)
					return "?";
				else
					return (Range * SciCon.M_TO_FT).ToString("f2");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string ChannelRiseText
		{ get { return ChannelRise.ToString(); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RangeErrEstimateText
		{ get { return RangeErrEstimate.ToString(); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RCMRangeStatusText
		{ get { return "0x" + RCMRangeStatus.ToString("x2"); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string VpeakText
		{ get { return VPeak.ToString(); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string RequesterLEDFlagsText
		{ get { return "0x" + RequesterLEDFlags.ToString("x4"); } }

		/// <summary>
		/// 
		/// </summary>
		/// 
		public string ResponderLEDFlagsText
		{ get { return "0x" + ResponderLEDFlags.ToString("x4"); } }

		#endregion

		#region Lifetime Management

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="rangeResult"></param>
		/// <param name="range"></param>
		/// <param name="channelRise"></param>
		/// <param name="rangeErrEstimate"></param>
		/// 
		public RangeData(
		  uint address,
		  RCMRangeResult rangeResult,
		  double range,
		  uint channelRise,
		  int rangeErrEstimate,
		  byte rcmRangeStatus,
		  ushort requesterLEDFlags,
		  ushort responderLEDFlags,
		  ushort vPeak,
		  uint rcmTimestamp,
		  double snr)
		{
			Address = address;
			RangeResult = rangeResult;
			Range = range;
			ChannelRise = channelRise;
			RangeErrEstimate = rangeErrEstimate;
			RCMRangeStatus = rcmRangeStatus;
			RequesterLEDFlags = requesterLEDFlags;
			ResponderLEDFlags = responderLEDFlags;
			VPeak = vPeak;
			RCMTimestamp = rcmTimestamp;
			SNR = snr;
		}

		#endregion
	}
}
