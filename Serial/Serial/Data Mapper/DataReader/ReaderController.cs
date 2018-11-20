using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Linq;
namespace Serial.DataMapper.DataReader
{
	public class ReaderController
	{
		PozyxReader _pozyx;
		INSReader _INS;

		ConcurrentQueue<DataEntry> dataEntries = new ConcurrentQueue<DataEntry>();

		ConcurrentQueue<DataEntry> avalibleDataEntries => new ConcurrentQueue<DataEntry>(dataEntries.Where(X => X.Used == false));
		public ConcurrentQueue<DataEntry> AllDataEntries => dataEntries;

		public ConcurrentQueue<Tuple<XYZ, XYZ>> KalmanData;
		public ConcurrentQueue<Tuple<XYZ, XYZ>> RollingAverageData;

		public ConcurrentQueue<DataEntry> UnCalibrated = new ConcurrentQueue<DataEntry>();

		public ConcurrentQueue<DataEntry> SegmentedData = new ConcurrentQueue<DataEntry>();

		bool Reading;

		object _dataEntryLock = new object();
		XYZ _currentPoZYX;

		public bool Kalman;
		public bool RollingAverageBool;

		public Stopwatch Timer;

		bool Pozyx;
		bool Ins;

		public bool Calibrated => _INS.IsCalibrated();


		public ReaderController(bool Pozyx = true, bool Ins = true)
		{
			Timer = new Stopwatch();
			this.Pozyx = Pozyx;
			this.Ins = Ins;
			if (Pozyx)
			{
				_pozyx = new PozyxReader(Timer);
			}
			if (Ins)
			{
				_INS = new INSReader(Timer);
			}
		}

		public void StartReading()
		{
			if (Reading == true)
			{
				Console.WriteLine("ALREADY READING....");
				Console.WriteLine("ALREADY READING....");
				Console.WriteLine("ALREADY READING....");
				Console.WriteLine("ALREADY READING....");
				Console.WriteLine("ALREADY READING....");
			}
			else
			{

				Reading = true;
				if (Pozyx)
				{
					_pozyx.ResetTid();
					Thread ReadThreadPOZYX = new Thread(ReadPozyx);
					ReadThreadPOZYX.Start();
				}

				if (Ins)
				{
					_INS.ResetTid();
					Thread ReadThreadINS = new Thread(ReadINS);
					ReadThreadINS.Start();
				}
				Thread.Sleep(1000);
				Console.WriteLine("Started...");
				ClearEntries();

				Timer.Start();
			}
		}

		public void StopReading()
		{
			Reading = false;
			Timer.Stop();
		}

		void ReadPozyx()
		{
			_pozyx.ResetTid();
			while (Reading)
			{
				XYZ PoZYX_Position = _pozyx.Read();
				lock (_dataEntryLock)
				{
					if (Ins == false)
					{
						DataEntry NewEntry;
						NewEntry = new DataEntry(_currentPoZYX, null, null, 0);
						dataEntries.Enqueue(NewEntry);
					}
					else
					{
						_currentPoZYX = PoZYX_Position;
					}
				}
			}
		}

		void ReadINS()
		{
			_INS.ResetTid();
			while (Reading)
			{
				var Output = _INS.Read();
				double Angle = Output.Item3;

				DataEntry NewEntry = null;
				DataEntry NewNonCaliEntry = null;
				lock (_dataEntryLock)
				{
					if (Pozyx == false)
					{
						NewEntry = new DataEntry(null, Output.Item1, Output.Item2, Angle);
                        if (_INS.IsCalibrated())
                        {
                            var Output_non = _INS.GetNonCalibrated();
                            NewNonCaliEntry = new DataEntry(null, Output_non.Item1, Output_non.Item2, Angle);
                        }
                    }
					else if (_currentPoZYX != null)
					{
						NewEntry = new DataEntry(_currentPoZYX, Output.Item1, Output.Item2, Angle);
						if (_INS.IsCalibrated())
						{
							var Output_non = _INS.GetNonCalibrated();
							NewNonCaliEntry = new DataEntry(null, Output_non.Item1, Output_non.Item2, Angle);
						}
					}
				}
				if (NewEntry != null)
				{
					dataEntries.Enqueue(NewEntry);
				}

				if (NewNonCaliEntry != null)
				{
					UnCalibrated.Enqueue(NewNonCaliEntry);
				}
			}
		}

		public void ClearEntries()
		{
			KalmanData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
			RollingAverageData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
			dataEntries = new ConcurrentQueue<DataEntry>();
			UnCalibrated = new ConcurrentQueue<DataEntry>();
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
			throw new TooManyDataEntriesRequestedException($"There is not this many DataEntries that can be requested.\nThere is only {avalibleDataEntries.Count()} avalible!");
		}

		public void CalibrateINS(int tid = 5000)
		{
			_INS.ClearCalibration();
			Console.WriteLine("Leave sensor level!");
			Thread.Sleep(1000);
			StartReading();
			Thread.Sleep(tid);

			StopReading();
			Console.WriteLine($"Calibrated for {tid/1000} sec");
			int count = 0;
			double AX = 0;
			double AY = 0;
			double AZ = 0;
			double GX = 0;
			double GY = 0;
			double GZ = 0;
			foreach (var item in dataEntries)
			{
				AX += item.INS_Accelerometer.X;
				AY += item.INS_Accelerometer.Y;
				AZ += item.INS_Accelerometer.Z;
				GX += item.INS_Gyroscope.X;
				GY += item.INS_Gyroscope.Y;
				GZ += item.INS_Gyroscope.Z;
				count++;
			}
			Console.WriteLine($"Data collection done, calculating ({count})");

			AX /= count;
			AY /= count;
			AZ /= count;
			GX /= count;
			GY /= count;
			GZ /= count;

			XYZ Accelerometer_calibration = new XYZ(AX, AY, AZ);
			XYZ Gyroscope_calibration = new XYZ(GX, GY, GZ);
			Console.WriteLine($"Accelerometer\n{Accelerometer_calibration}");
			Console.WriteLine($"Gyroscope\n{Gyroscope_calibration}");
			_INS.SetCalibration(Accelerometer_calibration, Gyroscope_calibration);
			ClearEntries();
			Console.WriteLine("Calibration Done");
		}

		public void AddDataEntry(DataEntry dataEntry)
		{
			dataEntries.Enqueue(dataEntry);
		}


	}
}
