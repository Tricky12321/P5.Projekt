using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Serial
{
    class INSReader : IReadable<INSDATA>
    {
        private SerialPort _serialPort;
        private double XGY;
        private double YGY;
        private double ZGY;
        private double XAC;
        private double YAC;
        private double ZAC;

        public INSReader()
        {
            Console.WriteLine($"Getting INS Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.INS);
            Console.WriteLine($"Opening INS Serial Port");
            Console.WriteLine($"INS Serial Port Opened");
        }

        public INSDATA Read()
        {
            string data1 = _serialPort.ReadLine();
            string data2 = _serialPort.ReadLine();
            CheckData(data1);
            CheckData(data2);

            return new INSDATA(new XYZ(XAC, YAC, ZAC), new XYZ(XGY, YGY, ZGY));
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
            }
            catch (TimeoutException) { }
            catch (FormatException) { }
            catch (IndexOutOfRangeException) { }
        }
    }
}
