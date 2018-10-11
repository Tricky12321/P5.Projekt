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
                string XYZstring = _serialPort.ReadLine();
                if (XYZstring.Contains("PO") && XYZstring.Contains(":"))
                {
                    XYZstring = XYZstring.Substring(2, XYZstring.Length - 3);

                    var message_split = XYZstring.Split(':');
                    Xx = Convert.ToDouble(message_split[0]);
                    Yy = Convert.ToDouble(message_split[1]);
                    Zz = Convert.ToDouble(message_split[2]);
                }
            }
            catch (TimeoutException) { }
            catch (FormatException) { }
            catch (IndexOutOfRangeException) { }
            return new XYZ(Xx, Yy, Zz);
        }
    }
}