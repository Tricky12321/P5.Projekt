using System;
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
        static DataClass Accelerometer = new DataClass("AC");
        static DataClass Gyroscope = new DataClass("GY");
        static int Timer = 5000;

        static PositionCalculator positionCalculator = new PositionCalculator();

        

		public static void Main()
		{

            /*
             * SerialPort Test = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
			Console.WriteLine($"Serialport found: {Test.PortName}");
			Console.ReadLine();
            */
            /*ConnectToCom();
			Console.WriteLine("Type QUIT to exit");

			while (DataTimer.ElapsedMilliseconds < Timer)
			{

			}
			_continue = false;
			_serialPort.Close();*/
            INSReader reader = new INSReader();
            while (true)
            {
                reader.Read();
            }
		}

        public static void ConnectToCom()
        {


            // Detects USB modem ports, to find arduino sheilds
            List<string> allPorts = new List<string>(SerialPort.GetPortNames());
            string Port = allPorts.Find(PortName => PortName.Contains("usbmodem"));
            if (Port == "")
            {
                Console.WriteLine("No Serial ports found!");
                throw new Exception("No Serial ports found");
            }
            else
            {
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
            //Gyroscope.SetInput(_serialPort);
            Accelerometer.Calibrate();
            //Gyroscope.Calibrate();
            StartReading();

        }

        public static void StartReading()
        {
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
                    string Data = _serialPort.ReadLine();
                    Accelerometer.HandleRawData(Data);
                    //Gyroscope.HandleRawData(Data);
                    Console.Clear();
                    Accelerometer.PrintXYZ();
                    Accelerometer.SnapData();
                    //Gyroscope.PrintXYZ();
                    CalculateKalman(Gyroscope.GetXYZ(), Accelerometer.GetXYZ());
                    //Gyroscope.SnapData();
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

        public static void CalculateKalman(XYZ Gyro, XYZ Accelerometer)
        {

			double Accelval1 = Math.Sqrt(Math.Pow(Accelerometer.X, 2) + Math.Pow(Accelerometer.Z, 2));
			double Accelval2 = Math.Sqrt(Math.Pow(Accelerometer.Y, 2) + Math.Pow(Accelerometer.Z, 2));
			double Accelval3 = Math.Sqrt(Math.Pow(Accelerometer.X, 2) + Math.Pow(Accelerometer.Z, 2));
			double Pitch = Math.Atan((Accelerometer.Y) / (Accelval1)) * (180 / Math.PI);
			double Roll = Math.Atan((Accelerometer.X) / (Accelval2)) * (180 / Math.PI);
			double Yaw = Math.Atan((Accelerometer.Z) / (Accelval3)) * (180 / Math.PI);
				/*
			double Gyroval = Math.Sqrt(Math.Pow(Gyro.X, 2) + Math.Pow(Gyro.Z, 2));
			double Gyroval2 = Math.Sqrt(Math.Pow(Gyro.Y, 2) + Math.Pow(Gyro.Z, 2));
			double Pitch = Math.Atan((Gyro.Y) / (Gyroval)) * (180 / Math.PI);
			double GyroO = Math.Atan((Gyro.X) / (Gyroval2)) * (180 / Math.PI);
*/


        }
    }

}