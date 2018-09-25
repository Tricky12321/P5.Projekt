using System;
using System.IO.Ports;
using System.Threading;
namespace Serial
{
    class MainClass
    {

		static bool _continue = true;
        static SerialPort _serialPort;
		static Thread readThread = new Thread(Read);
        public static void Main()
        {
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;



			ConnectToCom();
            Console.WriteLine("Type QUIT to exit");

            while (_continue)
            {
                message = Console.ReadLine();
                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
            }

            _serialPort.Close();
        }

		public static void ConnectToCom() {
			

			// Create a new SerialPort object with default settings.
            _serialPort = new SerialPort("/dev/cu.usbmodem1411", 115200);

            // Allow the user to set the appropriate properties.

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _continue = true;
            readThread.Start();
			readThread.Join();
            
		}

        public static void Read()
        {
			DataClass Accelerometer = new DataClass();
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
					if (message.Contains("AC") && message.Contains(":")) {
						message = message.Substring(2, message.Length - 3);

                        var message_split = message.Split(':');
                        int X = Convert.ToInt32(message_split[0]);
                        int Y = Convert.ToInt32(message_split[1]);
                        int Z = Convert.ToInt32(message_split[2]);
						Accelerometer.NewNum();
						//Accelerometer.UpdateX(X);
						//Accelerometer.UpdateY(Y);
						//Accelerometer.UpdateZ(Z);
                        Console.Clear();
                        Console.WriteLine($"X: {X}");
                        Console.WriteLine($"Y: {Y}");
                        Console.WriteLine($"Z: {Z}");
						Console.WriteLine($"FILTER");
						//Console.WriteLine($"X: {Accelerometer.GetX()}");
						//Console.WriteLine($"Y: {Accelerometer.GetY()}");
						//Console.WriteLine($"Z: {Accelerometer.GetZ()}");
						Console.WriteLine($"HZ: {Accelerometer.GetHz()}");
					}
                }
                catch (TimeoutException) { }
				catch (FormatException) {
					Console.Clear();
					Console.WriteLine("Data error");
				
				}
            }
        }


    }
}
