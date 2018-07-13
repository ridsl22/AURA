using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{

	public class EquipmentSettingsModel
	{

		public bool RemoteConfirm { get; set; }			// Enabled/Disabled
		public bool DualAuraMode { get; set; }			// Enabled/Disabled
		public string DualAuraAddress { get; set; }		// 6 hex chars
		public int Volume { get; set; }					// 0-3
		public ExtrnAudio ExtAudio { get; set; }
		public bool AutoBrake { get; set; }				// Enabled/Disabled
		public int AutoBrakeDelay { get; set; }			// 0-59 sec
		public NoGPSMode NoGPSMd { get; set; }			// Travel/Work
		public bool CrawlAlertOnly { get; set; }		// Enabled/Disabled
		public bool SilenceWorkModeAudio { get; set; }	// Enabled/Disabled
		public bool SilenceCrawlModeAudio { get; set; }	// Enabled/Disabled


		public EquipmentSettingsModel() { }


		public EquipmentSettingsModel(EquipmentSettingsModel model)
		{
			RemoteConfirm = model.RemoteConfirm;
			DualAuraMode = model.DualAuraMode;
			DualAuraAddress = model.DualAuraAddress;
			Volume = model.Volume;
			ExtAudio = model.ExtAudio;
			AutoBrake = model.AutoBrake;
			AutoBrakeDelay = model.AutoBrakeDelay;
			NoGPSMd = model.NoGPSMd;
			CrawlAlertOnly = model.CrawlAlertOnly;
			SilenceWorkModeAudio = model.SilenceWorkModeAudio;
			SilenceCrawlModeAudio = model.SilenceCrawlModeAudio;
		}


		public string ExternalAudioToString()
		{
			switch (ExtAudio)
			{
				case ExtrnAudio.None:
					return "None";
				case ExtrnAudio.ManualSilence:
					return "Manual silence";
				case ExtrnAudio.AutoSilence:
					return "Auto-silence";
				case ExtrnAudio.AutoSilenceReminders:
					return "Auto-silence+reminders";
			}
			return String.Empty;
		}


		public string VolumeToString(int volume)
		{
			switch (volume)
			{
				case 1:
					return "1=low";
				case 2:
					return "2=medium";
				case 3:
					return "3=loud";
			}
			return "";
		}
	}

	public enum ExtrnAudio
	{
		None = 0,
		ManualSilence,
		AutoSilence,
		AutoSilenceReminders
	}

	public enum NoGPSMode
	{
 		Travel,
		Work
	}

}
