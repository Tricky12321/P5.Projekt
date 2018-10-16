using System;
using System.Threading;
using Serial;
using System.Collections.Generic;
using System.Linq;
using Serial.DataMapper;

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
            nn = new NeuralNetwork(0.2, new int[] { 100, 50, 50, 3 });
        }

        public void Start()
        {
            running = true;
            trainThread = new Thread(Train);
            trainThread.Start();
        }

        private void Train()
        {
            while (running)
            {
                _dataMapper.StartReading();
                _serialList = _dataMapper.GetDataEntries(1000);
                _dataMapper.StopReading();

                XYZ deltaXYZ = new XYZ(_serialList.Last().INS_Acceleration.X - _serialList.First().INS_Acceleration.X, _serialList.Last().INS_Acceleration.Y - _serialList.First().INS_Acceleration.Y, _serialList.Last().INS_Acceleration.Z - _serialList.First().INS_Acceleration.Z);

                List<double> inputList = new List<double>();
                foreach (DataEntry data in _serialList)
                {
                    inputList.AddRange(data.INS_Acceleration.ToList());
                }

                for (int i = 0; i < 100; i++)
                {
                    nn.Train(inputList, deltaXYZ.ToList());
                    Console.WriteLine("TRAIN!!");
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
