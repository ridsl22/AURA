using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	public static class ProtocolConst
	{
		public const byte SOP = 0xaa;

		public const ushort HeaderLen = 5;

		public const ushort HeaderOfs_SOP = 0;
		public const ushort HeaderOfs_SeqNo = 1;
		public const ushort HeaderOfs_MsgOpcode = 2;
		public const ushort HeaderOfs_PayloadLen = 3;
		public const ushort HeaderLen_PayloadLen = 2;
	}


	/// <summary>
	/// 
	/// </summary>
	/// 
	public static class TransportProtocol
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// 
		public static ushort GetMessagePacketLength(AppMessage msg)
		{
			if (msg == null)
				throw new ArgumentNullException("msg", "msg may not be null");

			return (ushort)(ProtocolConst.HeaderLen + msg.GetLength());
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <returns></returns>
		/// 
		public static ushort CalculateMessagePacketLength(byte[] buf, ushort ofs)
		{
			if (buf == null)
				throw new ArgumentNullException("buf", "buf may not be null");

			ushort payloadLen = BitConverter.ToUInt16(buf, ofs + ProtocolConst.HeaderOfs_PayloadLen);

			return (ushort)(ProtocolConst.HeaderLen + payloadLen);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// 
		public static void GetPacketHeaderInfo(byte[] buf, ushort ofs, out byte seqNo, out byte opcode)
		{
			if (buf == null)
				throw new ArgumentNullException("buf", "buf may not be null");

			seqNo = buf[ofs + ProtocolConst.HeaderOfs_SeqNo];
			opcode = buf[ofs + ProtocolConst.HeaderOfs_MsgOpcode];
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <returns></returns>
		/// 
		public static byte GetMessageOpcode(byte[] buf, ushort ofs)
		{
			if (buf == null)
				throw new ArgumentNullException("buf", "buf may not be null");

			return buf[ofs + ProtocolConst.HeaderOfs_MsgOpcode];
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="msg"></param>
		/// 
		public static void PackMessagePacket(byte seqNo, byte[] buf, AppMessage msg)
		{
			if (buf == null)
				throw new ArgumentNullException("buf", "buf may not be null");

			ushort payloadLen = msg.GetLength();

			PackHeader(buf, seqNo, (byte)msg.Opcode, payloadLen);

			msg.PackBuffer(buf, ProtocolConst.HeaderLen);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="seqNo"></param>
		/// <param name="attemptNo"></param>
		/// <param name="opcode"></param>
		/// <param name="payloadLen"></param>
		/// 
		private static void PackHeader(byte[] buf, byte seqNo, byte opcode, ushort payloadLen)
		{
			buf[ProtocolConst.HeaderOfs_SOP] = ProtocolConst.SOP;
			buf[ProtocolConst.HeaderOfs_SeqNo] = seqNo;
			buf[ProtocolConst.HeaderOfs_MsgOpcode] = opcode;
			BitConverter.GetBytes(payloadLen).CopyTo(buf, ProtocolConst.HeaderOfs_PayloadLen);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <param name="len"></param>
		/// 
		public static void ValidatePacket(byte[] buf, ushort ofs, ushort len)
		{
			if (buf == null)
				throw new ArgumentNullException("buf", "buf may not be null");

			if ((ofs + len) > buf.Length)
				throw new InvalidOperationException(string.Format("buf len ({0}) too small for ofs + len (ofs = {1}, len = {2}, total = {3}",
				  buf.Length, ofs, len, ofs + len));

			if (buf[ofs + ProtocolConst.HeaderOfs_SOP] != ProtocolConst.SOP)
				throw new InvalidOperationException(string.Format("SOP incorrect (byte {0}, val = 0x{1:x2}, expected = {2:x2})",
				  ProtocolConst.HeaderOfs_SOP, buf[ofs + ProtocolConst.HeaderOfs_SOP], ProtocolConst.SOP));

			ushort payloadLen = BitConverter.ToUInt16(buf, ofs + ProtocolConst.HeaderOfs_PayloadLen);

			ushort fullPktLen = (ushort)(ProtocolConst.HeaderLen + payloadLen);

			if (len < fullPktLen)
				throw new InvalidOperationException(string.Format("Data len ({0}) too small for packet (payload len = {1}, full packet len = {2})",
				  len, payloadLen, fullPktLen));
		}
	}
}
