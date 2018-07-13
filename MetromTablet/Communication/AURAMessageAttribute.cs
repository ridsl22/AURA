using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class AURAMessageAttribute : Attribute
	{
		public AURAMsgOpcode Opcode
		{ get; private set; }

		public AURAMessageAttribute(AURAMsgOpcode opcode)
		{
			Opcode = opcode;
		}
	}


	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class AURAMessageHandlerAttribute : Attribute
	{
	}
}
