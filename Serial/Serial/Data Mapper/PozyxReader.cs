using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;
namespace Serial
{
	class PozyxReader : HzCalculator
	{
		private SerialPort _serialPort;
		private XYZ _pozyx_data;
		public XYZ Pozyx_data => _pozyx_data;
		public long Tid = 0;

		public static Stopwatch Timer_Input;
		public static long Last_Timer = 0;

		public PozyxReader(Stopwatch Timer)
		{
			Timer_Input = Timer;
			_serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
			Console.WriteLine($"{_serialPort} Serial Port Opened");
		}

		public XYZ Read()
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
			}
            catch (Exception)
            {
                return Read();
            }

			return _pozyx_data;
		}

		private void CheckData(string data)
		{

			if (data.Contains("PO") && data.Contains(":"))
			{
				data = data.Substring(2, data.Length - 3);

				var message_split = data.Split(':');
				double Xx = Convert.ToDouble(message_split[0]);
				double Yy = Convert.ToDouble(message_split[1]);
				double Zz = Convert.ToDouble(message_split[2]);
				_pozyx_data = new XYZ(Xx, Yy, Zz, Tid);
			}
		}

		public void ResetTid()
        {
            Tid = 0;
            Last_Timer = Timer_Input.ElapsedMilliseconds;
        }
	}
}