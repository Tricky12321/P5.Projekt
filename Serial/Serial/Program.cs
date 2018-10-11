using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Serial
{
	class MainClass
	{

		static bool _continue = true;
		static SerialPort _serialPort;
		//static Thread readThread = new Thread(Read);
		static Stopwatch DataTimer = new Stopwatch();
		static DataClass Accelerometer = new DataClass("AC");
		static DataClass Gyroscope = new DataClass("GY");
		static int Timer = 1000000;

		static PositionCalculator positionCalculator = new PositionCalculator();


		static INS_reader iNS_Reader;
		public static void Main()
		{
			/*
            Console.WriteLine($"Creating POZYX.");
            PozyxReader Pozyx = new PozyxReader();
            Console.WriteLine($"Reading POZYX.");

            SerialPort Test = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
			Console.WriteLine($"Serialport found: {Test.PortName}");
            */
			iNS_Reader = new INS_reader();
			Thread PrintThread = new Thread(Print);
			PrintThread.Start();
			PrintThread.Join();
			Console.ReadLine();

		}

		public static void Print()
		{
			while (true)
			{
				iNS_Reader.Read();
				Console.Clear();
				Console.WriteLine($"INS READER");
				Console.WriteLine($"Aceelerometer data");
				Console.WriteLine(iNS_Reader.AcceXYZ);
				Console.WriteLine($"Gyro data");
				Console.WriteLine(iNS_Reader.GyroXYZ);
				Console.WriteLine($"HZ:{iNS_Reader.HZ_rate}");
			}
		}
	}
}
==== BASE ====
