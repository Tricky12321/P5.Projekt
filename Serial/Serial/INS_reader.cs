using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Serial
{
    class INS_reader : IReadable<INSDATA>
    {
        private SerialPort _serialPort;
        private string _dataSet;
        private double XGY;
        private double YGY;
        private double ZGY;
        private double XAC;
        private double YAC;
        private double ZAC;

        public INS_reader()
        {
            Console.WriteLine($"Getting INS Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.INS);
            Console.WriteLine($"Opening INS Serial Port");
            _serialPort.Open();
            Console.WriteLine($"INS Serial Port Opened");
        }

        public INSDATA Read()
        {
            _dataSet = _serialPort.ReadLine();
            string data2 = _serialPort.ReadLine();
            try
            {
                if (_dataSet.Contains("GY") && _dataSet.Contains(":"))
                {
                    _dataSet = _dataSet.Substring(2, _dataSet.Length - 3);

                    var message_split = _dataSet.Split(':');
                    XGY = Convert.ToDouble(message_split[0]);
                    YGY = Convert.ToDouble(message_split[1]);
                    ZGY = Convert.ToDouble(message_split[2]);

                }
            }
            catch (TimeoutException) { }
            catch (FormatException) { }
            catch (IndexOutOfRangeException) { }

            try
            {
                if (_dataSet.Contains("AC") && _dataSet.Contains(":"))
                {
                    _dataSet = _dataSet.Substring(2, _dataSet.Length - 3);

                    var message_split = _dataSet.Split(':');
                    XAC = Convert.ToDouble(message_split[0]);
                    YAC = Convert.ToDouble(message_split[1]);
                    ZAC = Convert.ToDouble(message_split[2]);

                }
            }
            catch (TimeoutException) { }
            catch (FormatException) { }
            catch (IndexOutOfRangeException) { }

            _dataSet += "test" + _serialPort.ReadLine();
            Console.WriteLine(_dataSet);
            return new INSDATA();
        }
    }
}
