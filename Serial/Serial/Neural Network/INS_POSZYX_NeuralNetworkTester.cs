using System;
using System.Threading;

namespace NeuralNetwork
{
    public class INS_POSZYX_NeuralNetworkTester
    {
        public INSReader insReader = new INSReader();
        public PozyxReader posReader = new PozyxReader();

        public INS_POSZYX_NeuralNetworkTester()
        {
        }

        public void Start(){

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
