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
        static Stopwatch DataTimer = new Stopwatch();
        static DataClass Accelerometer = new DataClass("AC");
        static DataClass Gyroscope = new DataClass("GY");

        static PositionCalculator positionCalculator = new PositionCalculator();
              
		public static void Main()
		{
			handler = new ConsoleEventDelegate(ConsoleEventCallback);
			if (Utilities.IsLinux || Utilities.IsMacOS) {
				Console.CancelKeyPress += delegate {
					SerialReader.CloseOpenPorts();
                };
			} else if (Utilities.IsWindows) {
				SetConsoleCtrlHandler(handler, true);
            }
            Console.WriteLine($"Creating POZYX.");
            PozyxReader Pozyx = new PozyxReader();
            Console.WriteLine($"Reading POZYX.");

            while (true)
            {
                Console.WriteLine(Pozyx.Read().ToString());
            }
        }
		static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
				SerialReader.CloseOpenPorts();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

    }
}