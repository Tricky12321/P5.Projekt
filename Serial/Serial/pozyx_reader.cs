using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    class PozyxReader
    {
        private static SerialPort _serialPort;
        public PozyxReader()
        {
            _serialPort = new SerialPort();
            string[] portNames = SerialPort.GetPortNames();
            while (true)
            {
                foreach (string portName in portNames)
                {
                    Console.WriteLine(portName);
                    _serialPort.PortName = portName;
                    _serialPort.Open();
                    _serialPort.Write("69");//error here
                    _serialPort.Close();
                }
            }
        }

        public XYZ getPosition()
        {
            XYZ position = new XYZ();
            return position;
        }
    }
}