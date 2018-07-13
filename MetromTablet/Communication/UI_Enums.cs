using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Communication
{
	public class UI_Enums
	{
		public enum UIA_Primitive
		{
			// UI events
			UIA_UI_TIMER_EXPIRED,//0

			// Button Interface
			UIA_BTN_DOWN_PRESSED,//1
			UIA_BTN_UP_PRESSED,//2
			UIA_BTN_LEFT_PRESSED,//3
			UIA_BTN_RIGHT_PRESSED,//4
			UIA_BTN_CONFIRM_PRESSED,//5
			UIA_BTN_CANCEL_PRESSED,//6
			UIA_BTN_LEFT_DOUBLE_DOWN_PRESSED,//7

			// Vacuume Dot Matrix interface:
			UIA_FPVFD_DefineCustomGlyph,//8
			UIA_FPVFD_DisplayControl,//9
			UIA_FPVFD_ClearDisplay,//10
			UIA_FPVFD_SetCursor,//11
			UIA_FPVFD_WriteData,//12
			UIA_FPVFD_WriteString,//13
			UIA_FPVFD_WriteDisplayFullLine,//14

			// Seven Segment Display
			UIA_SSD_SetDecimalPt,//15
			UIA_SSD_SetWarningLED,//16
			UIA_SSD_SetUINT16,//17
			UIA_SSD_DisplaySpecial,//18

			// Audio & Warning LED
			UIA_AUDL_SetAlarmAckRepeatTime,//19
			UIA_AUDL_SetVolumeLevel,//20
			UIA_AUDL_UpdatePeerOnly,//21
			UIA_AUDL_PlayNotification,//22
			UIA_AUDL_SilenceMode//23
		};


		public enum WhichPeer
		{
			PEER1,    // nominally front peer
			PEER2,        // nominally rear peer
			PEER_Unknown  // both peers, or for situations where front/back is not known
		};


		public enum SSD_DisplaySpecialCodes
		{
			SSD_DISPLAY_BLANK_ALL,
			SSD_DISPLAY_SHOW_ALL,
			SSD_DISPLAY_NO_PEER,
			SSD_DISPLAY_ERROR1
		};


		public enum AudioLedNotifications
		{
			AUDL_STARTUP,
			AUDL_SERVICE_NEEDED,
			AUDL_ALERT,
			AUDL_ALERT_NO_AUDIO,
			AUDL_ALARM,
			AUDL_ALARM_ACKNOWLEDGED_GPS,
			AUDL_ALARM_ACKNOWLEDGED_NOGPS,
			// keep the following at the end, not part of the notification description table
			AUDL_OFF,
			AUDL_NONE
		};
	}
}
