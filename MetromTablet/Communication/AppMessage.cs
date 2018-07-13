using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	public abstract class AppMessage
	{
		#region Properties

		/// <summary>
		/// 
		/// </summary>
		///
		public byte Opcode
		{ get; protected set; }

		#endregion

		#region Lifetime Management

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="opcode"></param>
		/// 
		public AppMessage(byte opcode)
		{
			Opcode = opcode;
		}

		#endregion

		#region Marshaling Methods

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// 
		public virtual ushort GetLength()
		{
			return 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// 
		public virtual void PackBuffer(byte[] buf, ushort ofs)
		{
		}

		#endregion
	}
}
