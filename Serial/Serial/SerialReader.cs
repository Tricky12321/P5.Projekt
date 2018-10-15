using System;
using System.Collections.Generic;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Diagnostics;
namespace Serial
{
	public enum ArduinoTypes
	{
		POZYX,
		INS
	}

	public static class SerialReader
	{

		const int _timeoutMS = 20000;

		static List<SerialPort> OpenSerialPorts = new List<SerialPort>();

		public static SerialPort GetSerialPort(ArduinoTypes SerialType)
		{
			Stopwatch Timeout = new Stopwatch();

			List<string> SerialPortNames = getSerialPorts().Where(Item => Item.Contains(getSerialPortNames())).ToList();

			List<SerialPort> serialPorts = new List<SerialPort>();

			foreach (var Port in SerialPortNames)
            {
                try
                {
					// Check if the port is already opened
					bool AlreadyOpen = false;
					foreach (var OpenPort in OpenSerialPorts)
					{
						if (OpenPort.PortName == Port) {
							AlreadyOpen = true;
						}
					}
					// Opening all serialPorts
					if (!AlreadyOpen) {
						SerialPort serialPort = new SerialPort(Port, 115200);
						serialPort.ReadTimeout = 1000;
						serialPort.WriteTimeout = 1000;
						serialPort.Open();
						serialPorts.Add(serialPort);
						Thread.Sleep(100);
                    }
                }
                catch (IOException ex)
                {

                }
                catch (UnauthorizedAccessException ex) { }
				
			}

			SerialPort FoundSerialPort = null;
			Timeout.Start();
			while (FoundSerialPort == null)
			{
				foreach (var currentSerialPort in serialPorts)
				{
					SerialPort serialPort = serialHandler(SerialType, currentSerialPort);
					if (serialPort != null)
					{
						Console.Clear();
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
			OpenSerialPorts.Add(FoundSerialPort);


			return FoundSerialPort;

		}

		private static SerialPort serialHandler(ArduinoTypes SerialType, SerialPort serialPort)
		{
			try
			{
				string Data = serialPort.ReadLine();
				Console.WriteLine($"[{serialPort.PortName}] Checking... ({SerialType.ToString()})");
				Console.WriteLine($"DATA: {Data}");
				if (Data.Contains(SerialType.ToString())) {
					Console.WriteLine($"[{serialPort.PortName}] Matched! ({Data})");
					Console.WriteLine($"[{serialPort.PortName}] Starting...!");
					serialPort.WriteLine("DATA OK");
					return serialPort;
				}
                else
                {
                    return null;
                }
			}
			catch (TimeoutException ex)
			{
				Console.WriteLine($"[{serialPort.PortName}] Timeout... Skipping");
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
