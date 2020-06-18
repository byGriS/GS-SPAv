using NModbus;
using NModbus.Serial;
using Service;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;

namespace Input {
	public class Work {

		private Input inputCUDR = null;
		private Input inputGS = null;
		private ObservableCollection<Stage> ListStages;
		private bool isCUDR, isGS;
		private bool GSerror = false;

		public delegate void ChangedState(string source, int state, string error);
		public event ChangedState OnChangedState;

		private Random testRandom = new Random();
		private Random r = new Random();


		private Timer TimerSecond = new Timer(1000);
		private int intervalReadCUDR = 2;
		private int intervalReadGS = 3;
		private int timerReadCUDR = 0;
		private int timerReadGS = 0;

		public Work(Input inputCUDR, Input inputGS, ObservableCollection<Stage> listStages) {
			this.inputCUDR = inputCUDR;
			this.inputGS = inputGS;
			this.ListStages = listStages;

			TimerSecond.Elapsed += TimerSecond_Elapsed;
		}

		public void Start(bool isCUDR, bool isGS, int intervalReadGS, int intervalReadCUDR) {
			this.isCUDR = isCUDR;
			this.isGS = isGS;
			this.intervalReadGS = intervalReadGS;
			this.intervalReadCUDR = intervalReadCUDR;

			if (isCUDR) {
				inputCUDR.Master = new ModbusFactory().CreateRtuMaster(new SerialPortAdapter(inputCUDR.SerialPort));
				inputCUDR.Master.Transport.ReadTimeout = 500;
				inputCUDR.Master.Transport.Retries = 1;
				inputCUDR.Master.Transport.RetryOnOldResponseThreshold = 1;
				inputCUDR.Master.Transport.SlaveBusyUsesRetryCount = false;
				try {
					inputCUDR.SerialPort.Open();
				} catch (Exception ex) {
					OnChangedState?.Invoke("CUDR", 0, ex.Message);
				}
			}

			if (isGS) {
				inputGS.SerialPort.BaudRate = 57600;
				try {
					inputGS.SerialPort.Open();
					inputGS.SerialPort.DataReceived -= SerialPort_DataReceived;
					inputGS.SerialPort.DataReceived += SerialPort_DataReceived;
				} catch (Exception ex) {
					OnChangedState?.Invoke("GS", 0, ex.Message);
				}
			}
			timerReadCUDR = 0;
			timerReadGS = 0;
			TimerSecond.Start();
		}


		public void Stop() {
			TimerSecond.Stop();
			if (inputCUDR.SerialPort.IsOpen)
				inputCUDR.SerialPort.Close();
			if (inputGS.SerialPort.IsOpen)
				inputGS.SerialPort.Close();
		}

		private void TimerSecond_Elapsed(object sender, ElapsedEventArgs e) {
			timerReadCUDR--;
			if (isCUDR && timerReadCUDR <= 0) {
				timerReadCUDR = intervalReadCUDR;
				ReadCUDR();
			}
			timerReadGS--;
			if (isGS && timerReadGS <= 0) {
				ReadGS();
				timerReadGS = intervalReadGS;
			}
		}

		public void ReadCUDR() {
			//inputCUDR.InputParams[0].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 1 });
			for (int i = 1; i < inputCUDR.InputParams.Count - 3;) {
				//inputCUDR.InputParams[i].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = (float)(testRandom.Next(1000) / 10.0) });
				//i++;
				//continue;

				byte slaveId = Convert.ToByte(inputCUDR.InputParams[i].SlaveID);
				string[] addressSplit = (inputCUDR.InputParams[i].Address.ToString()).Replace('.', ',').Split(',');
				ushort startAddress = (ushort)(Convert.ToInt16(addressSplit[0]));
				if (startAddress == 0) {
					i++;
					continue;
				}
				ushort numRegisters = 0;
				int countRegister = 1;
				switch (inputCUDR.InputParams[i].TypeParam) {
					case TypeParam.BIT:
					case TypeParam.WORD:
						numRegisters = 1;
						break;
					case TypeParam.DWORD:
					case TypeParam.FLOAT:
						numRegisters = 2;
						break;
				}

				#region определение кол-во читаемых регистров
				for (int j = i + 1; j < inputCUDR.InputParams.Count && j > -1; j++) {
					if (inputCUDR.InputParams[j].SlaveID == inputCUDR.InputParams[i].SlaveID &&
						inputCUDR.InputParams[j].Command == inputCUDR.InputParams[i].Command &&
						inputCUDR.InputParams[j].TypeParam == inputCUDR.InputParams[i].TypeParam) {
						switch (inputCUDR.InputParams[i].TypeParam) {
							case TypeParam.BIT:
							case TypeParam.WORD:
								if (inputCUDR.InputParams[j].Address - (inputCUDR.InputParams[i].Address + (countRegister - 1) * 1) == 1)
									countRegister++;
								else
									j = int.MaxValue;
								break;
							case TypeParam.DWORD:
							case TypeParam.FLOAT:
								if (inputCUDR.InputParams[j].Address - (inputCUDR.InputParams[i].Address + (countRegister - 1) * 2) == 2)
									countRegister++;
								else
									j = int.MaxValue;
								break;
						}
					} else
						break;
				}
				//i += countRegister - 1;
				numRegisters = (ushort)(numRegisters * countRegister);
				#endregion
				#region чтение данных
				ushort[] registers = null;
				bool[] coils = null;
				if (inputCUDR.InputParams[i].Command == Command.ReadCoils) {
					try {
						coils = inputCUDR.Master.ReadCoils(slaveId, startAddress, numRegisters);
					} catch { }
				}
				if (inputCUDR.InputParams[i].Command == Command.ReadHoldingRegisters) {
					try {
						registers = inputCUDR.Master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
					} catch {
						OnChangedState?.Invoke("CUDR", 0, "Устройство не отвечает");
						return;
						i++;
					}
				}
				if ((registers == null) && (coils == null)) {
					continue;
				}
				#endregion
				#region преобразование прочитанных регистров
				switch (inputCUDR.InputParams[i].TypeParam) {
					case TypeParam.WORD:
						for (int r = 0; r < registers.Length; r++) {
							Int16 int16value = BitConverter.ToInt16(new byte[] {
										BitConverter.GetBytes(registers[r])[0],
										BitConverter.GetBytes(registers[r])[1] },
								0);
							inputCUDR.InputParams[i++].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = int16value });
						}
						break;
					case TypeParam.DWORD:
						for (int r = 0; r < registers.Length; r += 2) {
							int intValue = BitConverter.ToInt32(new byte[] {
										BitConverter.GetBytes(registers[r])[0],
										BitConverter.GetBytes(registers[r])[1],
										BitConverter.GetBytes(registers[r+1])[0],
										BitConverter.GetBytes(registers[r+1])[1]},
								0);
							inputCUDR.InputParams[i++].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = intValue });
						}
						break;
					case TypeParam.FLOAT:
						for (int r = 0; r < registers.Length; r += 2) {
							float floatData = BitConverter.ToSingle(new byte[] {
										BitConverter.GetBytes(registers[r])[0],
										BitConverter.GetBytes(registers[r])[1],
										BitConverter.GetBytes(registers[r+1])[0],
										BitConverter.GetBytes(registers[r+1])[1]},
								0);
							inputCUDR.InputParams[i].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = inputCUDR.InputParams[i].Param.CalcValue(floatData, true) });
							i++;
						}
						break;
				}
				#endregion

			}

			if (inputCUDR.InputParams[1].Param.Points.Count <= 1) {
				inputCUDR.InputParams[6].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
				inputCUDR.InputParams[0].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
			} else {
				inputCUDR.InputParams[6].Param.Points.Add(new Service.DataParamPoint {
					DateTime = DateTime.Now,
					Value = (inputCUDR.InputParams[1].Param.Points[inputCUDR.InputParams[1].Param.Points.Count - 1].Value -
						inputCUDR.InputParams[1].Param.Points[inputCUDR.InputParams[1].Param.Points.Count - 2].Value) * (3600 / intervalReadCUDR)
				});
				if (inputGS.InputParams[0].Param.Points.Count > 0) {
					inputCUDR.InputParams[0].Param.Points.Add(new Service.DataParamPoint {
						DateTime = DateTime.Now,
						Value = (inputCUDR.InputParams[6].Param.Points[inputCUDR.InputParams[6].Param.Points.Count - 1].Value /
						inputGS.InputParams[0].Param.GetFlow() * (float)100)
					});
				} else {
					if (inputGS.InputParams[2].Param.Points.Count > 0) {
						inputCUDR.InputParams[0].Param.Points.Add(new Service.DataParamPoint {
							DateTime = DateTime.Now,
							Value = (inputCUDR.InputParams[6].Param.Points[inputCUDR.InputParams[6].Param.Points.Count - 1].Value /
							//inputGS.InputParams[2].Param.Points[inputGS.InputParams[2].Param.Points.Count - 1].Value)
							inputGS.InputParams[2].Param.GetFlow() * (float)100)
						});
					} else {
						inputCUDR.InputParams[0].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
					}
				}
			}

			if (inputCUDR.InputParams[3].Param.Points.Count <= 1) {
				inputCUDR.InputParams[7].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
				inputCUDR.InputParams[2].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
			} else {
				inputCUDR.InputParams[7].Param.Points.Add(new Service.DataParamPoint {
					DateTime = DateTime.Now,
					Value = (inputCUDR.InputParams[3].Param.Points[inputCUDR.InputParams[3].Param.Points.Count - 1].Value -
						inputCUDR.InputParams[3].Param.Points[inputCUDR.InputParams[3].Param.Points.Count - 2].Value) * (3600 / intervalReadCUDR)
				});
				if (inputGS.InputParams[0].Param.Points.Count > 0) {
					inputCUDR.InputParams[2].Param.Points.Add(new Service.DataParamPoint {
						DateTime = DateTime.Now,
						Value = (inputCUDR.InputParams[7].Param.Points[inputCUDR.InputParams[7].Param.Points.Count - 1].Value /
						inputGS.InputParams[0].Param.GetFlow() * (float)100)
					});
				} else {
					if (inputGS.InputParams[2].Param.Points.Count > 0) {
						inputCUDR.InputParams[2].Param.Points.Add(new Service.DataParamPoint {
							DateTime = DateTime.Now,
							Value = (inputCUDR.InputParams[7].Param.Points[inputCUDR.InputParams[7].Param.Points.Count - 1].Value /
							inputGS.InputParams[2].Param.GetFlow() * (float)100)
						});
					} else {
						inputCUDR.InputParams[2].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
					}
				}
			}

			if (inputCUDR.InputParams[5].Param.Points.Count <= 1) {
				inputCUDR.InputParams[8].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
				inputCUDR.InputParams[4].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
			} else {
				inputCUDR.InputParams[8].Param.Points.Add(new Service.DataParamPoint {
					DateTime = DateTime.Now,
					Value = (inputCUDR.InputParams[5].Param.Points[inputCUDR.InputParams[5].Param.Points.Count - 1].Value -
						inputCUDR.InputParams[5].Param.Points[inputCUDR.InputParams[5].Param.Points.Count - 2].Value) * (3600 / intervalReadCUDR)
				});
				if (inputGS.InputParams[0].Param.Points.Count > 0) {
					inputCUDR.InputParams[4].Param.Points.Add(new Service.DataParamPoint {
						DateTime = DateTime.Now,
						Value = (inputCUDR.InputParams[8].Param.Points[inputCUDR.InputParams[8].Param.Points.Count - 1].Value /
						inputGS.InputParams[0].Param.GetFlow() * (float)100)
					});
				} else {
					if (inputGS.InputParams[2].Param.Points.Count > 0) {
						inputCUDR.InputParams[4].Param.Points.Add(new Service.DataParamPoint {
							DateTime = DateTime.Now,
							Value = (inputCUDR.InputParams[8].Param.Points[inputCUDR.InputParams[8].Param.Points.Count - 1].Value /
							inputGS.InputParams[2].Param.GetFlow() * (float)100)
						});
					} else {
						inputCUDR.InputParams[4].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = 0 });
					}
				}
			}

			OnChangedState?.Invoke("CUDR", 1, "");
		}

		#region ReadGS
		private byte[] read12 = StringToByteArray("12000000055C");
		private byte[] read13 = StringToByteArray("1300000004A0");
		private byte[] bufferInput = new byte[0];
		private bool readStage = false;

		public void ReadGS() {
			if (GSerror)
				OnChangedState?.Invoke("GS", 0, "ПЛК не отвечает");
			GSerror = true;
			bufferInput = new byte[0];
			try {
				inputGS.SerialPort.Write(read12, 0, read12.Length);
			} catch (Exception ex) {
				OnChangedState?.Invoke("GS", 0, ex.Message);
			}
		}

		private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) {
			SerialPort sp = sender as SerialPort;
			int bytes = sp.BytesToRead;
			byte[] buffer = new byte[bytes];
			sp.Read(buffer, 0, bytes);
			CheckBuffer(buffer);
		}

		private void CheckBuffer(byte[] buffer) {
			int tempLength = bufferInput.Length + buffer.Length;
			byte[] concat = new byte[tempLength];
			for (int i = 0; i < tempLength; i++) {
				if (i < bufferInput.Length) {
					concat[i] = bufferInput[i];
				} else {
					concat[i] = buffer[i - bufferInput.Length];
				}
			}
			bufferInput = concat;
			int paramIndex = 0;
			if (readStage) {
				if (bufferInput.Length >= 76) {
					readStage = false;
					if (bufferInput.Length > 90) {

					} else {
						string result = Encoding.UTF8.GetString(bufferInput, 10, 64);
						string stageTitle = result.Split(new string[] { "\0" }, StringSplitOptions.None)[0];
						if (ListStages.Count == 0 || ListStages[ListStages.Count - 1].Text != stageTitle)
							Application.Current.Dispatcher.Invoke(new System.Threading.ThreadStart(delegate {
								ListStages.Add(new Stage {
									ID = r.Next(int.MaxValue),
									Text = stageTitle,
									DateTime = DateTime.Now,
									IsSecond = false
								});
							}));
						OnChangedState?.Invoke("GS", 1, "");
						GSerror = false;
					}
				}
			} else {
				if (bufferInput.Length >= 628) {
					bool addLog = false;
					if (CheckCRC(bufferInput)) {
						for (int i = 8; i < bufferInput.Length - 10; i += 71) {
							//	for (int i = 6; i < bufferInput.Length - 10; i += 69) {
							byte[] paramValue = new byte[] { bufferInput[i], bufferInput[i + 1], bufferInput[i + 2], bufferInput[i + 3] };
							float floatData = BitConverter.ToSingle(paramValue, 0);
							if (floatData > 1000000 || floatData < -1000000 || (floatData > 0 && floatData < 0.00001) || (floatData < 0 && floatData > -0.00001)) {
								floatData = -1;
								addLog = true;
							}
							try {
								inputGS.InputParams[paramIndex].Param.Points.Add(new Service.DataParamPoint { DateTime = DateTime.Now, Value = inputGS.InputParams[paramIndex].Param.CalcValue(floatData) });
								paramIndex++;
							} catch { }
						}
					} else {
						string path = Constants.ENVPATH + "logfileCRC.txt";
						string result = BitConverter.ToString(bufferInput);
						System.IO.File.AppendAllText(path, DateTime.Now + ";" + result + "\r\n", System.Text.Encoding.UTF8);
					}
					
					if (addLog) {
						string path = Constants.ENVPATH + "logfile.txt";
						string result = BitConverter.ToString(bufferInput);						
						System.IO.File.AppendAllText(path, DateTime.Now + ";" + result + "\r\n", System.Text.Encoding.UTF8);
					}
					bufferInput = new byte[0];
					readStage = true;
					inputGS.SerialPort.Write(read13, 0, read13.Length);
				}
			}
		}

		public static byte[] StringToByteArray(string hex) {
			return Enumerable.Range(0, hex.Length)
											 .Where(x => x % 2 == 0)
											 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
											 .ToArray();
		}

		public bool CheckCRC(byte[] input) {
			byte[] inputWithoutCRC = new byte[input.Length - 2];
			for(int i = 0; i < input.Length - 2; i++) {
				inputWithoutCRC[i] = input[i];
			}
			byte[] crc = ModbusCRC16Calc(inputWithoutCRC);
			if (input[input.Length - 2] == crc[0] && input[input.Length - 1] == crc[1])
				return true;
			else
				return false;
		}

		public static byte[] ModbusCRC16Calc(byte[] Message) {
			byte[] CRC = new byte[2];
			ushort Register = 0xFFFF;
			ushort Polynom = 0xA001;

			for (int i = 0; i < Message.Length; i++) {
				Register = (ushort)(Register ^ Message[i]);

				for (int j = 0; j < 8; j++) {
					if ((ushort)(Register & 0x01) == 1) {
						Register = (ushort)(Register >> 1);
						Register = (ushort)(Register ^ Polynom);
					} else {
						Register = (ushort)(Register >> 1);
					}
				}
			}

			CRC[1] = (byte)(Register >> 8);
			CRC[0] = (byte)(Register & 0x00FF);

			return CRC;
		}
		#endregion
	}
}