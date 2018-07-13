using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	class Constants
	{
	}


	public enum SeparationSource
	{
		Unknown = 0,
		RCM = 1,
		GPS = 2
	}


	public enum AppCommState
	{
		NotConnected,
		Indeterminate,
		DemoMode,
		Connecting,
		Connected
	}


	public static class UICmdResult
	{
		public const byte Success = 0;
		public const byte Failed = 1;
		public const byte CmdInProgress = 2;
		public const byte CmdNotValid = 3;
		public const byte Timeout = 4;
	}
}
