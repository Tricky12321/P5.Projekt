using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

namespace Serial.DataMapper.Kalman
{
	public static class KalmanController
	{
		public static ConcurrentQueue<Tuple<XYZ, XYZ>> GenerateKalman(ConcurrentQueue<DataEntry> dataEntries)
		{
			ConcurrentQueue<Tuple<XYZ, XYZ>> KalmanData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();

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
			return KalmanData;
		}
	}
}
