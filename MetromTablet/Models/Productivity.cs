using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{
	public class Productivity
	{
		public Prod prod { get; set; }

		public Productivity()
		{
			prod = new Prod();
		}

		public Productivity(Productivity p)
		{
			prod = new Prod();
			prod = p.prod;
		}
	}
}
