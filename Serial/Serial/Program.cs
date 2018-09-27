using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
namespace Serial
{
	class MainClass
	{

		static bool _continue = true;
		static SerialPort _serialPort;
		static Thread readThread = new Thread(Read);
		static Stopwatch DataTimer = new Stopwatch();
		static DataClass Accelerometer = new DataClass();
        
		public static void Main()
		{
			StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

			ConnectToCom();
			Console.WriteLine("Type QUIT to exit");


			while (DataTimer.ElapsedMilliseconds < 99999999999)
			{

			}
			DataTimer.Stop();
			_continue = false;
			_serialPort.Close();
		}

		public static void ConnectToCom()
		{


			// Create a new SerialPort object with default settings.
			_serialPort = new SerialPort("/dev/cu.usbmodem1421", 115200);

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
			DataTimer.Start();
			while (_continue && DataTimer.ElapsedMilliseconds < 999999999)
			{
				try
				{
					Accelerometer.HandleRawData(_serialPort.ReadLine());
					Console.Clear();
					Accelerometer.PrintXYZ();
					Accelerometer.PrintXYZKalman();
					Accelerometer.PrintHz();
					var TimerString = Math.Round((double)(DataTimer.ElapsedMilliseconds / 1000), 0).ToString();
					Console.WriteLine($"Timer: {TimerString}");
				}
				catch (TimeoutException) { }
				catch (FormatException)
				{
					Console.Clear();
					Console.WriteLine("Data error");

				}
			}
			_continue = false;
			Accelerometer.WriteToCSV();
			Console.Clear();
			Console.WriteLine("Wrote Data to CSV");
		}


	}
}
