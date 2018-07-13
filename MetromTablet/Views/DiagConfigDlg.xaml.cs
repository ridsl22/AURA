using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

using Metrom.AURA.Base;
using Metrom.AURA.Messaging;


namespace MetromTablet.Views
{


	/// <summary>
	/// Interaction logic for DiagConfigDlg.xaml
	/// </summary>
	/// 
	public partial class DiagConfigDlg : Window
	{
		#region Properties

		public CEMDiagConfig DiagConfigCEM
		{ get; private set; }

		public UIMDiagConfig DiagConfigUIM
		{ get; private set; }

		#endregion

		#region Lifetime Management

		/// <summary>
		/// 
		/// </summary>
		/// 
		public DiagConfigDlg(CEMDiagConfig diagConfigCEM, UIMDiagConfig diagConfigUIM)
		{
			if (diagConfigCEM == null)
				throw new ArgumentNullException("diagConfigCEM");

			InitializeComponent();

			DiagConfigCEM = new CEMDiagConfig(diagConfigCEM);

			if (diagConfigUIM != null)
				DiagConfigUIM = new UIMDiagConfig(diagConfigUIM);
		}

		#endregion

		#region UI Events

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// 
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			CEMDiagOption cemOptions = DiagConfigCEM.Options;  // convenience

			cbEnableAccelOutput_.IsChecked = (cemOptions & CEMDiagOption.EnableAccelCapture) != 0;
			cbEnablePeerDataOutput_.IsChecked = (cemOptions & CEMDiagOption.EnablePeerDataOutput) != 0;
			cbEnableRangeDataOutput_.IsChecked = (cemOptions & CEMDiagOption.EnableRangeDataOutput) != 0;
			cbEnableRangeDebugOutput_.IsChecked = (cemOptions & CEMDiagOption.EnableRangeDebugOutput) != 0;
			cbEnableGPSDataOutput_.IsChecked = (cemOptions & CEMDiagOption.EnableGPSDataOutput) != 0;
			cbEnableKillGPS_.IsChecked = (cemOptions & CEMDiagOption.EnableKillGPS) != 0;
			cbEnableOADataOutput_.IsChecked = (cemOptions & CEMDiagOption.EnableOADataOutput) != 0;
			cbKillBeacon_.IsChecked = (cemOptions & CEMDiagOption.KillBeacon) != 0;
			cbIgnoreBeacons_.IsChecked = (cemOptions & CEMDiagOption.IgnoreBeacons) != 0;
			cbDoNotDiscardRangeData_.IsChecked = (cemOptions & CEMDiagOption.DoNotDiscardRangeData) != 0;
			cbSendRCMScanData_.IsChecked = (cemOptions & CEMDiagOption.SendRCMScanData) != 0;
			cbExerciseGPSMath_.IsChecked = (cemOptions & CEMDiagOption.ExerciseGPSMath) != 0;

			if (DiagConfigUIM == null)
			{
				Brush disableBrush = Brushes.Gray;

				cbSendUIMDataUpdate_.IsEnabled = false;
				cbSendUIMDataUpdate_.Foreground = disableBrush;

				cbDisableNoMotionNAS_.IsEnabled = false;
				cbDisableNoMotionNAS_.Foreground = disableBrush;
			}
			else
			{
				UIMDiagOption uimOptions = DiagConfigUIM.Options;  // convenience

				cbSendUIMDataUpdate_.IsChecked = (uimOptions & UIMDiagOption.EnableDataUpdatePassThru) != 0;
				cbDisableNoMotionNAS_.IsChecked = (uimOptions & UIMDiagOption.DisableNoMotionNAS) != 0;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// 
		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			CEMDiagOption cemOptions = CEMDiagOption.None;

			if (cbEnableAccelOutput_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnableAccelCapture;
			if (cbEnablePeerDataOutput_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnablePeerDataOutput;
			if (cbEnableRangeDataOutput_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnableRangeDataOutput;
			if (cbEnableRangeDebugOutput_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnableRangeDebugOutput;
			if (cbEnableGPSDataOutput_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnableGPSDataOutput;
			if (cbEnableKillGPS_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnableKillGPS;
			if (cbEnableOADataOutput_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.EnableOADataOutput;
			if (cbKillBeacon_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.KillBeacon;
			if (cbIgnoreBeacons_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.IgnoreBeacons;
			if (cbDoNotDiscardRangeData_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.DoNotDiscardRangeData;
			if (cbSendRCMScanData_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.SendRCMScanData;  // not used yet in CEM
			if (cbExerciseGPSMath_.IsChecked ?? false)
				cemOptions |= CEMDiagOption.ExerciseGPSMath;

			DiagConfigCEM.Options = cemOptions;

			if (DiagConfigUIM != null)
			{
				UIMDiagOption uimOptions = UIMDiagOption.None;

				if (cbSendUIMDataUpdate_.IsChecked ?? false)
					uimOptions |= UIMDiagOption.EnableDataUpdatePassThru;
				if (cbDisableNoMotionNAS_.IsChecked ?? false)
					uimOptions |= UIMDiagOption.DisableNoMotionNAS;

				DiagConfigUIM.Options = uimOptions;
			}

			DialogResult = true;
		}

		#endregion

	}


}
