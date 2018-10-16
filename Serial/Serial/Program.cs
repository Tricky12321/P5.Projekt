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

		//static Thread readThread = new Thread(Read);
		static INSReader InsReader;
		static PozyxReader PozyxReader;
		public static void Main()
		{
			InsReader = new INSReader();
			PozyxReader = new PozyxReader();
			Console.Clear();
			Console.WriteLine("Waiting for Sensors to start Writing!");
			Thread.Sleep(5000);
			Thread ReadThreadINS = new Thread(ReadINS);
			Thread ReadThreadPOZYX = new Thread(ReadPozyx);
			ReadThreadINS.Start();
			ReadThreadPOZYX.Start();

			Thread PrintThread = new Thread(Print);
			PrintThread.Start();
			PrintThread.Join();
			Console.ReadLine();

		}

		public static void ReadPozyx() {
			while (true)
			{
				PozyxReader.Read();
			}
		}

		public static void ReadINS() {
			while (true)
			{
				InsReader.Read();
			}
		}

		public static void Print()
		{
			while (true)
			{
                
				Console.Clear();
                // INS
				Console.WriteLine($"INS READER");
                Console.WriteLine($"Aceelerometer data");
                Console.WriteLine(InsReader.AcceXYZ);
                Console.WriteLine($"Gyro data");
                Console.WriteLine(InsReader.GyroXYZ);
				Console.WriteLine($"INS HZ:{InsReader.HZ_rate}");

                // POZYX
				Console.WriteLine($"POZYX READER");
				Console.WriteLine($"{PozyxReader.Pozyx_data}");
                Console.WriteLine($"POZYX HZ:{PozyxReader.HZ_rate}");
				Thread.Sleep(10);
			}
		}
	}
}
