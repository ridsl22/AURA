using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{
	public class CommissioningSettingsModel
	{

		public string DevName { get; set; }
		public int FrontOffset { get; set; } // 0-250
		public int RearOffset { get; set; } // 0-250


        public CommissioningSettingsModel() { }


        public CommissioningSettingsModel(CommissioningSettingsModel model)
        {
            DevName = model.DevName;
            FrontOffset = model.FrontOffset;
            RearOffset = model.RearOffset;
        }
	}
}
