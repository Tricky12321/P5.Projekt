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


		static List<CustomSerialPort> OpenSerialPorts = new List<CustomSerialPort>();
		static List<CustomSerialPort> serialPorts = new List<CustomSerialPort>();
        
		private static void FindSerialPorts() {
            if (serialPorts.Count == 0)
            {
				List<string> SerialPortNames = getSerialPorts().Where(Item => Item.Contains(getSerialPortNames())).ToList();
                foreach (var Port in SerialPortNames)
                {
                    try
                    {
                        CustomSerialPort serialPort = new CustomSerialPort(Port, 115200, Parity.None, 8, StopBits.One); ;
                        serialPort.ReadTimeout = 500;
                        serialPort.WriteTimeout = 500;
                        serialPorts.Add(serialPort);
                    }
                    catch (IOException) {
						Console.WriteLine($"[{Port}] Invalid Serialport");

					}
                    catch (UnauthorizedAccessException) {
						Console.WriteLine($"[{Port}] Unauthorized Access");
					}
                }
            }

		}

		public static CustomSerialPort GetSerialPort(ArduinoTypes SerialType)
		{
			FindSerialPorts();
			List<CustomSerialPort> WorkingPorts = serialPorts.Where(x => x.PortOpen == false && x.IsMatched == false).ToList();
			foreach (var ClosedPort in WorkingPorts)
			{
				ClosedPort.Open();
				ClosedPort.PortOpen = true;
			}

			CustomSerialPort FoundSerialPort = SearchForSerialPorts(SerialType);

			// Close all opened Serialports
			WorkingPorts = serialPorts.Where(x => x.PortOpen == false && x.IsMatched == false).ToList();
			foreach (var Port in WorkingPorts)
			{
				if (Port != FoundSerialPort)
				{
					Port.PortOpen = false;
					Port.Close();
				}
			}

			return FoundSerialPort;

		}

		private static CustomSerialPort SearchForSerialPorts(ArduinoTypes SerialType ) {
			CustomSerialPort FoundSerialPort = null;
            while (FoundSerialPort == null)
            {
                foreach (var currentSerialPort in serialPorts)
                {
                    if (FoundSerialPort == null)
                    {
                        CustomSerialPort serialPort = serialHandler(SerialType, currentSerialPort);
                        if (serialPort != null)
                        {
                            FoundSerialPort = serialPort;
                            OpenSerialPorts.Add(FoundSerialPort);

                        }
                    }
                }
            }
			FoundSerialPort.IsMatched = true;
			return FoundSerialPort;
		}


		private static CustomSerialPort serialHandler(ArduinoTypes SerialType, CustomSerialPort serialPort)
		{
			try
			{
				string Data = serialPort.ReadLine();
				Console.WriteLine($"[{serialPort.PortName}] Checking... ({SerialType.ToString()})");
				Console.WriteLine($"DATA: {Data}");
				if (Data.Contains(SerialType.ToString()))
				{
					Console.WriteLine($"[{serialPort.PortName}] Matched!");
					return serialPort;
				}
				else
				{
					return null;
				}
			}
			catch (TimeoutException)
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

		public static void CloseOpenPorts()
		{
			foreach (var Port in OpenSerialPorts)
			{
				Port.Close();
			}
		}
	}
}
