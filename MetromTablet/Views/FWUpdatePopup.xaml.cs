using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MetromTablet.Communication;
using Microsoft.Win32;

namespace MetromTablet.Views
{
	/// <summary>
	/// Interaction logic for FWUpdatePopup.xaml
	/// </summary>
	public partial class FWUpdatePopup : Window
	{
		public bool PM;
		public string FileName { get; set; }
		private byte outSeqNo_ = 0;
		private byte[] outBuf_ = new byte[256];
		private bool updateRunning;

		const uint MF_BYCOMMAND = 0x00000000;
		const uint MF_GRAYED = 0x00000001;
		const uint MF_ENABLED = 0x00000000;
		const uint SC_CLOSE = 0xF060;
		const int WM_SHOWWINDOW = 0x00000018;
		const int WM_CLOSE = 0x10;

		//[DllImport("user32.dll")]
		//static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
		//[DllImport("user32.dll")]
		//static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);


		public double ProgressPercent
		{
			get { return pbFWUpdate.Value; }
			set { pbFWUpdate.Value = value; }
		}


		public FWUpdatePopup(bool _pm)
		{
			InitializeComponent();
			this.Owner = MainWindow.mainWindow;
			PM = _pm;
			Title = PM ? "Update PM Firmware" : "Update CEM Firmware";
		}


		private void BrowseFirmware()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (PM)
				dlg.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
			else
				dlg.Filter = "CEM Image|*.bin";

			if (dlg.ShowDialog() == true)
			{
				FileName = dlg.FileName.Trim();
				if (FileName.EndsWith(".bin"))
					btnUpdate.IsEnabled = true;
				MetromRailPage.fwUpdateData_ = File.ReadAllBytes(FileName);
				MetromRailPage.fwUpdateSize_ = (uint)MetromRailPage.fwUpdateData_.Length;
			}
			tbFWUpdate.Text = FileName;
		}


		public void Update(bool ota)
		{
			tbFWUpdate.Text = FileName;
			if (PM)
			{
				if (ota)
				{
					MetromRailPage.fwUpdateData_ = File.ReadAllBytes(FileName);
					MetromRailPage.fwUpdateSize_ = (uint)MetromRailPage.fwUpdateData_.Length;
				}
				uint dataSize = MetromRailPage.fwUpdateSize_ + (64 - (MetromRailPage.fwUpdateSize_ % 64));
				SendMgmtMsg(new Msg_UILogInfo(LongData.Binary, 64, dataSize, CalculateCRC(MetromRailPage.fwUpdateData_)));
			}
			else
			{
				SendMgmtMsg(new Msg_UICEMReflashInit());

			}

			btnUpdate.IsEnabled = false;
			btnBrowse.IsEnabled = false;
			updateRunning = true;

			//var hwnd = new WindowInteropHelper(this).Handle;
			//IntPtr hMenu = GetSystemMenu(hwnd, false);

			//if (hMenu != IntPtr.Zero)
			//{
			//	EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
			//}
		}


		public void SendMgmtMsg(AppMessage msg)
		{
			try
			{
				ushort msgFullLen = TransportProtocol.GetMessagePacketLength(msg);
				TransportProtocol.PackMessagePacket(++outSeqNo_, outBuf_, msg);
				MetromRailPage.serialPort.Write(outBuf_, 0, msgFullLen);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


		public void UpdateDone(bool hasCompleted)
		{
			updateRunning = false;
			//if (!PM)
			//{
			MessageBox.Show("Firmware update completed!");
			//}
			//else
			//{
			//	MessageBox.Show("Please wait for PM reboot");
			//}
			this.Close();
		}


		public byte[] CalculateCRC(byte[] data)
		{
			CRC32 crc32 = new CRC32();
			//String hash = String.Empty;
			byte[] crc = new byte[4];

			
			
			int remainingZeros = 64 - ((int)MetromRailPage.fwUpdateSize_ % 64);
			byte[] toCalc = new byte[MetromRailPage.fwUpdateSize_ + remainingZeros];
			//toCalc = data;
			for(int j = 0; j < MetromRailPage.fwUpdateSize_; j++)
			{
 				toCalc[j] = data[j];
			}
			for (int j = (int)MetromRailPage.fwUpdateSize_; j < MetromRailPage.fwUpdateSize_ + remainingZeros; j++)
			{
				toCalc[j] = 0;
			}
			int i = 0;
			foreach (byte b in crc32.ComputeHash(toCalc))
			{
				crc[i] = b;
				i++;
				//hash += b.ToString("x2").ToLower();
			}
			//MessageBox.Show(hash);
			return crc;
		}


		private void btnBrowse_TouchDown(object sender, TouchEventArgs e)
		{
			BrowseFirmware();
		}


		private void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			Update(false);
		}


		private void btnUpdate_TouchDown(object sender, TouchEventArgs e)
		{
			Update(false);
		}


		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			pbFWUpdate.Value = e.ProgressPercentage;
			if (pbFWUpdate.Value == 100)
			{
				MessageBox.Show("Firmware update completed!");
				updateRunning = false;
				this.Close();
			}
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			BrowseFirmware();
		}
	}
}
