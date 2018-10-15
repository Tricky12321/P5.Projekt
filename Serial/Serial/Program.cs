using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NeuralNetwork;

namespace Serial
{
	class MainClass
	{ 
		static bool _continue = true;
		static SerialPort _serialPort;
		static Stopwatch DataTimer = new Stopwatch();
		static DataClass Accelerometer = new DataClass("AC");
		static DataClass Gyroscope = new DataClass("GY");
		static int Timer = 1000000;

		static PositionCalculator positionCalculator = new PositionCalculator();


        static INSReader insReader = new INSReader();
		public static void Main()
		{
            insReader = new INSReader();
            PozyxReader posReader = new PozyxReader();
            DataMapper dataMapper = new DataMapper(posReader, insReader);

            dataMapper.StartReading();
            Thread.Sleep(1000);
            var list = dataMapper.ReadToList(100);
            dataMapper.StopReading();

			Thread PrintThread = new Thread(Print);
			PrintThread.Start();
			PrintThread.Join();
			Console.ReadLine();
		}

		public static void Print()
		{
			while (true)
			{
				insReader.Read();
				Console.Clear();
				Console.WriteLine($"INS READER");
				Console.WriteLine($"Aceelerometer data");
				Console.WriteLine(insReader.AcceXYZ);
				Console.WriteLine($"Gyro data");
				Console.WriteLine(insReader.GyroXYZ);
				Console.WriteLine($"HZ:{insReader.HZ_rate}");
			}
		}
	}
}
