using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{
	public class AppSettings
	{
		public int IdleIntervalSec { get; set; }


		public AppSettings() { }


		public AppSettings(AppSettings s)
		{
			IdleIntervalSec = s.IdleIntervalSec;
		}
	}
}
