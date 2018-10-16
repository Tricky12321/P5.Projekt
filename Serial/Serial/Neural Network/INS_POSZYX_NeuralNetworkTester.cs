using System;
using System.Threading;
using Serial;
using System.Collections.Generic;
using System.Linq;
using Serial.DataMapper;
using System.Diagnostics;
namespace NeuralNetwork
{
    class INS_POSZYX_NeuralNetworkTester
    {
        private DataMapper _dataMapper;
        private IEnumerable<DataEntry> _serialList;
        private NeuralNetwork nn;

        private Thread trainThread;
        private bool running = true;

        public INS_POSZYX_NeuralNetworkTester()
        {
            _dataMapper = new DataMapper();
            nn = new NeuralNetwork(0.2, new int[] { 3000, 100, 100, 3 });
        }

        public void Start()
        {
            running = true;
            trainThread = new Thread(Train);
            trainThread.Start();
            trainThread.Join();
        }

        private void Train()
        {
            while (running)
            {
                try
                {
                    _serialList = _dataMapper.GetDataEntries(1000);

                    XYZ deltaXYZ = new XYZ(_serialList.Last().INS_Accelerometer.X - _serialList.First().INS_Accelerometer.X, _serialList.Last().INS_Accelerometer.Y - _serialList.First().INS_Accelerometer.Y, _serialList.Last().INS_Accelerometer.Z - _serialList.First().INS_Accelerometer.Z);

                    List<double> inputList = new List<double>();
                    foreach (DataEntry data in _serialList)
                    {
                        inputList.AddRange(data.INS_Accelerometer.GetNormalizedList());
                    }
                    Console.WriteLine("TRAIN!!");
                    Stopwatch Timer = new Stopwatch();
                    Timer.Start();
                    for (int i = 0; i < 1000; i++)
                    {
                        nn.Train(inputList, deltaXYZ.GetNormalizedList(1000));
                    }
                    Timer.Stop();
                    Console.WriteLine($"DONE TRAINING!!{Timer.ElapsedMilliseconds}MS");

                }
                catch (TooManyDataEntriesRequestedException)
                {
                    Console.WriteLine("No data!");
                    Thread.Sleep(1000);
                }

            }
        }

        public void Stop()
        {
            running = false;
            trainThread.Abort();
        }
    }
}
