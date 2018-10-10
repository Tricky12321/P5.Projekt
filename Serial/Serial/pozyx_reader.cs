using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial 
{
    class PozyxReader //: IReadable<XYZ>
    {
        private String _description = "POZYX";
        private SerialPort _serialPort;
        
        public PozyxReader()
        {
            Console.WriteLine($"Getting {_description} Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
            Console.WriteLine($"Opening {_description} Serial Port");
            _serialPort.Open();
            Console.WriteLine($"{_description} Serial Port Opened");
        }


        public string Read()
        {
            return _serialPort.ReadLine();
        }
    }
}