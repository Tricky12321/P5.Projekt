using System;
using System.Threading;
using Serial;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork
{
    class INS_POSZYX_NeuralNetworkTester
    {
        private INSReader _insReader;
        private PozyxReader _posReader;
        private DataMapper _dataMapper;
        private List<Tuple<XYZ, INSDATA>> _serialList;
        private NeuralNetwork nn;

        private Thread trainThread;
        private bool running = true;

        public INS_POSZYX_NeuralNetworkTester(INSReader insReader, PozyxReader posReader)
        {
            _insReader = insReader;
            _posReader = posReader;
            _dataMapper = new DataMapper(_posReader, _insReader);
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
                _serialList = _dataMapper.ReadToList(1000);
                _dataMapper.StopReading();

                XYZ deltaXYZ = new XYZ(_serialList.Last().Item1.X - _serialList.First().Item1.X, _serialList.Last().Item1.Y - _serialList.First().Item1.Y, _serialList.Last().Item1.Z - _serialList.First().Item1.Z);

                List<double> inputList = new List<double>();
                foreach (Tuple<XYZ,INSDATA> data in _serialList)
                {
                    inputList.AddRange(data.Item2.XYZacc.ToList());
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
