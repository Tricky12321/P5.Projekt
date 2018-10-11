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
		POZYX,
		INS
	}

	public static class SerialReader
	{

		static List<SerialPort> OpenSerialPorts = new List<SerialPort>();

		public static SerialPort GetSerialPort(ArduinoTypes SerialType)
		{
			List<string> SerialPortNames = getSerialPorts().Where(Item => Item.Contains(getSerialPortNames())).ToList();

			List<SerialPort> serialPorts = new List<SerialPort>();

			foreach (var Port in SerialPortNames)
            {
                try
                {
                    // Opening all serialPorts
                    SerialPort serialPort = new SerialPort(Port, 115200);
                    serialPort.ReadTimeout = 500;
                    serialPort.WriteTimeout = 500;
                    serialPort.Open();
                    serialPorts.Add(serialPort);
                }
                catch (IOException ex)
                {

                }
                catch (UnauthorizedAccessException ex) { }
				
			}

			SerialPort FoundSerialPort = null;
			while (FoundSerialPort == null)
			{
				foreach (var currentSerialPort in serialPorts)
				{
					SerialPort serialPort = serialHandler(SerialType, currentSerialPort);
					if (serialPort != null)
					{
						Console.Clear();
						Console.WriteLine($"Found valid serial port on {serialPort.PortName}");
						FoundSerialPort = serialPort;
                    }
				}
			}

			// Close all opened Serialports
			foreach (var Port in serialPorts)
			{
				if (Port != FoundSerialPort) {
					Port.Close();
                }

			}
			return FoundSerialPort;

		}

		private static SerialPort serialHandler(ArduinoTypes SerialType, SerialPort serialPort)
		{
			try
			{
				string Data = serialPort.ReadLine();
                Console.WriteLine($"Checking {serialPort.PortName}...");
				if (Data == (SerialType.ToString() + "\r")) {
					while (Data == (SerialType.ToString() + "\r") || Data == "\r")
                    {
                        serialPort.WriteLine("DATA OK");
                        Thread.Sleep(1000);
						Data = serialPort.ReadLine();
                    }
					return serialPort;
				}
                else
                {
                    return null;
                }
			}
			catch (TimeoutException ex)
			{
				Console.WriteLine("Timeout... Skipping");
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
				return "usbmodem";

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

		public static void CloseOpenPorts() {
			foreach (var Port in OpenSerialPorts)
			{
				Port.Close();
			}
		}
	}
}
