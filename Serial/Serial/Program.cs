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

		static PositionCalculator positionCalculator = new PositionCalculator();
                
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
			_serialPort = new SerialPort("/dev/cu.usbmodem14401", 115200);

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
			double xAcc = 0, yAcc = 0, zAcc = 0;



			Stopwatch HzTimer = new Stopwatch();
			DataTimer.Start();
			HzTimer.Start();
			double timer2 = 0;
			while (_continue && DataTimer.ElapsedMilliseconds < 999999999)
			{
				try
				{
					Accelerometer.HandleRawData(_serialPort.ReadLine());
					Console.Clear();
					Accelerometer.PrintXYZKalman();
                    
					timer2 = (double)HzTimer.ElapsedMilliseconds / 1000;
					HzTimer.Reset();
                    HzTimer.Start();

					xAcc += Accelerometer.X / 1000f * 9.82;
					yAcc += Accelerometer.Y / 1000f * 9.82;
					zAcc += Accelerometer.Z / 1000f * 9.82;
                    


					Console.WriteLine(xAcc);
					Console.WriteLine(yAcc);
					Console.WriteLine(zAcc);


					positionCalculator.CalculatePositionFromAccelerometer(new DataPoint(new XYZ(xAcc, yAcc, zAcc), timer2));
					Console.WriteLine($"Timer: {timer2.ToString()}");
                    
					Console.WriteLine($"X: {positionCalculator.CurrentPosition.X}");
					Console.WriteLine($"Y: {positionCalculator.CurrentPosition.Y}");
					Console.WriteLine($"Z: {positionCalculator.CurrentPosition.Z}");	
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
