using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Serial.DataMapper.Kalman;
using Serial.DataMapper.DataReader;
using Serial.DataMapper.Segmenter;
namespace Serial.DataMapper
{
	public class DataMapper : ReaderController
    {

		public DataMapper(bool Pozyx = true, bool Ins = true) : base(Pozyx, Ins) 
        {
            
        }

        // 50 points pr. segment
        
        public void GenerateKalman()
        {
            Kalman = true;
			KalmanData = KalmanController.GenerateKalman(AllDataEntries);
        }

		public ConcurrentQueue<DataEntry> SegmentData(int NumOfSplits = 50) {
			return SegmentController.SegmentData(AllDataEntries, NumOfSplits);
		}

        private List<double> RollingAverage(List<double> input, int periodLength)
        {
            List<double> listToReturn = new List<double>();
            for (int i = 0; i < input.Count - periodLength; i++)
            {
                List<double> subRange = input.GetRange(i, periodLength);
                listToReturn.Add(subRange.Average());
            }
            return listToReturn;
        }

		public void CalculateRollingAverage(int PeriodLength)
        {
            RollingAverageData = new ConcurrentQueue<Tuple<XYZ, XYZ>>();
            List<DataEntry> datas = new List<DataEntry>(AllDataEntries);
            List<double> AX = new List<double>();
            List<double> AY = new List<double>();
            List<double> AZ = new List<double>();

            List<double> GX = new List<double>();
            List<double> GY = new List<double>();
            List<double> GZ = new List<double>();
            List<double> Timer = new List<double>();

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
                RollingAverageData.Enqueue(new Tuple<XYZ, XYZ>(new XYZ(AX[i], AY[i], AZ[i], Timer[i]), new XYZ(GX[i], GY[i], GZ[i], Timer[i])));
            }
            RollingAverageBool = true;
        }

        public ConcurrentQueue<Tuple<DataEntry, double>> CalculateVariance(int periodLength = 50)
        {
            List<DataEntry> data = AllDataEntries.ToList();
            ConcurrentQueue<Tuple<DataEntry, double>> output = new ConcurrentQueue<Tuple<DataEntry, double>>();
            double maxEntry;
            double minEntry;
            double variance;

            int periodCount = data.Count() / periodLength;
            for (int i = 0; i < periodCount; i++)
            {
                maxEntry = data.GetRange(i * periodLength, periodLength).Max(x => x.INS_Accelerometer.X);
                minEntry = data.GetRange(i * periodLength, periodLength).Min(x => x.INS_Accelerometer.X);
                variance = maxEntry - minEntry;

                for (int j = i * periodLength; j < (i + 1) * periodLength; j++)
                {
                    output.Enqueue(new Tuple<DataEntry, double>(data[j], variance));
                }
            }
            return output;
        }

    }
}
