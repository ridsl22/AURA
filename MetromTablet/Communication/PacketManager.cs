using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	public abstract class PacketManager
	{
		#region Instance Fields

		/// <summary>
		/// SOP byte 1. If SOP len is 1, this is the SOP; if SOP len is 2, this is the first byte.
		/// </summary>
		private byte sop1_;

		/// <summary>
		/// SOP byte 2. Only used if SOP len is 2.
		/// </summary>
		private byte sop2_;

		/// <summary>
		/// Length of the SOP in bytes.
		/// </summary>
		private byte sopLen_;

		/// <summary>
		/// 
		/// </summary>
		private uint bytesNeededForPacketLenField_;

		/// <summary>
		/// The accumulator buffer, holding saved data during partial packet reads.
		/// </summary>
		private byte[] inBuf_;

		/// <summary>
		/// The number of bytes in the accumulator buffer.
		/// </summary>
		private uint bytesIn_;

		/// <summary>
		/// If SOP len is 2, indicates that the first SOP byte has been read.
		/// </summary>
		private bool inSOP_;

		/// <summary>
		/// Indicates whether we are currently inside a packet (true) or not (false).
		/// </summary>
		private bool inPacket_;

		/// <summary>
		/// The index in the newData or accumulator buffer at which the current packet starts
		/// (only valid when inPacket_ is true); does not include any fixed offset which may
		/// be in force.
		/// </summary>
		private uint packetStart_;

		/// <summary>
		/// The number of bytes available in the accumulator buffer for a current partial
		/// packet (processed during the last NewData() call).
		/// </summary>
		private uint bytesAvailable_;

		/// <summary>
		/// The number of bytes already processed in the accumulator buffer from the last
		/// NewData() call.
		/// </summary>
		private uint partialPacketLen_;

		/// <summary>
		/// The full length of the current packet (only valid when inPacket_ is true), or
		/// zero if the packet length byte(s) have not been read yet.
		/// </summary>
		private uint fullPacketLen_;

		/// <summary>
		/// The maximum length of a packet.
		/// </summary>
		private uint maxPacketLen_;

		/// <summary>
		/// 
		/// </summary>
		private bool pleaseReset_;

		#endregion

		#region Events

		public event Action<uint> DiscardedBytes;

		#endregion

		#region Lifetime Management

		/// <summary>
		/// 
		/// </summary>
		/// 
		public PacketManager(byte sop, uint byteCountForPacketLen, uint maxPacketLen)
		{
			sop1_ = sop;
			sop2_ = 0;
			sopLen_ = 1;

			bytesNeededForPacketLenField_ = byteCountForPacketLen;
			maxPacketLen_ = maxPacketLen;

			CommonInit();
		}


		public PacketManager(byte sop1, byte sop2, uint byteCountForPacketLen, uint maxPacketLen)
		{
			sop1_ = sop1;
			sop2_ = sop2;
			sopLen_ = 2;

			bytesNeededForPacketLenField_ = byteCountForPacketLen;
			maxPacketLen_ = maxPacketLen;

			CommonInit();
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		private void CommonInit()
		{
			inBuf_ = new byte[1024];

			bytesIn_ = 0;
			inSOP_ = false;
			inPacket_ = false;
			packetStart_ = 0;
			bytesAvailable_ = 0;
			partialPacketLen_ = 0;
			fullPacketLen_ = 0;
			pleaseReset_ = false;
		}

		#endregion

		#region Operations

		/// <summary>
		/// 
		/// </summary>
		/// 
		public void Reset()
		{
			pleaseReset_ = true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="newData"></param>
		/// <param name="newDataOfs"></param>
		/// <param name="newDataLen"></param>
		/// 
		public void NewData(byte[] newData, uint newDataOfs, uint newDataLen)
		{
			// Sanity.

			if (newData == null)
				throw new ArgumentNullException("newData", "newData may not be null");
			if (newDataLen == 0)
				throw new ArgumentException("newDataLen may not be zero", "newDataLen");
			if ((newDataOfs + newDataLen) > newData.Length)
				throw new InvalidOperationException("New data offset plus length overruns the supplied buffer");

			if (pleaseReset_)
				InternalReset();

			uint discardedBytes = 0;

#if VERBOSE
      Debug.WriteLine("--PM: NewData: ofs {0}, len {1}:", newDataOfs, newDataLen);
      Debug.WriteLine(MemDebug.DumpMem(newData, newDataLen, newDataOfs, prefix: "--PM: ").ToString());
#endif

			// srcBuf:  The source data buffer to use.  To genericize the below processing code, we use a
			// single reference to a buffer which may be the incoming newData buffer (if there was not partial
			// data waiting for us when the NewData() call was made) or the inBuf_ buffer (if there was partial
			// data waiting when the call was made).
			byte[] srcBuf;

			// srcOfs:  The base (fixed) offset in the srcBuf at which valid data starts.  If we are processing
			// data from the newData buffer, calling code may supply a nonzero newDataOfs which we must use; if
			// we're processing data from inBuf_, the base offset will be zero because the buffer is under our
			// control.  This is another variable used to genericize the below processing code.
			uint srcOfs;

			// srcNdx:  The relative index of the current byte being examined during the below loop; used
			// to track discarded non-packet data bytes while looking for the SOP marker when not in a
			// packet.
			uint srcNdx = 0;

			// If we are currently in the middle of a packet, that means that a previous NewData() call found
			// partial packet data, in which case the data in that call's newData buffer had to be copied to
			// the accumulator buffer to save it until the next call.  In this case, we append the incoming
			// new data onto the existing data in the accumulator buffer.

			if (inPacket_ || inSOP_)
			{
#if VERBOSE
        Debug.WriteLine("--PM: Already in packet at beginning of call; srcBuf will be accumulator buffer.");
#endif

				// Resize the accumulator buffer if necessary.

				if (inBuf_.Length < (bytesIn_ + newDataLen))
				{
					byte[] resizeInBuf = new byte[bytesIn_ + newDataLen + 128];
					inBuf_.CopyTo(resizeInBuf, 0);
					inBuf_ = resizeInBuf;
				}

				// Append incoming data onto the end of existing data in the accumulator buffer.

				Array.Copy(newData, newDataOfs, inBuf_, bytesIn_, newDataLen);

				// Set the source buffer and offset values (offset is zero when we're working with the
				// accumulator buffer).

				srcBuf = inBuf_;
				srcOfs = 0;
				srcNdx = bytesIn_;

				// Adjust the count of bytes currently in the accumulator, and also adjust the count of
				// bytes available for packet processing (tracked in a separate variable in order to make
				// the below loop generic).

				bytesIn_ += newDataLen;
				bytesAvailable_ += newDataLen;

#if VERBOSE
        Debug.WriteLine("--PM: Accumulator buffer now contains {0} bytes:", bytesIn_);
        Debug.WriteLine(MemDebug.DumpMem(inBuf_, bytesIn_, 0, prefix: "--PM: ").ToString());
#endif
			}
			else
			{
#if VERBOSE
        Debug.WriteLine("--PM: Not in packet at beginning of call; srcBuf will be new data buffer.");
#endif

				// We're not currently in a packet, so we will attempt to process as much data as possible
				// directly out of the supplied newData buffer; set the source buffer and offset values.

				srcBuf = newData;
				srcOfs = newDataOfs;
				srcNdx = 0;

				// Set the count of bytes available for packet processing - in this case, it will be the
				// same as the number of new data bytes.

				bytesAvailable_ = newDataLen;
			}

#if VERBOSE
      Debug.WriteLine("--PM: Processing now starts with index {0}, offset {1} in buffer.", srcNdx, srcOfs);

      if (inSOP_)
        Debug.WriteLine("--PM: Note that coming into the call, we're looking for SOP 2.");
      if (inPacket_)
        Debug.WriteLine("--PM: Note that coming into the call, we're in the middle of a packet.");
#endif

			// bytesLeft:  The count of bytes left to examine during the current call.
			uint bytesLeft = newDataLen;

			// Process all new bytes, dispatching whole messages as discovered.

			bool bailAndCopyPartialData = false;

			do
			{
				// If we're not in the middle of a packet, scan for the SOP; non-packet data
				// bytes outside of a packet and before the SOP will be discarded.

				if (!inPacket_)
				{
#if VERBOSE
          if (sopLen_ == 1)
            Debug.WriteLine("--PM: Scanning for SOP...");
          else
          {
            if (inSOP_)
              Debug.WriteLine("--PM: Scanning for SOP 2...");
            else
              Debug.WriteLine("--PM: Scanning for SOP 1...");
          }
#endif

					// Look for the SOP starting with the current value of srcNdx, and increment
					// it as non-packet data (i.e., non-SOP bytes) are found.

					do
					{
						if (sopLen_ == 1)
						{
							if (srcBuf[srcOfs + srcNdx] == sop1_)
							{
								// We've found the SOP.  Change our inPacket_ state and record the start
								// index for the packet (the index of the SOP).

								inPacket_ = true;
								packetStart_ = srcNdx;
								break;
							}
							else
								++discardedBytes;
						}
						else
						{
							if (inSOP_)
							{
								if (srcBuf[srcOfs + srcNdx] == sop2_)
								{
									// We've found SOP 2. Change our inPacket_ state and continue. Note that if
									// we're here (inSOP_ == true), a previous call found SOP 1 but there was
									// not enough data to look for SOP 2; therefore, the "bail and copy partial
									// data" block was executed, which sets the value of packetStart_.

									inSOP_ = false;
									inPacket_ = true;
									break;
								}
								else
									inSOP_ = false;
							}
							else
							{
								if (srcBuf[srcOfs + srcNdx] == sop1_)
								{
									// We've found SOP 1. Note it and check for SOP 2.

									packetStart_ = srcNdx;

#if VERBOSE
                  Debug.WriteLine("--PM: Checking for SOP 2...");
#endif

									// If there are no more bytes to examine, however, copy the single SOP
									// in preparation for the next batch of data.

									if (bytesLeft == 1)
									{
#if VERBOSE
                    Debug.WriteLine("--PM: ...not enough bytes left to check for SOP 2; bailing and checking next time.");
#endif
										inSOP_ = true;
										bailAndCopyPartialData = true;
										break;
									}
									else if (srcBuf[srcOfs + srcNdx + 1] == sop2_)
									{
#if VERBOSE
                    Debug.WriteLine("--PM: SOP 2 present, now in a packet.");
#endif
										inSOP_ = false;
										inPacket_ = true;
										break;
									}
									else
										++discardedBytes;
								}
								else
									++discardedBytes;
							}
						}

						// The current byte wasn't the SOP or, if 2-byte SOP, wasn't SOP 1; move to the next
						// byte and also decrement both the local bytesLeft and global bytesAvailable_ counters.

						++srcNdx;
						--bytesLeft;
						--bytesAvailable_;

					} while (bytesLeft > 0);

					if (bailAndCopyPartialData)
						continue;

					// If we didn't find the SOP, reset accumulator data and bail.

#if VERBOSE
          if (!inPacket_)
            Debug.WriteLine("--PM: No SOP found; discarding all data as noise.");
          else
          {
            if (sopLen_ == 1)
              Debug.WriteLine("--PM: SOP found, index {0}; bytesAvailable_ = {1}, bytesLeft = {2}.", packetStart_, bytesAvailable_, bytesLeft);
            else
              Debug.WriteLine("--PM: SOP found, index {0} and {1}; bytesAvailable_ = {2}, bytesLeft = {3}.", packetStart_, packetStart_ + 1, bytesAvailable_, bytesLeft);
          }
#endif

					if (!inPacket_)
						break;
				}

				// At this point:
				//   - inPacket_ == true;
				//   - packetStart_ is the offset-relative index in srcBuf containing the SOP;
				//   - bytesAvailable_ is the count of bytes available from packetStart_ through
				//       the end of data in srcBuf;
				//   - bytesLeft is the count of bytes left to examine during this loop.

				// We're in the middle of packet data (either we just found an SOP in this loop,
				// or we've been waiting for more data to complete a partial packet).

				// If we've not already calculated the full packet length (because we haven't already
				// read enough header data to get to the payload length and therefore can't calculate
				// it), see if we can do so now.

				if (fullPacketLen_ == 0)
				{
#if VERBOSE
          Debug.WriteLine("--PM: No packet len yet...");
#endif

					// If we don't have enough data yet to have read the packet payload length: either
					// append the new data onto the accumulator buffer (if we've been looking at the new
					// data buffer); or, if the current start of packet isn't already at the head of the
					// accumulator, copy data in the accumulator from the current start of packet to the
					// head; then bail.

					if (bytesAvailable_ < bytesNeededForPacketLenField_)
					{
#if VERBOSE
            Debug.WriteLine("--PM: Not enough bytes for len field (bytesAvailable_ = {0}, bytes needed = {1}).", bytesAvailable_, bytesNeededForPacketLenField_);
#endif

						bailAndCopyPartialData = true;
						break;
					}

					// We now have enough data to pick out the payload length, so calculate the full
					// packet length.

					fullPacketLen_ = CalculateFullPacketLength(srcBuf, srcOfs + packetStart_);

#if VERBOSE
          Debug.WriteLine("--PM: Enough bytes for len field; full packet len = {0}.", fullPacketLen_);
#endif
				}

				if (fullPacketLen_ <= maxPacketLen_)
				{
					// At this point, we now know the full packet length, in fullPacketLen_.

					// If there isn't yet a full packet's worth of data, save / justify data (as above when
					// we didn't have enough data to read the payload length), then bail.

					if (bytesAvailable_ < fullPacketLen_)
					{
#if VERBOSE
            Debug.WriteLine("--PM: Not enough bytes for full packet.");
#endif

						bailAndCopyPartialData = true;
						break;
					}

					// At this point, we have at least enough data for a full packet, so process it!

#if VERBOSE
          Debug.WriteLine("--PM: Got a packet - processing! Index {0}, length {1}", srcOfs + packetStart_, fullPacketLen_);
#endif

					ProcessPacket(srcBuf, srcOfs + packetStart_, fullPacketLen_);

					// Adjust tracking variables in preparation for the next loop.

					inPacket_ = false;

					// bytesLeft must be reduced by the number of in-packet bytes that were added in
					// this call to NewData().  partialPacketLen_ contains the number of in-packet
					// bytes that were present before this call, so we subtract this value from
					// fullPacketLen_ to get the bytes added in this call.

					bytesLeft -= fullPacketLen_ - partialPacketLen_;

					// The number of bytes available for further processing is now the same as bytesLeft.

					bytesAvailable_ = bytesLeft;

#if VERBOSE
          Debug.WriteLine("--PM: After processing packet: bytesAvailable_ = {0}, bytesLeft = {1}.", bytesAvailable_, bytesLeft);
#endif

					// The SOP scanning index must be incremented by the full packet length.

					srcNdx = packetStart_ + fullPacketLen_;

					fullPacketLen_ = 0;
					partialPacketLen_ = 0;

#if VERBOSE
          Debug.WriteLine("--PM: and srcNdx now = {0}", srcNdx);
#endif
				}
				else
				{
#if VERBOSE
          Debug.WriteLine("--PM: ********** Packet len ({0}) > max packet len ({1})!  SOP may have been bogus; resetting to scan for another packet! **********", fullPacketLen_, maxPacketLen_);
#endif

					inPacket_ = false;
					bytesLeft = 0;  // for now, we discard the rest of the data in our buffers
				}

			} while ((bytesLeft > 0) && !bailAndCopyPartialData);

			// If we've exited the loop because of partial data, copy new data into or left justify
			// data in the accumulator buffer; otherwise, reset tracking variables for the next call.

			if (bailAndCopyPartialData)
			{
				if ((srcBuf == newData) || (packetStart_ > 0))
				{
					// Resize the accumulator buffer if necessary.

					if (bytesLeft > inBuf_.Length)
					{
						byte[] resizeInBuf = new byte[bytesLeft + 128];
						inBuf_.CopyTo(resizeInBuf, 0);
						inBuf_ = resizeInBuf;
					}

					Array.Copy(srcBuf, srcOfs + packetStart_, inBuf_, 0, bytesLeft);
					bytesIn_ = bytesLeft;
					packetStart_ = 0;
				}

				partialPacketLen_ += bytesLeft;

#if VERBOSE
        Debug.WriteLine("--PM: Bailing, copied partial data into accumulator (partialPacketLen_ = {0}, bytesIn_ = {1}).", partialPacketLen_, bytesIn_);
#endif
			}
			else
			{
				// We've exhausted all available data with no partial data hanging around, so reset
				// all tracking variables.

				InternalReset();
			}

			if ((discardedBytes != 0) && (DiscardedBytes != null))
				DiscardedBytes(discardedBytes);
		}

		#endregion

		#region Abstract Implementation Methods

		/// <summary>
		/// Called from NewData() when a full packet has been read.
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <param name="len"></param>
		/// 
		protected abstract void ProcessPacket(byte[] buf, uint ofs, uint len);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="pktOfs"></param>
		/// <returns></returns>
		/// 
		protected abstract uint CalculateFullPacketLength(byte[] buf, uint pktOfs);

		#endregion

		#region Implementation

		/// <summary>
		/// 
		/// </summary>
		/// 
		public void InternalReset()
		{
			bytesIn_ = 0;
			fullPacketLen_ = 0;
			packetStart_ = 0;
			bytesAvailable_ = 0;
			partialPacketLen_ = 0;

			pleaseReset_ = false;
		}

		#endregion
	}
}
