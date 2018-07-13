using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MetromTablet.Models
{
	public class SecondAppDomainObject : MarshalByRefObject
	{
		Timer timer;

		//  Call this method via a proxy.  
		public void StartProcessingStuff()
		{
			timer = new Timer(tick, null, 1000, 1000);
		}

		void tick(object state)
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop timer 
			throw new Exception("New AppDomain Unhandled Exception!!", new ArgumentOutOfRangeException());
		}
	}
}
