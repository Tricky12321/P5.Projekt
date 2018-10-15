using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;

namespace Serial
{
    class INSReader : IReadable<INSDATA>
    {
        private SerialPort _serialPort;
        public XYZ AcceXYZ
        {
            get
            {
                return new XYZ(XAC, YAC, ZAC);
            }
        }

        public XYZ GyroXYZ
        {
            get
            {
                return new XYZ(XGY, YGY, ZGY);
            }
        }

        private double XGY;
        private double YGY;
        private double ZGY;

        private double XAC;
        private double YAC;
        private double ZAC;
        private const int _hz_log_count = 100;
        private Queue<double> _hz_rate_log = new Queue<double>();
        public double HZ_rate
        {
            get
            {
                return Math.Round(_hz_rate_log.Average(), 0);
            }
            set
            {
                _hz_rate_log.Enqueue(1000 / value);
                _hz_rate_log.Dequeue();
            }
        }


        public INSReader()
        {
            for (int i = 0; i < _hz_log_count; i++)
            {
                _hz_rate_log.Enqueue(0);
            }


            Console.WriteLine($"Getting INS Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.INS);
            Console.WriteLine($"Opening INS Serial Port");
            Console.WriteLine($"INS Serial Port Opened");
        }

        public INSDATA Read()
        {
            string data1 = _serialPort.ReadLine();
            string data2 = _serialPort.ReadLine();
            string data3 = _serialPort.ReadLine();
            CheckData(data1);
            CheckData(data2);
            CheckData(data3);

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
                else if (data.Contains("timer"))
                {
                    HZ_rate = Convert.ToInt32(data.Replace("timer:", "").Replace("\r", ""));
                }

            }
            catch (TimeoutException) { }
            catch (FormatException) { }
            catch (IndexOutOfRangeException) { }
        }
    }
}
