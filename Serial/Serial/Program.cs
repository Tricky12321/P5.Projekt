﻿using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
namespace Serial
{
	class MainClass
	{

		static bool _continue = true;
		static SerialPort _serialPort;
		static Thread readThread = new Thread(Read);
		static Stopwatch DataTimer = new Stopwatch();
		static DataClass Accelerometer = new DataClass();
		static int Timer = 5000;

		static PositionCalculator positionCalculator = new PositionCalculator();
                



		public static void Main()
		{

			ConnectToCom();
			Console.WriteLine("Type QUIT to exit");

			while (DataTimer.ElapsedMilliseconds < Timer)
			{

			}
			_continue = false;
			_serialPort.Close();
		}

		public static void ConnectToCom()
		{


            // Detects USB modem ports, to find arduino sheilds
			List<string> allPorts = new List<string>(SerialPort.GetPortNames());
			string Port = allPorts.Find(PortName => PortName.Contains("usbmodem"));
			if (Port == "") {
				Console.WriteLine("No Serial ports found!");
				throw new Exception("No Serial ports found");
			} else{
				_serialPort = new SerialPort(Port, 115200);
				Console.WriteLine($"Found serial port: {Port}");
            }
			Thread.Sleep(2000);
			// Allow the user to set the appropriate properties.

			// Set the read/write timeouts
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;

			_serialPort.Open();
			_continue = true;
			Accelerometer.SetInput(_serialPort);
			Accelerometer.Calibrate();
			StartReading();

		}

		public static void StartReading() {
			readThread.Start();
            readThread.Join();
		}

		public static void Read()
		{
			DataTimer.Reset();
			DataTimer.Start();
			while (_continue && DataTimer.ElapsedMilliseconds < Timer)
			{
				try
				{
					Accelerometer.HandleRawData(_serialPort.ReadLine());
					Console.Clear();
					Accelerometer.PrintXYZKalmanLowpass();
					Accelerometer.PrintXYZSpeed();
					Accelerometer.SnapData();
					Console.WriteLine($"Timer: {DataTimer.ElapsedMilliseconds}");
				}
				catch (TimeoutException) { }
				catch (FormatException)
				{
					Console.Clear();
					Console.WriteLine("Data error");
				}
			}
			DataTimer.Stop();
			_continue = false;
			Accelerometer.WriteData();
			Console.Clear();
			Console.WriteLine("Wrote Data to CSV");
		}

	}
}
