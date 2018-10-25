using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace Serial.DataMapper
{
	public class DataMapper
	{
		private PozyxReader _pozyx;
		private INSReader _INS;

		private ConcurrentQueue<DataEntry> dataEntries = new ConcurrentQueue<DataEntry>();

		private ConcurrentQueue<DataEntry> avalibleDataEntries => new ConcurrentQueue<DataEntry>(dataEntries.Where(X => X.Used == false));
		public ConcurrentQueue<DataEntry> AllDataEntries => dataEntries;

		public ConcurrentQueue<Tuple<XYZ,XYZ>> KalmanData;

		private bool Reading = false;

		private object _dataEntryLock = new object();
		private XYZ _currentPoZYX = null;

		public bool Kalman = false;

		public Stopwatch Timer;

		public DataMapper()
		{
			Timer = new Stopwatch();
			_INS = new INSReader(Timer);
			_pozyx = new PozyxReader(Timer);
		}

		public void GenerateKalman()
		{
			Kalman = true;
			KalmanData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();

			List<XYZ> Accel = new List<XYZ>();
            List<XYZ> Gyro = new List<XYZ>();
            foreach (var Entry in dataEntries)
			{
				Accel.Add(Entry.INS_Accelerometer);
				Gyro.Add(Entry.INS_Gyroscope);
			}

			Accel = KalmanFilter.KalmanData(Accel);
			Gyro = KalmanFilter.KalmanData(Gyro);
            
			int count = Accel.Count();
			for (int i = 0; i < count; i++)
			{
				KalmanData.Enqueue(new Tuple<XYZ, XYZ>(Accel[i], Gyro[i]));
			}

		}

		public void StartReading()
		{
			ClearEntries();
			_INS.ResetTid();
			_pozyx.ResetTid();
			Reading = true;
			Thread ReadThreadINS = new Thread(ReadINS);
			Thread ReadThreadPOZYX = new Thread(ReadPozyx);
			ReadThreadINS.Start();
			ReadThreadPOZYX.Start();
			Timer.Start();
			Console.WriteLine("Starting data-read in 3 sec!");
			Thread.Sleep(2000);
            
		}

		public void StopReading()
		{
			Reading = false;
			Timer.Stop();
		}


		private void ReadPozyx()
		{
			_pozyx.ResetTid();
			while (Reading)
			{
				XYZ PoZYX_Position = _pozyx.Read();
				lock (_dataEntryLock)
				{
					_currentPoZYX = PoZYX_Position;
				}
			}
		}

		private void ReadINS()
		{
			_INS.ResetTid();
			while (Reading)
			{
				var Output = _INS.Read();
				XYZ Accelerometer = Output.Item1;
				XYZ Gyroscope = Output.Item2;
				DataEntry NewEntry = null; ;
				lock (_dataEntryLock)
				{
					if (_currentPoZYX != null)
					{
						if (Output.Item1.TimeOfData > 1000) {
							NewEntry = new DataEntry(_currentPoZYX, Accelerometer, Gyroscope);
                        }
					}
				}
				if (NewEntry != null)
				{
					dataEntries.Enqueue(NewEntry);
				}
			}
		}

		public void ClearEntries()
		{
			dataEntries = new ConcurrentQueue<DataEntry>();
			_currentPoZYX = null;
		}

		public IEnumerable<DataEntry> GetDataEntries(int amount = 1000)
		{
			if (amount <= avalibleDataEntries.Count())
			{
				var Output = avalibleDataEntries.Take(amount);
				Output.ToList().ForEach(X => X.Used = true);
				return Output;
			}
			else
			{
				throw new TooManyDataEntriesRequestedException($"There is not this many DataEntries that can be requested.\nThere is only {avalibleDataEntries.Count()} avalible!");
			}
		}

	}
}
