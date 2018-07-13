using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{
	public class Prod
	{
		public string Asset { get; set; }
		public string Time { get; set; }
		public Location Loc { get; set; }
		public string Speed { get; set; }
		public Distance Dist { get; set; }
		public TimeWrkEng TimeAcc { get; set; }
		public string Cyc { get; set; }
		public string Mode { get; set; }
		public string EngStat { get; set; }
		public string Health { get; set; }

		public Prod() 
		{
			Loc = new Location();
			Dist = new Distance();
			TimeAcc = new TimeWrkEng();
		}

		public Prod(Prod p)
		{
			Asset = p.Asset;
			Time = p.Time;
			Loc = new Location(p.Loc);
			Speed = p.Speed;
			Dist = new Distance(p.Dist);
			TimeAcc = new TimeWrkEng(p.TimeAcc);
			Cyc = p.Cyc;
			Mode = p.Mode;
			EngStat = p.EngStat;
			Health = p.Health;
		}
	}


	public class Location
	{
		public string Lat { get; set; }
		public string Lon { get; set; }

		public Location() {}

		public Location(Location l)
		{
			Lat = l.Lat;
			Lon = l.Lon;
		}
	}


	public class Distance
	{
		public string Wrk { get; set; }
		public string Trv { get; set; }

		public Distance(){}

		public Distance(Distance d)
		{
			Wrk = d.Wrk;
			Trv = d.Trv;
		}
	}


	public class TimeWrkEng
	{
		public string Wrk { get; set; }
		public string Eng { get; set; }

		public TimeWrkEng() { }

		public TimeWrkEng(TimeWrkEng t)
		{
			Wrk = t.Wrk;
			Eng = t.Eng;
		}
	}
}
