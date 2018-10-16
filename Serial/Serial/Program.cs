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
		//static Thread readThread = new Thread(Read);
		static INSReader InsReader;
		static PozyxReader PozyxReader;
		public static void Main()
		{
			INS_POSZYX_NeuralNetworkTester nn = new INS_POSZYX_NeuralNetworkTester(InsReader, PozyxReader);
            		nn.Start();
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
