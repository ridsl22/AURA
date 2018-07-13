using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MetromTablet.Communication
{
	public class AURAMessageFactory
	{
		#region Types

		/// <summary>
		/// 
		/// </summary>
		/// 
		private abstract class MsgInfo
		{
			private ConstructorInfo ctorInfo_ = null;

			public AURAMsgOpcode Opcode
			{ get; private set; }

			public MsgInfo(AURAMsgOpcode opcode, ConstructorInfo ci)
			{
				Opcode = opcode;
				ctorInfo_ = ci;
			}

			public void ReconstituteAndDispatchMessage(byte[] buf, ushort ofs, ushort len, object state)
			{
				ushort payloadOfs = (ushort)(ofs + ProtocolConst.HeaderLen);
				ushort payloadLen = (ushort)(len - ProtocolConst.HeaderLen);

				AURAAppMessage msg = (AURAAppMessage)ctorInfo_.Invoke(new object[] { buf, payloadOfs, payloadLen });

				InvokeHandler(msg, state);
			}

			protected abstract void InvokeHandler(AURAAppMessage msg, object state);
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		private class MsgInfo1ParamHandler : MsgInfo
		{
			private Action<AURAAppMessage> msgHandler_;

			public MsgInfo1ParamHandler(AURAMsgOpcode opcode, ConstructorInfo ci, Action<AURAAppMessage> handler)
				: base(opcode, ci)
			{
				msgHandler_ = handler;
			}

			protected override void InvokeHandler(AURAAppMessage msg, object state)
			{
				msgHandler_(msg);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// 
		private class MsgInfo2ParamHandler : MsgInfo
		{
			private Action<AURAAppMessage, object> msgHandler_;

			public MsgInfo2ParamHandler(AURAMsgOpcode opcode, ConstructorInfo ci, Action<AURAAppMessage, object> handler)
				: base(opcode, ci)
			{
				msgHandler_ = handler;
			}

			protected override void InvokeHandler(AURAAppMessage msg, object state)
			{
				msgHandler_(msg, state);
			}
		}

		#endregion

		#region Instance Fields

		/// <summary>
		/// 
		/// </summary>
		/// 
		private Dictionary<AURAMsgOpcode, MsgInfo> msgHandlerMap_ = new Dictionary<AURAMsgOpcode, MsgInfo>();

		#endregion

		#region Lifetime Management

		/// <summary>
		/// 
		/// </summary>
		/// 
		public AURAMessageFactory(Dispatcher dispatcher, object handlerClass)
		{
			// Temporary map used while discovering message classes (that have been decorated with
			// the AURAMessage attribute).
			Dictionary<Type, Tuple<AURAMsgOpcode, ConstructorInfo>> tempMap = new Dictionary<Type, Tuple<AURAMsgOpcode, ConstructorInfo>>();
			HashSet<AURAMsgOpcode> tempSet = new HashSet<AURAMsgOpcode>();

			// Scan our assembly to discover all message classes that are subclasses of AURAAppMessage
			// and that have been decorated with the AURAMessage attribute.

			Assembly ourAssembly = Assembly.GetExecutingAssembly();

			// Look for classes that are subclasses of AURAAppMessage.

			foreach (Type type in ourAssembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(AURAAppMessage)))
				{
					object[] attrs = type.GetCustomAttributes(typeof(AURAMessageAttribute), false);

					if ((attrs == null) || (attrs.Length == 0))
						throw new ApplicationException(string.Format("AURAMessageFactory: message {0} has no AURAMessage attribute", type.Name));
					else if (attrs.Length > 1)
						throw new ApplicationException(string.Format("AURAMessageFactory: message {0} has multiple AURAMessage attributes (how can that be?)", type.Name));
					else
					{
						AURAMessageAttribute msgAttr = (AURAMessageAttribute)attrs[0];

						if (tempSet.Contains(msgAttr.Opcode))
							throw new ApplicationException(string.Format("AURAMessageFactory: duplicate message opcode on message {0}", type.Name));
						else
						{
							tempSet.Add(msgAttr.Opcode);

							// Incoming messages must define a constructor with the signature:
							// ctor(byte[], ushort, ushort).

							ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(byte[]), typeof(ushort), typeof(ushort) });

							// If such a ctor is defined, stash away the ConstructorInfo object for later use.

							if (ci != null)
								tempMap.Add(type, new Tuple<AURAMsgOpcode, ConstructorInfo>(msgAttr.Opcode, ci));
						}
					}
				}
			}

			// Now we know all (decorated) message classes; scan the supplied handler class (typically
			// the main application class) looking for methods decorated with the AURAMessageHandler
			// attribute, and create and record a delegate for any such message handler if the
			// associated message type is known (discovered in the loop above).

			foreach (MethodInfo method in handlerClass.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				object[] attrs = method.GetCustomAttributes(typeof(AURAMessageHandlerAttribute), false);

				if ((attrs != null) && (attrs.Length == 1))
				{
					ParameterInfo[] pis = method.GetParameters();

					// Handler must match one of two signatures:
					// - one parameter, of a class type derived from AURAAppMessage and decorated with an
					//   AURAMessage attribute (and thus already present in our temporary map); or
					// - two parameters, one as above and a second of type object.

					if ((pis == null) || ((pis.Length != 1) && (pis.Length != 2)))
						throw new ApplicationException(string.Format("AURAMessageFactory: handler {0} must take one param of an AURAAppMessage-derived type and optionally a second of type object", method.Name));
					else
					{
						if ((pis.Length == 2) && (pis[1].ParameterType != typeof(object)))
							throw new ApplicationException(string.Format("AURAMessageFactory: handler {0} second param must be of type object", method.Name));

						Tuple<AURAMsgOpcode, ConstructorInfo> tempData;

						if (!tempMap.TryGetValue(pis[0].ParameterType, out tempData))
							throw new ApplicationException(string.Format("AURAMessageFactory: handler {0} first parame must be an AURAAppMessage-derived type", method.Name));

						if (msgHandlerMap_.ContainsKey(tempData.Item1))
							throw new ApplicationException(string.Format("AURAMessageFactory: duplicate handler for {0}: {1}", tempData.Item1, method.Name));
						else
						{
							MethodInfo curMethod = method;  // don't use var method in the lambda closure...

							MsgInfo msgInfo;

							if (pis.Length == 1)
							{
								msgInfo = new MsgInfo1ParamHandler(tempData.Item1, tempData.Item2, (msg) =>
								{
									dispatcher.BeginInvoke((Action)(() => { curMethod.Invoke(handlerClass, new object[] { msg }); }));
								});
							}
							else
							{
								msgInfo = new MsgInfo2ParamHandler(tempData.Item1, tempData.Item2, (msg, st) =>
								{
									dispatcher.BeginInvoke((Action)(() => { curMethod.Invoke(handlerClass, new object[] { msg, st }); }));
								});
							}

							msgHandlerMap_.Add(tempData.Item1, msgInfo);
						}
					}
				}
			}
		}

		#endregion

		#region Operations

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="ofs"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		/// 
		public void DispatchMessage(byte[] buf, ushort ofs, ushort len, object state = null)
		{
			if (buf == null)
				throw new ArgumentNullException("buf");
			if (len == 0)  // actually, len should be >= header size...
				throw new ArgumentException("len", "len must be nonzero");

			byte rawOpcode = TransportProtocol.GetMessageOpcode(buf, ofs);

			if (!Enum.IsDefined(typeof(AURAMsgOpcode), rawOpcode))
				throw new InvalidOperationException(string.Format("MessageFactory.DispatchMessage(): opcode 0x{0:x2} unknown", rawOpcode));

			AURAMsgOpcode opcode = (AURAMsgOpcode)rawOpcode;

			MsgInfo msgInfo = null;

			if (!msgHandlerMap_.TryGetValue(opcode, out msgInfo))
			{
				throw new InvalidOperationException(string.Format("MessageFactory.DispatchMessage(): no handler registered for {0}", opcode));
			}
			
			msgInfo.ReconstituteAndDispatchMessage(buf, ofs, len, state);
		}

		#endregion

	}


}
