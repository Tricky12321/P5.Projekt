using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Serial.DataMapper
{
	public class DataMapper
	{
		private PozyxReader _pozyx;
		private INSReader _INS;

		private ConcurrentQueue<DataEntry> dataEntries = new ConcurrentQueue<DataEntry>();

		private ConcurrentQueue<DataEntry> avalibleDataEntries => new ConcurrentQueue<DataEntry>(dataEntries.Where(X => X.Used == false));
		public ConcurrentQueue<DataEntry> AllDataEntries => dataEntries;

		public ConcurrentQueue<Tuple<XYZ, XYZ>> KalmanData;
		public ConcurrentQueue<Tuple<XYZ, XYZ>> RollingAverageData;

		private bool Reading = false;

		private object _dataEntryLock = new object();
		private XYZ _currentPoZYX = null;

		public bool Kalman = false;
		public bool RollingAverageBool = false;

		public Stopwatch Timer;

		bool Pozyx = false;
		bool Ins = false;

		public DataMapper(bool Pozyx = true, bool Ins = true)
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

		private void ReadPozyx()
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
						NewEntry = new DataEntry(_currentPoZYX, null, null,0);
						dataEntries.Enqueue(NewEntry);
					}
					else
					{
						_currentPoZYX = PoZYX_Position;
					}
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
				double Angle = Output.Item3;
				DataEntry NewEntry = null; ;
				lock (_dataEntryLock)
				{
					if (Pozyx == false)
					{
						NewEntry = new DataEntry(null, Accelerometer, Gyroscope, Angle);

					}
					else if (_currentPoZYX != null)
					{
						NewEntry = new DataEntry(_currentPoZYX, Accelerometer, Gyroscope, Angle);
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
			KalmanData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
            RollingAverageData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
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

		public void CalibrateINS()
		{
			_INS.ClearCalibration();
			Console.WriteLine("Leave sensor level!");
			Thread.Sleep(1000);
			StartReading();
			Thread.Sleep(5000);
			StopReading();
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

		public void CalculateRollingAverage(int PeriodLength)
		{
			RollingAverageData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
			List<DataEntry> datas = new List<DataEntry>(dataEntries);
			List<double> AX = new List<double>();
			List<double> AY = new List<double>();
			List<double> AZ = new List<double>();

			List<double> GX = new List<double>();
			List<double> GY = new List<double>();
			List<double> GZ = new List<double>();
			List<long> Timer = new List<long>();

			foreach (var data in datas)
			{
				AX.Add(data.INS_Accelerometer.X);
				AY.Add(data.INS_Accelerometer.Y);
				AZ.Add(data.INS_Accelerometer.Z);

				GX.Add(data.INS_Gyroscope.X);
				GY.Add(data.INS_Gyroscope.Y);
				GZ.Add(data.INS_Gyroscope.Z);
				Timer.Add(data.INS_Gyroscope.TimeOfData);
			}

			AX = RollingAverage(AX, PeriodLength);
			AY = RollingAverage(AY, PeriodLength);
			AZ = RollingAverage(AZ, PeriodLength);
			GX = RollingAverage(GX, PeriodLength);
			GY = RollingAverage(GY, PeriodLength);
			GZ = RollingAverage(GZ, PeriodLength);
			int count = AX.Count;
			for (int i = 0; i < count; i++)
			{
				Console.WriteLine($"[{i}] Accelerometer {AX[i]},{AY[i]},{AZ[i]}");
				Console.WriteLine($"[{i}] Gyroscope {GX[i]},{GY[i]},{GZ[i]}");
				RollingAverageData.Enqueue(new Tuple<XYZ, XYZ>(new XYZ(AX[i], AY[i], AZ[i],Timer[i]), new XYZ(GX[i], GY[i], GZ[i], Timer[i])));
			}
			RollingAverageBool = true;
		}

		private List<double> RollingAverage(List<double> InputList, int PeriodLength)
		{

			return Enumerable.Range(0, InputList.Count - PeriodLength).Select(n => InputList.Skip(n).Take(PeriodLength).Average()).ToList();
		}

	}
}
