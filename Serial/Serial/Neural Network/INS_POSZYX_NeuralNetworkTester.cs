using System;
using System.Threading;
using Serial;
using System.Collections.Generic;
using System.Linq;
using Serial.DataMapper;
namespace NeuralNetwork
{
	public class INS_POSZYX_NeuralNetworkTester
	{
		//private INSReader _insReader; // Skal ikke bruges mere, bare lav en instans af DataMapper, og så finder den selv ud af det
		//private PozyxReader _posReader; // Skal ikke bruges mere, bare lav en instans af DataMapper, og så finder den selv ud af det
		private DataMapper _dataMapper;
		//private List<Tuple<XYZ, INSDATA>> _serialList;
		private NeuralNetwork nn;

		private Thread trainThread;
		private bool running = true;

		public INS_POSZYX_NeuralNetworkTester()
		{
			nn = new NeuralNetwork(0.2, new int[] { 100, 50, 50, 3 });
			_dataMapper = new DataMapper();
			_dataMapper.StartReading();
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
				var Test = _dataMapper.GetDataEntries().OrderBy(X=>X.entryNum);
				XYZ deltaXYZ = new XYZ(Test.Last().PoZYX.X - Test.First().PoZYX.X, Test.Last().PoZYX.Y - Test.First().PoZYX.Y, Test.Last().PoZYX.Z - Test.First().PoZYX.Z);

				List<double> inputList = new List<double>();
				foreach (DataEntry data in Test)
				{
					inputList.AddRange(data.INS_Accellerometer.ToList());
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
