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
        private static SerialPort _serialPort;
        public PozyxReader()
        {
            Console.WriteLine($"Getting POZYX Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
            Console.WriteLine($"Opening POZYX Serial Port");
            _serialPort.Open();
            Console.WriteLine($"POZYX Serial Port Opened");
        }

        public XYZ Read()
        {
            _serialPort.Open();

            _serialPort.Close();
            return new XYZ();
        }
    }
}