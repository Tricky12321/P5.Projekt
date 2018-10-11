using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial 
{
    class PozyxReader : IReadable<XYZ>
    {
        private string _description = "POZYX";
        private SerialPort _serialPort;

		private XYZ _pozyx_data;

        public PozyxReader()
        {
            Console.WriteLine($"Getting {_description} Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
            Console.WriteLine($"Opening {_description} Serial Port");
            Console.WriteLine($"{_description} Serial Port Opened");
        }

        public XYZ Read()
        {
            double Xx = -1;
            double Yy = -1;
            double Zz = -1;

            try
            {
				string data = _serialPort.ReadLine();
                string XYZstring = _serialPort.ReadLine();
                


            }
            catch (TimeoutException) { }
            catch (FormatException) { }
            catch (IndexOutOfRangeException) { }
			return _pozyx_data);
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
            else if (data.Contains("AC") && data.Contains(":"))
            {
                data = data.Substring(2, data.Length - 3);

                var message_split = data.Split(':');
                XAC = Convert.ToDouble(message_split[0]);
                YAC = Convert.ToDouble(message_split[1]);
                ZAC = Convert.ToDouble(message_split[2]);

            }
        }
    }
}