using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{

	public class OperatingOptionsModel
	{

		public bool AudioEnabled { get; set; }			// Enabled/Disabled
		//public WorkMode WorkMd { get; set; }
		public bool WorkMd { get; set; }
		public bool CrawlMode { get; set; }				// Enabled/Disabled
		public bool CrawlModeAlarms { get; set; }		// Enabled/Disabled
		public int WorkModeEntry { get; set; }			// 1-30 mph
		public double WorkModeHysteresis { get; set; }	// 0.5-5 mph
		public int CrawlModeEntry { get; set; }			// 1-30 mph
		public double CrawlModeHysteresis { get; set; }	// 0.5-5 mph
		public int ApproachTMZone { get; set; }			// 40-2500 ft
		public int InsideTMZone { get; set; }			// 30-2500 ft
		public int CriticalTMZone { get; set; }			// 20-2500 ft
		public int ApproachWMZone { get; set; }			// 30-2500 ft
		public int InsideWMZone { get; set; }			// 20-2500 ft
		public int CriticalWMZone { get; set; }			// 10-2500 ft		
		public int ApproachCMZone { get; set; }			// 8-800 ft
		public int InsideCMZone { get; set; }			// 6-800 ft
		public int CriticalCMZone { get; set; }			// 4-800 ft
		public int AlarmRepeat { get; set; }            // 1-90 sec


        public OperatingOptionsModel()
        {
            WorkModeEntry = 1;
            WorkModeHysteresis = 0.5;
            CrawlModeEntry = 1;
            CrawlModeHysteresis = 0.5;
            ApproachTMZone = 40;
            InsideTMZone = 30;
            CriticalTMZone = 20;
            ApproachWMZone = 30;
            InsideWMZone = 20;
            CriticalWMZone = 10;
            ApproachCMZone = 8;
            InsideCMZone = 6;
            CriticalCMZone = 4;
            AlarmRepeat = 1;
        }


        public OperatingOptionsModel(OperatingOptionsModel model)
        {
            AudioEnabled = model.AudioEnabled;
            WorkMd = model.WorkMd;
            CrawlMode = model.CrawlMode;
            CrawlModeAlarms = model.CrawlModeAlarms;
            WorkModeEntry = model.WorkModeEntry;
            WorkModeHysteresis = model.WorkModeHysteresis;
            CrawlModeEntry = model.CrawlModeEntry;
            CrawlModeHysteresis = model.CrawlModeHysteresis;
            ApproachTMZone = model.ApproachTMZone;
            InsideTMZone = model.InsideTMZone;
            CriticalTMZone = model.CriticalTMZone;
            ApproachWMZone = model.ApproachWMZone;
            InsideWMZone = model.InsideWMZone;
            CriticalWMZone = model.CriticalWMZone;
            ApproachCMZone = model.ApproachCMZone;
            InsideCMZone = model.InsideCMZone;
            CriticalCMZone = model.CriticalCMZone;
            AlarmRepeat = model.AlarmRepeat;
        }


        public string WorkModeToString(WorkMode wm)
        {
			switch (wm)
            {
                case WorkMode.Disabled:
                    return "Disabled";
                case WorkMode.EnableAlarms:
                    return "Enable Alarms";
                case WorkMode.ShowDistance:
                    return "Show Distance";
                case WorkMode.AlarmsShowDistance:
                    return "Alarms+ShowDist";
            }
            return String.Empty;
        }
    }


	public enum WorkMode
	{
		Disabled = 0,
		EnableAlarms,
		ShowDistance,
		AlarmsShowDistance
	}
}
