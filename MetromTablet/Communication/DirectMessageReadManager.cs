using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	public class DirectMessageReadManager : PacketManager
	{
        public const ushort kMaxPacketLen = 256;//128;


		#region Events

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <param name="len"></param>
		/// 
		public delegate void NewPacketHandler(byte[] buf, ushort ofs, ushort len);

		/// <summary>
		/// 
		/// </summary>-
		/// 
		public event NewPacketHandler NewPacket;

		#endregion

		#region Lifetime Management

		/// <summary>
		/// 
		/// </summary>
		/// 
		public DirectMessageReadManager()
			: base(ProtocolConst.SOP, ProtocolConst.HeaderOfs_PayloadLen + ProtocolConst.HeaderLen_PayloadLen, kMaxPacketLen)
		{
		}

		#endregion

		#region PacketManager Overrides

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <param name="len"></param>
		/// 
		protected override void ProcessPacket(byte[] buf, uint ofs, uint len)
		{
			if (NewPacket != null)
				NewPacket(buf, (ushort)ofs, (ushort)len);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="pktOfs"></param>
		/// <returns></returns>
		/// 
		protected override uint CalculateFullPacketLength(byte[] buf, uint pktOfs)
		{
			return TransportProtocol.CalculateMessagePacketLength(buf, (ushort)pktOfs);
		}

		#endregion
	}
}
