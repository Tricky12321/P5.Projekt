using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
namespace Serial
{
	class INSReader : HzCalculator
	{
		private SerialPort _serialPort;
		public long Tid = 0;
		public static Stopwatch Timer_Input;
		public static long Last_Timer = 0;

		private XYZ Accel_Calibration;
		private XYZ Gyro_Calibration;
		private bool UseCalibration = false;

		public XYZ AcceXYZ
		{
			get
			{
				if (UseCalibration)
				{
					return new XYZ(XAC - Accel_Calibration.X, YAC - Accel_Calibration.X, ZAC - Accel_Calibration.X, Tid);
				}
				else
				{
					return new XYZ(XAC, YAC, ZAC, Tid);
				}
			}
		}

		public XYZ GyroXYZ
		{
			get
			{
				if (UseCalibration)
				{
					return new XYZ(XGY - Gyro_Calibration.X, YGY - Gyro_Calibration.Y, ZGY - Gyro_Calibration.Z, Tid);
				}
				else
				{
					return new XYZ(XGY, YGY, ZGY, Tid);
				}
			}
		}

		private double XGY;
		private double YGY;
		private double ZGY;

		private double XAC;
		private double YAC;
		private double ZAC;

		private object LockObject = new object();
		private bool NewData = false;

		public INSReader(Stopwatch Timer)
		{
			Timer_Input = Timer;
			_serialPort = SerialReader.GetSerialPort(ArduinoTypes.INS);
		}

		public Tuple<XYZ, XYZ> Read()
		{
			try
			{
				List<string> dataItems = new List<string>(_serialPort.ReadLine().Split('#'));
				foreach (var data in dataItems)
				{
					CheckData(data);
				}
				long TimerSinceLast = Timer_Input.ElapsedMilliseconds - Last_Timer;
				Last_Timer = Timer_Input.ElapsedMilliseconds;
				Tid = Tid + TimerSinceLast;
				HZ_rate = TimerSinceLast;
				return new Tuple<XYZ, XYZ>(AcceXYZ, GyroXYZ);
			}
			catch (Exception)
			{
				return Read();
			}

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
				/*else if (data.Contains("timer"))
				{
					UInt32 TimerSinceLast = Convert.ToUInt32(data.Replace("timer:", "").Replace("\r", "").Replace("-", ""));

					if (TimerSinceLast == 0)
					{
						Console.WriteLine("Something went bad!!");
					}

				}
				*/

			}
			catch (TimeoutException) { }
			catch (FormatException) { }
			catch (IndexOutOfRangeException) { }
		}

		public void ResetTid()
		{
			Tid = 0;
			Last_Timer = Timer_Input.ElapsedMilliseconds;
		}

		public void SetCalibration(XYZ Acclerometer, XYZ Gyroscope)
		{
			Accel_Calibration = Acclerometer;
			Gyro_Calibration = Gyroscope;
			UseCalibration = true;
		}

		public void ClearCalibration() {
			UseCalibration = false;
		}
	}
}
