using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDI.WPF.Tools;
using Metrom.AURA.Base;
using Metrom.Base;
using Metrom.WPF;

namespace MetromTablet.Communication
{
	/// <summary>
	/// Holds a set of descriptive / identifying information pertaining to an AURA device
	/// connected to the PC.
	/// </summary>
	/// 
	public class DeviceInfo : PropChangedBase
	{
		#region Types

		[Flags]
		private enum DataItem
		{
			None = 0,
			CEMConfig = (1 << 0),
			UIMConfig = (1 << 1),
			UIMHello = (1 << 2),
			CEMHello = (1 << 3),
			CEMSSS = (1 << 4),
			OAState = (1 << 5)
		}

		#endregion

		#region Constants

		private const byte kLocalInvalidNetworkId = 0xff;  // 'local' - meaningful only inside this class!

		#endregion

		#region Instance Fields

		private TimeoutTimer tmoConnect_ = new TimeoutTimer();

		private bool devicePresent_;
		private ThreePartVersion cemVersion_;
		private ThreePartVersion uimVersion_;
		private ThreePartVersion rcmVersion_;
		private TwoPartVersion configVersion_;
		private string machineName_;
		private uint address_;
		private byte networkId_;

		private CEMCommsStatus dntStatus_;
		private CEMGPSStatus gpsStatus_;
		private CEMRCMStatus rcmStatus_;

		private bool inConnectSequence_;

		private Action registeredConnectCompleteAction_;
		private Action connectCompleteAction_;

		private DataItem dataItems_ = DataItem.None;

		#endregion

		#region Properties

		public bool GotCEMConfig
		{
			get { return GetItemsPresent(DataItem.CEMConfig); }
			private set { SetItemsPresent(DataItem.CEMConfig, value); }
		}

		public bool GotUIMConfig
		{
			get { return GetItemsPresent(DataItem.UIMConfig); }
			private set { SetItemsPresent(DataItem.UIMConfig, value); }
		}

		public bool GotUIMHello
		{
			get { return GetItemsPresent(DataItem.UIMHello); }
			private set { SetItemsPresent(DataItem.UIMHello, value); }
		}

		public bool GotCEMHello
		{
			get { return GetItemsPresent(DataItem.CEMHello); }
			private set { SetItemsPresent(DataItem.CEMHello, value); }
		}

		public bool GotCEMSubSystemStatus
		{
			get { return GetItemsPresent(DataItem.CEMSSS); }
			private set { SetItemsPresent(DataItem.CEMSSS, value); }
		}

		public bool GotOAState
		{
			get { return GetItemsPresent(DataItem.OAState); }
			private set { SetItemsPresent(DataItem.OAState, value); }
		}

		public Action ConnectSequenceComplete
		{
			get { return registeredConnectCompleteAction_; }
			set
			{
				registeredConnectCompleteAction_ = value;
				connectCompleteAction_ = value;
			}
		}


		/// <summary>
		/// Gets or sets (private) a bool indicating whether an AURA device has been detected on the
		/// connected serial port.
		/// </summary>
		/// 
		public bool DeviceIsPresent
		{
			get { return devicePresent_; }
			private set
			{
				if (value != devicePresent_)
				{
					//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT: DeviceIsPresent = {0}", value));

					devicePresent_ = value;
					PropChanged("DeviceIsPresent");
				}
			}
		}


		/// <summary>
		/// Gets or sets (private) the three-part CEM version number.
		/// </summary>
		/// 
		public ThreePartVersion CEMVersion
		{
			get { return cemVersion_; }
			private set
			{
				cemVersion_ = value;
				PropChanged("CEMVersion");
			}
		}


		/// <summary>
		/// Gets or sets (private) the three-part UIM version number.
		/// </summary>
		/// 
		public ThreePartVersion UIMVersion
		{
			get { return uimVersion_; }
			private set
			{
				uimVersion_ = value;
				PropChanged("UIMVersion");
			}
		}


		/// <summary>
		/// Gets or sets (private) the three-part RCM version number.
		/// </summary>
		/// 
		public ThreePartVersion RCMVersion
		{
			get { return rcmVersion_; }
			private set
			{
				rcmVersion_ = value;
				PropChanged("RCMVersion");
			}
		}


		/// <summary>
		/// Gets or sets (private) the two-part Configuration version number.
		/// </summary>
		/// 
		public TwoPartVersion ConfigVersion
		{
			get { return configVersion_; }
			private set
			{
				configVersion_ = value;
				GlobalData.ConfigVersion = configVersion_;
				PropChanged("ConfigVersion");
			}
		}


		/// <summary>
		/// Gets or sets (private) the machine name.
		/// </summary>
		/// 
		public string MachineName
		{
			get { return machineName_; }
			private set
			{
				machineName_ = (value == null) ? "" : value;
				PropChanged("MachineName");
			}
		}


		/// <summary>
		/// Gets or sets (private) the Comms MAC address (also used as the RCM node ID).
		/// </summary>
		/// 
		public uint Address
		{
			get { return address_; }
			private set
			{
				if (value != address_)
				{
					address_ = value;
					PropChanged("Address");
					PropChanged("AddressText");
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public byte NetworkId
		{
			get { return networkId_; }
			private set
			{
				if (value != networkId_)
				{
					networkId_ = value;
					PropChanged("NetworkId");
					PropChanged("NetworkIdText");
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public string AddressText
		{ get { return (address_ == AURADef.InvalidMACAddress) ? "" : address_.ToString("x6"); } }


		public string NetworkIdText
		{ get { return (networkId_ == kLocalInvalidNetworkId) ? "" : networkId_.ToString(); } }

		/// <summary>
		/// Gets or sets (private) the 32-bit CEM flags value conveyed in the CEMHello msg.
		/// </summary>
		/// 
		public uint CEMInfoFlags
		{ get; private set; }


		/// <summary>
		/// Gets or sets (private) the 32-bit UIM flags value conveyed in the UIMHello msg.
		/// </summary>
		/// 
		public uint UIMInfoFlags
		{ get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// 
		public CEMCommsStatus DNTSSStatus
		{
			get { return dntStatus_; }
			set
			{
				if (value != dntStatus_)
				{
					dntStatus_ = value;
					PropChanged("DNTSSStatus");
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public CEMGPSStatus GPSSSStatus
		{
			get { return gpsStatus_; }
			set
			{
				if (value != gpsStatus_)
				{
					gpsStatus_ = value;
					PropChanged("GPSSSStatus");
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public CEMRCMStatus RCMSSStatus
		{
			get { return rcmStatus_; }
			set
			{
				if (value != rcmStatus_)
				{
					rcmStatus_ = value;
					PropChanged("RCMSSStatus");
				}
			}
		}

		#endregion

		#region Lifetime Management

		/// <summary>
		/// Default ctor.
		/// </summary>
		/// 
		public DeviceInfo()
		{
			ResetImpl(false);

			tmoConnect_.TimeoutAction = ConnectSequenceTimeout;
		}

		#endregion

		#region Operations

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inDemoMode"></param>
		/// 
		public void Reset(bool inDemoMode = false)
		{
			ResetImpl(true, inDemoMode);
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public void ConnectSequenceStarting()
		{
			if (inConnectSequence_)
				throw new InvalidOperationException("Already in connect sequence");

			inConnectSequence_ = true;
			tmoConnect_.Start(4000);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// 
		public void NoteCEMConfigMsg(Msg_UICEMConfigInfoADU msg)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");

			if (!inConnectSequence_)
				return;  // EARLY RETURN!

			GotCEMConfig = true;
			DeviceIsPresent = true;

			// In case the CEM has an "old" (pre-Hello) firmware version, we'll establish an initial
			// Config version that can be replaced should a CEMHello come in. This version will be:
			//
			// 1.0: if the CEM supports 4-char vehicle names (>= v. 2.9.0)
			// 0.5: if the CEM doesn't support 4-char vehicle names

			ConfigVersion = (((msg.Config.Caps) & AURACaps.FourCharName) != 0)
			  ? new TwoPartVersion(1, 0)
			  : new TwoPartVersion(0, 5);

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: Got CEMConfig msg.");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// 
		public void NoteUIMConfigMsg(Msg_UIUIMConfig msg)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");

			if (!inConnectSequence_)
				return;  // EARLY RETURN!

			GotUIMConfig = true;
			DeviceIsPresent = true;

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: Got UIMConfig msg.");
		}


		/// <summary>
		/// Records the receipt and contents of the UIMHello message.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		/// 
		public void NoteUIMHelloMsg(Msg_UIMHello msg)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");

			if (!inConnectSequence_)
				return;  // EARLY RETURN!

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: Got UIMHello msg.");

			GotUIMHello = true;
			DeviceIsPresent = true;

			UIMVersion = msg.UIMVersion;
			UIMInfoFlags = msg.Flags;

			// Note that we don't call RaiseCompleteActionIfComplete() here, because the receipt of
			// a UIMHello msg isn't sufficient to determine completion (it must be followed by a CEM
			// response: either CEMHello or, for old CEMs that don't implement Hello, at least one
			// SSS and one OAState msg).
		}


		/// <summary>
		/// Records the receipt and contents of the CEMHello message from the connected device.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		/// 
		public void NoteCEMHelloMsg(Msg_CEMHello msg)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");

			// NOTE: The CEMHello message will be sent in the following circumstances:
			// 1) At CEM startup (automatic).
			// 2) On loss of RCM communication (automatic).
			// 3) On resumption of normal RCM communication (automatic).
			// 4) In response to a CEMHello sent by the PC.
			// 5) At the start and end of a GPS module reset cycle (15 sec TSM power cycle).
			//
			// Because of the automatic cases, we always want to copy data from the message, because
			// the CEM or RCM versions may have changed (the former in the case of a CEM firmware
			// update; the latter in the case of normal startup or a TSM disconnect, reconnect, or
			// failure.

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: Got CEMHello msg.");

			GotCEMHello = true;
			DeviceIsPresent = true;

			CEMVersion = msg.CEMVersion;
			ConfigVersion = msg.ConfigVersion;
			MachineName = msg.MachineName;
			CEMInfoFlags = msg.Flags;

			// General note concerning Address and RCMVersion: it's possible that the DNT subsystem
			// is working fine (required for the CEM to get the DNT MAC address) but the RCM subsystem
			// isn't, or that this msg was sent after DNT "radio ready" but before the RCM "get status"
			// has completed. In these cases we can have an Address but no RCMVersion. This "state" isn't
			// covered by the "Got" predicates. We don't really need to add a predicate for it, though
			// - we can deduce this by seeing a meaningful address but RCMVersion is null. It *is*
			// important information, however: if an RCM board fails (serial communication or power),
			// we'd see the MAC address but no RCM version information.

			Address = msg.Address;
			NetworkId = msg.NetworkId;

			RCMVersion = msg.RCMVersion.IsValid ? msg.RCMVersion : null;

			RaiseCompleteActionIfComplete();
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public void NoteCEMSubSystemStatusMsg()
		{
			if (!inConnectSequence_)
				return;  // EARLY RETURN!

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: Got CEMSubSystemStatus msg.");

			GotCEMSubSystemStatus = true;
			DeviceIsPresent = true;

			// Note that we don't call RaiseCompleteActionIfComplete() here, because the receipt of
			// an SSS msg isn't sufficient to determine completion (it must be followed by an OAState
			// msg).
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		public void NoteOAStateMsg()
		{
			if (!inConnectSequence_)
				return;  // EARLY RETURN!

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: Got OAState msg.");

			GotOAState = true;
			DeviceIsPresent = true;

			RaiseCompleteActionIfComplete();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="config"></param>
		/// 
		public void NoteCEMConfigChange(CEMConfig config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			MachineName = config.Name;
			NetworkId = config.DNTNetworkId;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// 
		public bool ConfigIsAtLeast(string compareVer)
		{
			return (configVersion_ == null) ? false : configVersion_.IsAtLeast(compareVer);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="compareVer"></param>
		/// <returns></returns>
		/// 
		public bool ConfigIsEarlierThan(string compareVer)
		{
			return (configVersion_ == null) ? false : configVersion_.IsEarlierThan(compareVer);
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Resets the device data in preparation for a connect attempt.
		/// </summary>
		/// <param name="isUserOperation"></param>
		/// <param name="inDemoMode"></param>
		/// 
		private void ResetImpl(bool isUserOperation, bool inDemoMode = false)
		{
			//if (isUserOperation)
				//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT: DeviceInfo reset{0}.", inDemoMode ? " (demo mode)" : ""));

			if (inDemoMode)
			{
				DeviceIsPresent = true;
				dataItems_ = DataItem.UIMHello | DataItem.CEMHello | DataItem.UIMConfig | DataItem.CEMConfig | DataItem.CEMSSS | DataItem.OAState;
				CEMVersion = new ThreePartVersion(3, 1, 16);
				UIMVersion = new ThreePartVersion(3, 1, 9);
				RCMVersion = new ThreePartVersion(2, 8, 0);
				ConfigVersion = new TwoPartVersion(1, 1);
				MachineName = "DEMO012345";
				Address = 0xf08ae7;
				NetworkId = 6;
				CEMInfoFlags = 0;
				UIMInfoFlags = 0;
				DNTSSStatus = CEMCommsStatus.Ready;
				GPSSSStatus = CEMGPSStatus.FixGood;
				RCMSSStatus = CEMRCMStatus.CommOk;
			}
			else
			{
				DeviceIsPresent = false;
				dataItems_ = DataItem.None;
				CEMVersion = null;
				UIMVersion = null;
				RCMVersion = null;
				ConfigVersion = null;
				MachineName = "";
				Address = AURADef.InvalidMACAddress;
				NetworkId = kLocalInvalidNetworkId;
				CEMInfoFlags = 0;
				UIMInfoFlags = 0;
				DNTSSStatus = CEMCommsStatus.InvalidStatusValue;
				GPSSSStatus = CEMGPSStatus.InvalidStatusValue;
				RCMSSStatus = CEMRCMStatus.InvalidStatusValue;

				connectCompleteAction_ = registeredConnectCompleteAction_;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		/// 
		private bool GetItemsPresent(DataItem items)
		{
			return (dataItems_ & items) == items;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="items"></param>
		/// <param name="present"></param>
		/// 
		private void SetItemsPresent(DataItem items, bool present)
		{
			if (present)
				dataItems_ |= items;
			else
				dataItems_ &= ~items;
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		private void ConnectSequenceTimeout()
		{
			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: ADU Connect Sequence response TIMEOUT. **********");

			RaiseCompleteAction();
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		private void RaiseCompleteActionIfComplete()
		{
			if (!inConnectSequence_)
				return;  // EARLY RETURN!

			if ((connectCompleteAction_ != null) &&
				(GetItemsPresent(DataItem.CEMHello) || GetItemsPresent(DataItem.CEMSSS | DataItem.OAState)))
			{
				tmoConnect_.Stop();

				RaiseCompleteAction();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		private void RaiseCompleteAction()
		{
			inConnectSequence_ = false;

			if (DeviceIsPresent)
			{
				if (!GotCEMHello)
				{
					CEMVersion = new ThreePartVersion();
					RCMVersion = new ThreePartVersion();
				}

				if (!GotUIMHello)
					UIMVersion = new ThreePartVersion();
			}

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, "CONNECT: DeviceInfo: Raising completion action; current state is:");

			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT:       DeviceIsPresent = {0}", DeviceIsPresent));
			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT:          GotUIMConfig = {0}", GotUIMConfig));
			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT:           GotUIMHello = {0}", GotUIMHello));
			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT:           GotCEMHello = {0}", GotCEMHello));
			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT: GotCEMSubSystemStatus = {0}", GotCEMSubSystemStatus));
			//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect, string.Format("CONNECT:            GotOAState = {0}", GotOAState));

			Action callback = connectCompleteAction_;
			connectCompleteAction_ = null;

			try
			{
				callback();
			}
			catch (Exception ex)
			{
				//MainWindow.Log.WriteToLog(LogConfigAURA.ADUConnect,
				 // string.Format("DeviceInfo.RaiseCompleteActionIfComplete(): Exception during callback invocation: {0}: {1}", ex.GetType(), ex.Message));
			}
		}

		#endregion
	}
}
