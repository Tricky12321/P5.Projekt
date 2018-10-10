﻿using System;
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
            Console.WriteLine(_serialPort.ReadLine());
            return new INSDATA();
        }
    }
}
