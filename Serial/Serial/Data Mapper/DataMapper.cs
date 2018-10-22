using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
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

		public DataMapper()
		{
			_INS = new INSReader();
			_pozyx = new PozyxReader();
			Console.Clear();
			Console.WriteLine("Waiting for Sensors to start Writing!");
			Thread.Sleep(5000);
			Thread ReaderThread = new Thread(StartReading);
			ReaderThread.Start();
		}

		public void GenerateKalman()
		{
			Kalman = true;
			KalmanData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
			KalmanFilter X_accel = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
			KalmanFilter Y_accel = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
			KalmanFilter Z_accel = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);

			KalmanFilter X_gyro = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
            KalmanFilter Y_gyro = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
            KalmanFilter Z_gyro = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
            
            foreach (var Entry in dataEntries)
			{
				double x_accel_kalman = X_accel.Output(Entry.INS_Accelerometer.X);
				double y_accel_kalman = Y_accel.Output(Entry.INS_Accelerometer.Y);
				double z_accel_kalman = Z_accel.Output(Entry.INS_Accelerometer.Z);

				double x_gyro_kalman = X_gyro.Output(Entry.INS_Gyroscope.X);
				double y_gyro_kalman = Y_gyro.Output(Entry.INS_Gyroscope.Y);
				double z_gyro_kalman = Z_gyro.Output(Entry.INS_Gyroscope.Z);

				XYZ Gyro = new XYZ(x_gyro_kalman, y_gyro_kalman, z_gyro_kalman);
				XYZ Accel = new XYZ(x_accel_kalman, y_accel_kalman, z_accel_kalman);
				KalmanData.Enqueue(new Tuple<XYZ, XYZ>(Accel, Gyro));
			}

		}

		public void StartReading()
		{
			Reading = true;
			Thread ReadThreadINS = new Thread(ReadINS);
			Thread ReadThreadPOZYX = new Thread(ReadPozyx);
			ReadThreadINS.Start();
			ReadThreadPOZYX.Start();
		}

		public void StopReading()
		{
			Reading = false;
		}


		private void ReadPozyx()
		{
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
						NewEntry = new DataEntry(_currentPoZYX, Accelerometer, Gyroscope);
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
