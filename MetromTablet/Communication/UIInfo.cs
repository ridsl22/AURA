using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDI.WPF.Tools;

namespace MetromTablet.Communication
{

	public class UIInfo : PropChangedBase
	{
		private bool inConfigSeq_ = false;
		private bool useSIUnits_ = false;
		private bool deviceIsConnected_ = false;
		private bool portIsOpen_ = false;

		public bool DeviceIsConnected
		{
			get { return deviceIsConnected_; }
			set
			{
				if (value != deviceIsConnected_)
				{
					deviceIsConnected_ = value;
					PropChanged("DeviceIsConnected");
					PropChanged("CanConfigDevice");
				}
			}
		}

		public bool InConfigSeq
		{
			get { return inConfigSeq_; }
			set
			{
				if (value != inConfigSeq_)
				{
					inConfigSeq_ = value;
					PropChanged("InConfigSeq");
					PropChanged("CanConfigDevice");
					PropChanged("CanUpdateFirmware");
				}
			}
		}

		public bool UseSIUnits
		{
			get { return useSIUnits_; }
			set
			{
				if (value != useSIUnits_)
				{
					useSIUnits_ = value;
					PropChanged("UseSIUnits");
				}
			}
		}


		public bool PortIsOpen
		{
			get { return portIsOpen_; }
			set
			{
				if (value != portIsOpen_)
				{
					portIsOpen_ = value;
					PropChanged("PortIsOpen");
					PropChanged("CanUpdateFirmware");
				}
			}
		}


		public bool CanUpdateFirmware
		{ get { return PortIsOpen && !InConfigSeq; } }


		/// <summary>
		/// 
		/// </summary>
		/// 
		public bool CanInteractWithDevice
		{
			get { return DeviceIsConnected && !InConfigSeq; }
		}
	}
}
