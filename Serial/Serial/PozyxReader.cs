using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace Serial
{
	class PozyxReader : HzCalculator, IReadable<XYZ>
	{
		private string _description = "POZYX";
		private SerialPort _serialPort;

		private XYZ _pozyx_data;

		public XYZ Pozyx_data => _pozyx_data;



		public PozyxReader()
		{
			_serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
			Console.WriteLine($"{_serialPort} Serial Port Opened");
		}

		public XYZ Read()
		{
			try
			{
				string TimerData = _serialPort.ReadLine();
				string XYZstring = _serialPort.ReadLine();
				CheckData(TimerData);
				CheckData(XYZstring);
			}
			catch (TimeoutException) { }
			catch (FormatException) { }
			catch (IndexOutOfRangeException) { }
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
				_pozyx_data = new XYZ(Xx, Yy, Zz);
			}
			else if (data.Contains("timer"))
			{
				HZ_rate = Convert.ToInt32(data.Replace("timer:", "").Replace("\r", ""));
			}
		}
	}
}