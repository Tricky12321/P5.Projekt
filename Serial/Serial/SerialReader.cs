using System;
using System.Collections.Generic;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Linq;
namespace Serial
{
	public enum ArduinoTypes
	{
		POXYZ,
		INS
	}

	public static class SerialReader
	{

		public static SerialPort GetSerialPort(ArduinoTypes SerialType)
		{
			List<string> SerialPortNames = getSerialPorts().Where(Item => Item.Contains(getSerialPortNames())).ToList();

			List<SerialPort> serialPorts = new List<SerialPort>();

			foreach (var Port in SerialPortNames)
			{
				// Opening all serialPorts
				SerialPort serialPort = new SerialPort(Port, 115200);
				serialPort.ReadTimeout = 500;
				serialPort.WriteTimeout = 500;
				serialPort.Open();

			}

			bool FoundPort = false;

			while (!FoundPort)
			{
				foreach (var currentSerialPort in serialPorts)
				{
					SerialPort serialPort = serialHandler(SerialType, currentSerialPort);
					if (serialPort != null)
					{
						FoundPort = true;
					}
				}
			}

			// Close all opened Serialports
			foreach (var Port in serialPorts)
			{
				Port.Close();

			}
			return null;

		}

		private static SerialPort serialHandler(ArduinoTypes SerialType, SerialPort serialPort)
		{
			string Data = serialPort.ReadLine();
			Console.WriteLine($"Checking {serialPort.PortName}...");
			if (Data == SerialType.ToString())
			{
				serialPort.WriteLine("OK");

				return serialPort;
			} else {
				return null;
            }
		}


		private static List<string> getSerialPorts()
		{
			return new List<string>(SerialPort.GetPortNames());
		}


		private static string getSerialPortNames()
		{
			if (Utilities.IsLinux || Utilities.IsMacOS)
			{
				// UNIX TTY.USBMODEM PORTS
				return "USBMODEM";

			}
			else if (Utilities.IsWindows)
			{
				// Windows COM ports
				return "COM";
			}
			else
			{
				throw new Exception("Unknown OS!");
			}
		}
	}
}
