using System;
using System.IO.Ports;
namespace Serial
{
	public class CustomSerialPort : SerialPort
	{
		public bool PortOpen = false;
		public bool IsMatched = false;

		public CustomSerialPort(string PortName, int baudRate, Parity parity) : base(PortName, baudRate, parity)
		{

		}

		public CustomSerialPort(string PortName) : base(PortName)
		{

		}

		public CustomSerialPort(string PortName, int baudRate, Parity parity, int databits, StopBits stopBits) : base(PortName, baudRate, parity, databits, stopBits)
        {

        }

	}
}
