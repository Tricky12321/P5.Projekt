using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace Serial
{
	class INSReader : HzCalculator
	{
		private SerialPort _serialPort;
		public UInt32 Tid = 0;
		public XYZ AcceXYZ
		{
			get
			{
				return new XYZ(XAC, YAC, ZAC, Tid);
			}
		}

		public XYZ GyroXYZ
		{
			get
			{
				return new XYZ(XGY, YGY, ZGY, Tid);;
			}
		}

		private double XGY;
		private double YGY;
		private double ZGY;

		private double XAC;
		private double YAC;
		private double ZAC;

		public INSReader()
		{
			_serialPort = SerialReader.GetSerialPort(ArduinoTypes.INS);
		}

		public INSReader(string SerialPortName)
		{
			_serialPort = new SerialPort(SerialPortName, 115200, Parity.None, 8, StopBits.One);
			_serialPort.Open();
			_serialPort.WriteLine("DATA OK");
		}

		public Tuple<XYZ, XYZ> Read()
		{

			string data1 = _serialPort.ReadLine();
			string data2 = _serialPort.ReadLine();
			string data3 = _serialPort.ReadLine();
			CheckData(data1);
			CheckData(data2);
			CheckData(data3);

			return new Tuple<XYZ,XYZ>(new XYZ(XAC, YAC, ZAC, Tid), new XYZ(XGY, YGY, ZGY, Tid));

		}

		private void CheckData(string data)
		{
			try
			{
				if (data.Contains("GY") && data.Contains(":"))
				{
					data = data.Substring(2, data.Length - 3);

					var message_split = data.Split(':');
					XGY = Convert.ToDouble(message_split[0]);
					YGY = Convert.ToDouble(message_split[1]);
					ZGY = Convert.ToDouble(message_split[2]);

				}
				else if (data.Contains("AC") && data.Contains(":"))
				{
					data = data.Substring(2, data.Length - 3);

					var message_split = data.Split(':');
					XAC = Convert.ToDouble(message_split[0]);
					YAC = Convert.ToDouble(message_split[1]);
					ZAC = Convert.ToDouble(message_split[2]);
				}
				else if (data.Contains("timer"))
				{
					UInt32 TimerSinceLast = Convert.ToUInt32(data.Replace("timer:", "").Replace("\r", ""));
					Tid += TimerSinceLast;
					HZ_rate = TimerSinceLast;
				}

			}
			catch (TimeoutException) { }
			catch (FormatException) { }
			catch (IndexOutOfRangeException) { }
		}

		public void ResetTid() {
			Tid = 0;
		}
	}
}
