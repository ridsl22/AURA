using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using IDI.Scientific;
using Metrom.AURA.Base;
using Metrom.AURA.Log;
using MetromTablet.Communication;
using MetromTablet.Models;
using Newtonsoft.Json;
using System.Windows.Documents;
using MetromTablet.Views;

namespace MetromTablet
{
	public partial class MetromRailPage : Page
	{
		#region AURA Message Handlers

		[AURAMessageHandler]
		private void HandleUIMgmtTextMsg(Msg_UIMgmtText msg)
		{
			isDeviceConnected = true;
			hiddenMenuUnlocked = false;
		}


		[AURAMessageHandler]
		private void HandleUICEMReadyMsg(Msg_UICEMReady msg)
		{
			isDeviceConnected = true;
			SendMgmtMsg(new Msg_UICEMReadyAck());
			SendMgmtMsg(new Msg_UICEMUIReady());
			SendMgmtMsg(new Msg_CEMHello());
		}


        [AURAMessageHandler]
        private void HandleUIGPSUpdateMsg(Msg_UIGPSUpdate msg)
        {
            latitude = msg.LocalLat;
            longitude = msg.LocalLng;
            gpsUpdate = new Msg_UIGPSUpdate(msg);
        }


        [AURAMessageHandler]
		private void HandleUICEMConfigInfoADUMsg(Msg_UICEMConfigInfoADU msg)
		{
			isDeviceConnected = true;
			//cemConfig = msg.Config;
			//try
			//{
			//	cemConfig.NamePrefix = machineConfig.MachineName.Substring(0, machineConfig.MachineName.IndexOf(" "));
			//	cemConfig.NameNumber = Convert.ToUInt32(machineConfig.MachineName.Substring(machineConfig.MachineName.LastIndexOf(" ") + 1));
			//}
			//catch { }
			//SendMgmtMsg(new Msg_UISetCEMConfigADU(cemConfig));
		}


		//[AURAMessageHandler]
		//private void HandleUISetTimeDateMsg(Msg_UISetTimeDate msg)
		//{
		//	Debug.WriteLine("CEM: Time/Date: {0:yyyy/MM/dd HH:mm:ss}", msg.UTCDateTime);
		//}


		//[AURAMessageHandler]
		//private void HandleUICalibOpResultMsg(Msg_UICalibOpResult msg)
		//{

		//}


		//[AURAMessageHandler]
		//private void HandleUICEMInfoMsg(Msg_UICEMInfo msg)
		//{

		//}


		[AURAMessageHandler]
		private void HandleCEMHello(Msg_CEMHello msg)
		{
			//menuManager.DeviceInfo.CEM = msg.CEMVersion.Major.ToString("00") + "."
			//					+ msg.CEMVersion.Minor.ToString("00") + "."
			//					+ msg.CEMVersion.Revision.ToString("00");
			//menuManager.DeviceInfo.RCM = msg.RCMVersion.Major.ToString("00") + "."
			//					+ msg.RCMVersion.Minor.ToString("00") + "."
			//					+ msg.RCMVersion.Revision.ToString("00");
			////deviceInfoModel.Name = msg.MachineName.Substring(0, 2) + " " + msg.MachineName.Substring(3);
			//menuManager.DeviceInfo.MAC = msg.Address == AURADef.InvalidMACAddress ? "" : msg.Address.ToString("x6");//
			//menuManager.DeviceInfo.GroupID = msg.NetworkId;
			//cemConfig.DNTNetworkId = msg.NetworkId;
			ConfigVersion = msg.ConfigVersion;
            //devInfo_ = msg.ConfigVersion;
            //cemConfig.Flags = (CEMConfigFlag)msg.Flags;

            //menuManager.CreateDeviceInfoValuesList();
			
			//SendMgmtMsg(new Msg_UIMHello());
			devInfo_.NoteCEMHelloMsg(msg);
			SendMgmtMsg(new Msg_UIGetCEMConfigADU());
			SendMgmtMsg(new Msg_UIGetUIMConfig());
		}


		[AURAMessageHandler]
		private void HandleUIMHello(Msg_UIMHello msg)
		{
			devInfo_.NoteUIMHelloMsg(msg);
		}


		private void prodDispatcherTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (productivity.prod.Time != "0000-00-00 00:00:00"
					&& Convert.ToDouble(productivity.prod.Loc.Lat) != 0.0
					&& Convert.ToDouble(productivity.prod.Loc.Lon) != 0.0)
				{
					string prodLog = logDir + "PD_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
					Thread t = new Thread(() =>
					{
						lock (locker)
						{
							try
							{
								File.WriteAllText(prodLog, jsToSend.TrimStart());
								SendCasLogZip(machineConfig.MachineName, prodLog, LongData.ProductivityLog);
								Thread.Sleep(60000);
							}
							catch { }
						}
					});
					t.IsBackground = true;
					t.Start();

					jsonCounter = 0;

					try
					{
						//if (DateTime.ParseExact(Properties.Settings.Default.MaintLogTimeStamp, "yyyy-MM-dd HH:mm:ss", null).Date != DateTime.Today
						if (Properties.Settings.Default.CheckedtemCount != NumberOfChecked()
							&& productivity.prod.Asset != String.Empty
							&& Convert.ToDouble(productivity.prod.Loc.Lon) != 0
							&& Convert.ToDouble(productivity.prod.Loc.Lat) != 0
							&& productivity.prod.Time != "0000-00-00 00:00:00")
						{
							sentMaintZip = true;
							string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							GenerateMaintLog(machineConfig.MachineName,
								Convert.ToDouble(productivity.prod.Loc.Lat),
								Convert.ToDouble(productivity.prod.Loc.Lon), LogTasks);

							maintMetaFileName = logDir + "ML_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";

							Thread th = new Thread(() =>
							{
								lock (locker)
								{
									try
									{
										GenerateMetaFile(
											maintMetaFileName,
											machineConfig.MachineName,
											DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
											LongData.Maint,
											Convert.ToDouble(productivity.prod.Loc.Lat),
											Convert.ToDouble(productivity.prod.Loc.Lon),
											startTime,
											DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
											maintLogFileName,
											maintLogSize);

										SendCasLogZip(machineConfig.MachineName, maintLogFileName, LongData.Maint);

										Thread.Sleep(60000);
									}
									catch { }
								}
							});
							th.IsBackground = true;
							th.Start();

							Properties.Settings.Default.CheckedtemCount = NumberOfChecked();
							Properties.Settings.Default.Save();
						}
					}
					catch { }
					try
					{
						if (Convert.ToDouble(productivity.prod.Loc.Lon) != 0
							&& Convert.ToDouble(productivity.prod.Loc.Lat) != 0
							&& productivity.prod.Time != "0000-00-00 00:00:00"
							//&& !MainWindow.sentEventZip)
							&& DateTime.ParseExact(Properties.Settings.Default.MainLogSentTime, "yyyy-MM-dd HH:mm:ss", null).Date != DateTime.Today)
						{
							eventlog = true;
							startTimeEvent = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							GetCASLog(AURALog.Primary);
						}
					}
					catch { }
					try
					{
						if (Convert.ToDouble(productivity.prod.Loc.Lon) != 0
							&& Convert.ToDouble(productivity.prod.Loc.Lat) != 0
							&& productivity.prod.Time != "0000-00-00 00:00:00"
							&& sentEventZip
							//&& !MainWindow.sentIncidentZip)
							&& DateTime.ParseExact(Properties.Settings.Default.IncidentLogSentTime, "yyyy-MM-dd HH:mm:ss", null).Date != DateTime.Today)
						{
							eventlog = false;
							startTimeIncident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							GetCASLog(AURALog.Secondary);
						}
					}
					catch { }
                }
			}
			catch { }
		}




		[AURAMessageHandler]
		private void HandleUIGetJSONData(Msg_UIGetJSONData msg)
		{
			JsonData += Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(msg.Text)).Replace("?", "");
			textBoxJsonData.ScrollToEnd();
            if (msg.Text.Contains("prod"))
            {
                jsonCounter++;
            }

            jsProductivity += Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(msg.Text)).Replace("?", "");
            string zipName = String.Empty;
            string temp = jsProductivity;

            if (temp.Trim().EndsWith("}"))
            {
                try
                {
                    temp = temp.Trim();
                    temp = temp.Remove(temp.Length - 1);
                    if (temp.Trim().EndsWith("}"))
                    {
                        var firstIndex = temp.IndexOf("prod");
                        if (firstIndex == temp.LastIndexOf("prod") && firstIndex != -1)
                        {
                            temp += "}";
                            var json = "[" + temp + "]";
                            temp = String.Empty;

                            List<Productivity> prodList = JsonConvert.DeserializeObject<List<Productivity>>(json);

                            productivity = new Productivity(prodList.Last());
                            jsToSend = jsProductivity;
                            File.AppendAllText(metromDir + "jsonLog.log", jsToSend);
                            jsProductivity = String.Empty;

                            if (productivity.prod.Time != "0000-00-00 00:00:00" && !MainWindow.gotTime)
                            {
                                jsonCounter = 0;
                                MainWindow.gotTime = true;
                            }

                            GetMemoryUsage();

                            btnDisplayMainLogs.IsEnabled = true;
                            btnDisplayIncidentLogs.IsEnabled = true;
                            btnDisplayRecentMainLogs.IsEnabled = true;
                            btnDisplayRecentIncidentLogs.IsEnabled = true;

                            //string prodLog = logDir + "PD_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
                            //Thread t = new Thread(() =>
                            //{
                            //	lock (locker)
                            //	{
                            //		try
                            //		{
                            //			File.WriteAllText(prodLog, jsToSend.TrimStart());
                            //			SendCasLogZip(productivity.prod.Asset, prodLog, LongData.ProductivityLog);
                            //			Thread.Sleep(60000);
                            //		}
                            //		catch { }
                            //	}
                            //});
                            //t.IsBackground = true;
                            //t.Start();

                            if (!MainWindow.prodTimerRan)
                            {
                                prodDispatcherTimer = new DispatcherTimer();
                                prodDispatcherTimer.Tick += prodDispatcherTimer_Tick;
                                prodDispatcherTimer.Interval = new TimeSpan(0, machineConfig.ProdReportInterval, 0);
                                MainWindow.prodTimerRan = true;
                                prodDispatcherTimer.Start();
                            }
                        }
                        else
                        {
                            temp = String.Empty;
                            jsProductivity = String.Empty;
                        }
                    }
                }
                catch (Exception e)
                {
                    temp = String.Empty;
                    jsProductivity = String.Empty;
                }
            }


        }


		[AURAMessageHandler]
		private void HandleUIGetConfigLogMsg(Msg_UIGetConfigLog msg)
		{
			jsConfig += Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(msg.Text)).Replace("?", "");
			string temp = jsConfig;

			if (temp.Trim().EndsWith("}"))
			{
				try
				{
					string confLogFile = logDir + "CL_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
					File.WriteAllText(confLogFile, jsConfig.TrimStart());

					Thread t = new Thread(() =>
					{
						try
						{
							Thread.Sleep(3000);
							SendCasLogZip(machineConfig.MachineName, confLogFile, LongData.ConfigurationLog);
							Thread.Sleep(60000);
						}
						catch { }
					});
					t.IsBackground = true;
					t.Start();
				}
				catch { }
			}
		}


		[AURAMessageHandler]
		private void HandleUIGetDiagnostics(Msg_UIGetDiagnostics msg)
		{
			DiagnosticsData += Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(msg.Text));
			textBoxDiag.ScrollToEnd();
			File.AppendAllText(metromDir + "diagnosticsLog.log", msg.Text);//remove!!!
		}


		[AURAMessageHandler]
		private void HandleUIGetUIMConfig(Msg_UIGetUIMConfig msg)
		{
			uimConfig = msg.Config;
		}


		[AURAMessageHandler]
		private void HandleUIGetEncoderConfig(Msg_UIGetEncoderConfig msg)
		{
			if (msg != null)
			{
				if (msg.Text.Length > 0)
				{
					JsonData += System.Environment.NewLine + msg.Text.Remove(msg.Text.Length - 1) + System.Environment.NewLine;
					textBoxJsonData.ScrollToEnd();
				}
			}
		}


		[AURAMessageHandler]
		private void HandleUICEMRangeData(Msg_UICEMRangeData msg)
		{
			if (!peerMap_.ContainsKey(msg.Address))
				return;  // EARLY RETURN!

			Peer peer = peerMap_[msg.Address];
			peer.NewRangeData(msg);

			string dataCheck;

			if (msg.RCMRangeStatus == 0)
			{
				switch (msg.DataCheck)
				{
					case RCMCheckToken.DataOk:
						dataCheck = "Data OK";
						break;
					case RCMCheckToken.DataIsZero:
						dataCheck = "Data is ZERO **********";
						break;
					case RCMCheckToken.DataIsHuge:
						dataCheck = "Data is HUGE **********";
						break;
					case RCMCheckToken.DataTypeUnexpected:
						dataCheck = "Data type unexpected";
						break;
					case RCMCheckToken.NoData:
						dataCheck = "No data";
						break;
					case RCMCheckToken.DataDiscarded_HighREE:
						dataCheck = "DISCARD: High REE";
						break;
					case RCMCheckToken.DataDiscarded_UnexpectedRangeChange:
						dataCheck = "DISCARD: Unexpected range change";
						break;
					default:
						dataCheck = string.Format("Bad check value 0x{0:x8}", msg.DataCheck);
						break;
				}
			}
			else
				dataCheck = "No data";

			DateTime now = DateTime.Now;
			lastCEMRangeData_ = now;
			string finalResult = ((msg.RangeResult == RCMRangeResult.GotRange) && !msg.RangeWasDiscarded) ? "ok" : "XX";
		}


		[AURAMessageHandler]
		private void HandleUICEMPeerUpdate(Msg_UICEMPeerUpdate msg)
		{
		}


		[AURAMessageHandler]
		private void HandleUIOAStateMsg(Msg_UIOAState msg)
		{
			if (commState_ == AppCommState.Connecting)
				devInfo_.NoteOAStateMsg();
		}


		[AURAMessageHandler]
		private void HandleUIDataUpdate(Msg_UIDataUpdate msg)
		{
			for (int ndx = 0; ndx < 2; ++ndx)
			{
				DataUpdateMsgPeerData pd = msg.PeerData[ndx];

				if (ndx == 0)
				{
					int distance = (int)(pd.Separation * SciCon.M_TO_FT);

					if (distance >= 0 && distance <= 9999)
					{
						PeerFrontRange = pd.Name != "  000000" ? distance.ToString() : " - - - - ";
					}	

					if (pd.SepSource == MetromTablet.Communication.SeparationSource.RCM && PeerFrontRange != " - - - - ")
					{
						PeerFrontRange += ".";
					}
				}
				else
				{
					int distance = (int)(pd.Separation * SciCon.M_TO_FT);

					if (distance >= 0 && distance <= 9999)
					{
						PeerRearRange = pd.Name != "  000000" ? (distance).ToString() : " - - - - ";
					}

					if (pd.SepSource == MetromTablet.Communication.SeparationSource.RCM && PeerRearRange != " - - - - ")
					{
						PeerRearRange += ".";
					}
				}
			}
		}


		[AURAMessageHandler]
		private void HandleUIData(Msg_UIData msg)
		{
			try
			{
				switch (msg.UiaPrimitive)
				{
					case UI_Enums.UIA_Primitive.UIA_FPVFD_WriteString:
						DisplayTextFirstRow = msg.Data.Substring(0, Math.Min(16, msg.Data.IndexOf('\0')));
						break;
					case UI_Enums.UIA_Primitive.UIA_FPVFD_WriteDisplayFullLine:
						if (msg.Data3 == 0 && !msg.Data.Contains("yyy"))
						{
							DisplayTextFirstRow = msg.Data.Substring(0, Math.Min(16, msg.Data.IndexOf('\0')));
							if (DisplayTextFirstRow.StartsWith("Page:"))
								DisplayTextFirstRow = "Page: ▲Up ▼Down";
						}
						if (msg.Data3 == 1)
						{
							DisplayTextSecondRow = msg.Data.Substring(0, Math.Min(16, msg.Data.IndexOf('\0')));
							if (DisplayTextSecondRow.Contains("Alarm") && DisplayTextSecondRow.Contains("Relay"))
								DisplayTextSecondRow = "  ✓Alarm ▶Relay";
						}
						break;
					//case UI_Enums.UIA_Primitive.UIA_FPVFD_DefineCustomGlyph:
					//	DisplayTextSecondRow = msg.Data.Substring(0, msg.Data.IndexOf('\0')).TrimEnd('?', 'Q');
					//	break;
					//case UI_Enums.UIA_Primitive.UIA_SSD_SetWarningLED:
					//	//DisplayTextSecondRow = msg.Data.Substring(0, msg.Data.IndexOf('\0')).TrimEnd('?', 'Q');
					//	break;
					//case UI_Enums.UIA_Primitive.UIA_FPVFD_DisplayControl:
					//	///DisplayTextSecondRow = msg.Data.Substring(0, msg.Data.IndexOf('\0'));
					//	break;
					case UI_Enums.UIA_Primitive.UIA_FPVFD_SetCursor:
						try
						{
							//var inline = (Underline)displayTextSecondRowTxtBx.Inlines.ElementAt(Convert.ToInt32(msg.Data3));
							string temp = DisplayTextSecondRow;
							DisplayTextSecondRow = String.Empty;
							displayTextSecondRowTxtBx.Inlines.Add(new Run(temp.Substring(0, Convert.ToInt32(msg.Data3))));
							displayTextSecondRowTxtBx.Inlines.Add(new Underline(new Run(temp[Math.Min(msg.Data3, temp.Length - 1)].ToString())));
							displayTextSecondRowTxtBx.Inlines.Add(new Run(temp.Substring(Math.Min(Convert.ToInt32(msg.Data3) + 1, temp.Length - 1),
								temp.Length - 1 - Math.Min(Convert.ToInt32(msg.Data3), temp.Length - 1))));
							//DisplayTextSecondRow = displayTextSecondRowTxtBx.Text;
						}
						catch { }
						break;
					//case UI_Enums.UIA_Primitive.UIA_SSD_SetUINT16:
					//	//if (msg.Data3 == 0)
					//	//	PeerFrontRange = msg.Data1.ToString();
					//	//if (msg.Data3 == 1)
					//	//	PeerRearRange = msg.Data2.ToString();

					//	if (msg.Data1 == 0)
					//		PeerFrontRange = msg.Value.ToString();
					//	if (msg.Data1 == 1)
					//		PeerRearRange = msg.Value.ToString();
					//break;
					//case UI_Enums.UIA_Primitive.UIA_SSD_SetDecimalPt:
					//	//if (msg.Data3 == 0)
					//	//	PeerFrontRange = msg.Data1.ToString() + ".";
					//	//if (msg.Data3 == 1)
					//	//	PeerRearRange = msg.Data2.ToString() + ".";
					//	if (msg.Data1 == 0)
					//		PeerFrontRange = msg.Value.ToString() + ".";
					//	if (msg.Data1 == 1)
					//		PeerRearRange = msg.Value.ToString() + ".";
					//	break;
					case UI_Enums.UIA_Primitive.UIA_AUDL_PlayNotification:
						switch (msg.NotificationType)
						{
							case 2:
								flashTimer.Start();
								break;
							case 3:
								break;
							case 4:
								flashTimer.Start();
								break;
							default:
								flashTimer.Stop();
								FlashON(false);
								break;
						}
						break;
				}
			}
			catch { }
		}


		[AURAMessageHandler]
		private void HandleUICEMSubSystemStatus(Msg_UICEMSubSystemStatus msg)
		{
			if (commState_ == AppCommState.Connecting)
				devInfo_.NoteCEMSubSystemStatusMsg();

			switch (msg.SubSystem)
			{
				case CEMSubSystem.Comms:
					devInfo_.DNTSSStatus = msg.CommsStatus;
					break;

				case CEMSubSystem.GPS:
					devInfo_.GPSSSStatus = msg.GPSStatus;
					break;

				case CEMSubSystem.RCM:
					devInfo_.RCMSSStatus = msg.RCMStatus;
					break;

				default:
					//AddLocalTextToOutput("Got unknown CEM subsystem value 0x" + ((byte)msg.SubSystem).ToString("x2"));
					break;
			}
		}


		[AURAMessageHandler]
		private void HandleUILogStatusMsg(Msg_UILogStatus msg)
		{
			if (dlgViewLog2_ != null)
				dlgViewLog2_.LogStatusFromCEM(msg.Data);

		}


		[AURAMessageHandler]
		private void HandleUILogEntryMsg(Msg_UILogEntry msg)
		{
			try
			{
				if (dlgViewLog2_ != null)
					dlgViewLog2_.LogEntryFromCEM(msg.Data);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


		[AURAMessageHandler]
		private void HandleUICommandAckMsg(Msg_UICommandAck msg)
		{
			if (fwUpdateData_ != null)
			{
				switch (msg.Source)
				{
					// ----- CEM Reflash ------------------------------
					case (byte)AURAMsgOpcode.UICEMReflashInit:
						if (msg.Result == UICmdResult.Success || msg.Result > 4)
						{
							//Starting CEM firmware update
							SendNextCEMReflashDataBlock(true);
						}
						else
						{
							//UICEMReflashInit command failed

							SendMgmtMsg(new Msg_UICEMReflashFini());
							MessageBox.Show(string.Format("UICEMReflashInit command failed with result {0}, result data {1}", msg.Result, msg.ResultData), "Command Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
							fwUpdateCurOfs_ = 0;
						}
						break;

					case (byte)AURAMsgOpcode.UICEMReflashData:
						if (msg.Result == UICmdResult.Success || msg.Result > 4)
						{
							if (fwUpdateCurOfs_ < fwUpdateSize_)
								SendNextCEMReflashDataBlock(true);
							else
							{
								SendMgmtMsg(new Msg_UICEMReflashFini());
								fwUpdateCurOfs_ = 0;
							}
						}
						else
						{
							MessageBox.Show(string.Format("UICEMReflashData command failed with result {0}, result data {1}", msg.Result, msg.ResultData), "Command Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
							SendMgmtMsg(new Msg_UICEMReflashFini());
							fwUpdateCurOfs_ = 0;
						}
						break;

					case (byte)AURAMsgOpcode.UICEMReflashFini:
						fwUpdatePopup.UpdateDone(true);//msg.Result == UICmdResult.Success);
						fwUpdateCurOfs_ = 0;
						fwUpdatePopup.ProgressPercent = 100;
						if (msg.Result != UICmdResult.Success)
							MessageBox.Show(string.Format("UICEMReflashFini command failed with result {0}, result data {1}", msg.Result, msg.ResultData), "Command Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
						break;
				}
			}
		}


		private void SendNextCEMReflashDataBlock(bool updateFeedback)
		{
			try
			{
				uint remainLen = fwUpdateSize_ - fwUpdateCurOfs_;
				ushort len = (ushort)((remainLen >= kMaxFWUpdateBlockSize) ? kMaxFWUpdateBlockSize : remainLen);

				//dlgFWUpdate_.PercentDone = ((double)fwUpdateCurOfs_ / fwUpdateSize_) * 99.0;
				//if (updateFeedback)
				//dlgFWUpdate_.Feedback = string.Format("Writing to flash... {0} bytes left, {1:f0}% done", fwUpdateSize_ - fwUpdateCurOfs_, dlgFWUpdate_.PercentDone);

				SendMgmtMsg(new Msg_UICEMReflashData(fwUpdateCurOfs_, len, fwUpdateData_));

				// NOTE: This here assumes success, and should be moved later when we support retries.
				fwUpdateCurOfs_ += len;

				fwUpdatePopup.ProgressPercent = Math.Round(100 * (double)fwUpdateCurOfs_ / (double)fwUpdateSize_);
			}
			catch { }
		}


		private void SendNextPMReflashDataBlock(byte[] fwUpdData, uint fwUpdSize, uint fwUpdCurOfs, int chunkLength)
		{
			if (pmAcknowledged)
			{
				try
				{
					//uint remainLen = fwUpdateSize_ - fwUpdateCurOfsPM_;
					uint remainLen = fwUpdSize - fwUpdCurOfs;
					//ushort len = (ushort)((remainLen >= 64) ? 64 : remainLen);
					ushort len = (ushort)((remainLen >= chunkLength) ? (uint)chunkLength : remainLen);
					byte[] tempData = new byte[len];
					//Array.Copy(fwUpdateData_, fwUpdateCurOfsPM_, tempData, 0, len);
					Array.Copy(fwUpdData, fwUpdCurOfs, tempData, 0, len);
					SendMgmtMsg(new Msg_UISendLogZip(tempData, len));

					fwUpdateCurOfsPM_ += len;
					fwUpdatePopup.ProgressPercent = Math.Round(100 * (double)fwUpdCurOfs / (double)fwUpdSize);
					pmAcknowledged = false;

					//if (fwUpdateCurOfsPM_ == fwUpdateSize_)
					if (fwUpdCurOfs == fwUpdSize)
					{
						fwUpdatePopup.PM = false;
						fwUpdateCurOfsPM_ = 0;
						Thread.Sleep(1000);
						ConfigSerial();
					}
				}
				catch { }
			}
		}


		[AURAMessageHandler]
		private void HandleUIGetLogAcknowledge(Msg_UIGetLogAcknowledge msg)
		{
			//if (msg.Text.Contains("OK"))
			pmAcknowledged = true;
			if (fwUpdatePopup != null && fwUpdatePopup.PM)
			{
				SendNextPMReflashDataBlock(fwUpdateData_, fwUpdateSize_, fwUpdateCurOfsPM_, 64);
			}
			//if (otaPMUpdate)
			//{
			//	SendNextPMReflashDataBlock(otaData, (uint)otaSize, (uint)(destIndex * chunkSize), chunkSize);
 
			//}
		}


		[AURAMessageHandler]
		private void HandleUIFWUpdateAcknowledge(Msg_UIPMVersion msg)
		{
			//menuManager.DeviceInfo.PM = String.Format("{0}.{1}.{2}", msg.Major, msg.Minor, msg.Tweek);
			if (fwUpdateData_ != null || otaPMUpdate)
			{
				MessageBox.Show(String.Format("PM firmware version: {0}.{1}.{2}", msg.Major, msg.Minor, msg.Tweek));
				otaPMUpdate = false;
			}
		}


		public static Vector v = new Vector();
		[AURAMessageHandler]
		private void HandleUICEMAccelCalibData(Msg_UICEMAccelCalibData msg)
		{
			string txt = string.Format("ACCEL,,,,,,,,,,,,,,{0:f5},{1:f5},{2:f5},{3:f5}", msg.GravityOffsetVec[0], msg.GravityOffsetVec[1], msg.GravityOffsetVec[2], msg.GravityOffsetVec.Magnitude());

			//if (winAccel_ != null)
			//	winAccel_.SetCalibVectors(msg.GravityOffsetVec, msg.FrontUnitVec);

			if (calib != null)
			{
				calib.GravityOffset = msg.GravityOffsetVec;
				calib.FrontVector = msg.FrontUnitVec;
			}
		}

		int destIndex = 0;
		int otaSize;
        int chunksCount = 0;

		[AURAMessageHandler]
		private void HandleUIOTAinit(Msg_UIOTAinit msg)
		{
			otaArchiveName = msg.Name.Substring(0, msg.Name.IndexOf('\0'));
			otaSize = msg.Length;
			otaDataReceived = new byte[otaSize + (chunkSize - otaSize % chunkSize)];
			destIndex = 0;
            chunksCount = otaSize / chunkSize;
			//SendMgmtMsg(new Msg_UIOTAack());
		}


        int chunkSize = 230;
		bool otaPMUpdate = false;

        [AURAMessageHandler]
		private void HandleUIOTAdata(Msg_UIOTAdata msg)
		{
			try
			{
                Array.Copy(msg.Data, 0, otaDataReceived, chunkSize * destIndex, chunkSize);
                //if (destIndex < chunksCount) // (destIndex + 1))
				//{

                    //try
                    //{
                   //     Array.Copy(msg.Data, 0, otaDataReceived, (chunkSize * destIndex++), chunkSize);
                    //}
                    //catch (Exception e)
                    //{
                    //    MessageBox.Show(e.Message);
                    //}
                    //SendMgmtMsg(new Msg_UIOTAack());
                destIndex++;
				Status = "OTA updates received: " + (100 * chunkSize * destIndex / otaSize).ToString() + "%";
				//}
				//else
				//{
				//	Array.Copy(msg.Data, 0, otaDataReceived, (chunkSize * destIndex), chunkSize);		
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


        [AURAMessageHandler]
        private void HandleUIOTAcomplete(Msg_UIOTAcomplete msg)
        {
            if (msg.Success)
            {
                Status = "OTA updates received: 100%";
                otaData = new byte[otaSize];
                try
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(metromUpdatesDir);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        //foreach (DirectoryInfo dir in di.GetDirectories())
                        //{
                        //    dir.Delete(true);
                        //}
                    }
                    catch { }

                    Array.Copy(otaDataReceived, 0, otaData, 0, otaSize);
                    CreateZipFromBytes(otaData, metromUpdatesDir + otaArchiveName);

                    //string[] files = Directory.GetFiles(metromUpdatesDir);
                    //foreach (string fileName in files)
                    //{
                    //	if (!fileName.EndsWith(".zip"))
                    //		File.Delete(fileName);
                    //}
                    //File.Copy(metromDir + otaArchiveName, metromUpdatesDir + otaArchiveName, true);//remove!!!

                    Status = "Extracting files...";
                    System.IO.Compression.ZipFile.ExtractToDirectory(metromUpdatesDir + otaArchiveName, metromUpdatesDir);
                    Thread.Sleep(5000);
                    Status = String.Empty;
                    string[] files = Directory.GetFiles(metromUpdatesDir);

                    foreach (string fileName in files)
                    {
                        if (fileName.Contains("PM"))
                        {
                            // PM update
                            FileInfo fi = new FileInfo(fileName);
                            long dataSize = fi.Length;// +(chunkSize - (fi.Length % chunkSize));
                                                      //SendMgmtMsg(new Msg_UILogInfo(LongData.Binary, (uint)chunkSize, (uint)dataSize, fwUpdatePopup.CalculateCRC(otaData)));
                                                      //otaPMUpdate = true;
                            try
                            {
                                fwUpdatePopup = new FWUpdatePopup(true);
                                fwUpdatePopup.FileName = fileName;
                                fwUpdatePopup.Update(true);
                                fwUpdatePopup.Closed += (s, e_) =>
                                {
                                    fwUpdatePopup = null;
                                    mIdle.IsEnabled = false;
                                    mIdle.IsEnabled = true;
                                };
                                fwUpdatePopup.ShowDialog();
                            }
                            catch { }

                        }
                            if (fileName.Contains("CEM"))
                            {
                        //    // CEM update
                        //    FileInfo fi = new FileInfo(fileName);
                        //    long dataSize = fi.Length;
                        //    try
                        //    {
                        //        fwUpdatePopup = new FWUpdatePopup(true);
                        //        fwUpdatePopup.FileName = fileName;
                        //        fwUpdatePopup.Update(true);
                        //        fwUpdatePopup.Closed += (s, e_) =>
                        //        {
                        //            fwUpdatePopup = null;
                        //            mIdle.IsEnabled = false;
                        //            mIdle.IsEnabled = true;
                        //        };
                        //        fwUpdatePopup.ShowDialog();
                        //    }
                        //    catch { }
                        }
                        if (fileName.StartsWith("setup"))
                        {
                            try
                            {
                                Process.Start(metromUpdatesDir + "setup.exe");
                                Application.Current.Shutdown();
                            }
                            catch { }
                        }
                    }
                    }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            else
            {
                otaArchiveName = String.Empty;
                otaSize = 0;
                otaDataReceived = null;
                destIndex = 0;
                MessageBox.Show("The modem is not responding");
            }
        }

        #endregion AURA Message Handlers
    }
}
