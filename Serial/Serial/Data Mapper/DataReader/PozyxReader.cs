using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;
namespace Serial.DataMapper.DataReader
{
	class PozyxReader : HzCalculator
	{
		SerialPort _serialPort;
		public long Tid;

		public static Stopwatch Timer_Input;
		public static long Last_Timer;

		public PozyxReader(Stopwatch Timer)
		{
			Timer_Input = Timer;
			_serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
			Console.WriteLine($"{_serialPort} Serial Port Opened");
		}

		public XYZ Read()
		{
			XYZ Output = null;
			string data = "";
			do
			{
				data = _serialPort.ReadLine();
				Output = CheckData(data);
			} while (Output == null);

			long TimerSinceLast = Timer_Input.ElapsedMilliseconds - Last_Timer;
			Last_Timer = Timer_Input.ElapsedMilliseconds;
			Tid += TimerSinceLast;
			HZ_rate = TimerSinceLast;
			Output.TimeOfData = Tid;
			return Output;

		}

		XYZ CheckData(string data)
		{

			if (data.Contains("PO") && data.Contains(":"))
			{
				data = data.Substring(2, data.Length - 3);

				var message_split = data.Split(':');
				try
				{
					double Xx = Convert.ToDouble(message_split[0]);
					double Yy = Convert.ToDouble(message_split[1]);
					double Zz = Convert.ToDouble(message_split[2]);
					return new XYZ(Xx, Yy, Zz);

				}
				catch (IndexOutOfRangeException)
				{
					Console.WriteLine("[POZYX] Out of range");
					return null;
				}
				catch (FormatException)
				{
					Console.WriteLine("[POZYX] Format exception");

					return null;
				}
			}
			return null;
		}

		public void ResetTid()
		{
			Tid = 0;
			Last_Timer = Timer_Input.ElapsedMilliseconds;
		}
	}
}