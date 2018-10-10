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
        //static Thread readThread = new Thread(Read);
        static Stopwatch DataTimer = new Stopwatch();
        static DataClass Accelerometer = new DataClass("AC");
        static DataClass Gyroscope = new DataClass("GY");
        static int Timer = 1000000;

        static PositionCalculator positionCalculator = new PositionCalculator();

        

		public static void Main()
		{
            

            Console.WriteLine($"Creating POZYX.");
            PozyxReader Pozyx = new PozyxReader();
            Console.WriteLine($"Reading POZYX.");

            while (true)
            {
                Console.WriteLine(Pozyx.Read().ToString());
            }
        }
    }
}