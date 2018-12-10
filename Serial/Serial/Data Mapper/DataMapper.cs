using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Serial.DataMapper.Kalman;
using Serial.DataMapper.DataReader;
using Serial.DataMapper.Segmenter;
using Serial.DynamicCalibrationName;
using Serial.DynamicCalibrationName.Points;

namespace Serial.DataMapper
{
    public class DataMapper : ReaderController
    {
        const double _gravitationalConst = 9.81;


        public DataMapper(bool Pozyx = true, bool Ins = true) : base(Pozyx, Ins)
        {

        }

        // 50 points pr. segment

        public void GenerateKalman()
        {
            Kalman = true;
            KalmanData = KalmanController.GenerateKalman(AllDataEntries);
        }

        public List<XYZ> GetAccelerationXYZFromCSV()
        {
            List<XYZ> listToReturn = new List<XYZ>();
            AllDataEntries.ToList().ForEach(x => listToReturn.Add(new XYZ(x.INS_Accelerometer.X, x.INS_Accelerometer.Y, x.INS_Accelerometer.Z, x.INS_Accelerometer.TimeOfData)));
            return listToReturn;
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
                //Console.WriteLine($"[{i}] Accelerometer {AX[i]},{AY[i]},{AZ[i]}");
                //Console.WriteLine($"[{i}] Gyroscope {GX[i]},{GY[i]},{GZ[i]}");
                RollingAverageData.Enqueue(new Tuple<XYZ, XYZ>(new XYZ(AX[i], AY[i], AZ[i], Timer[i]), new XYZ(GX[i], GY[i], GZ[i], Timer[i])));
            }
            RollingAverageBool = true;
        }

        public ConcurrentQueue<Tuple<DataEntry, double, double>> CalculateVariance(int periodLength = 50)
        {
            List<DataEntry> data = AllDataEntries.ToList();
            ConcurrentQueue<Tuple<DataEntry, double, double>> output = new ConcurrentQueue<Tuple<DataEntry, double, double>>();
            List<DataEntry> batch = new List<DataEntry>();
            double tendencySlope;
            double tendencyOffset;
            double residualSS;

            int periodCount = data.Count() - periodLength;
            for (int i = 0; i < periodCount; i++)
            {
                batch = data.GetRange(i, periodLength);
                tendencySlope = CalculateTendencySlope(batch);
                tendencySlope = Math.Pow(tendencySlope, 3);
                tendencyOffset = CalculateTendensyOffset(batch, tendencySlope);
                residualSS = CalculateResidualSumOfSquares(batch, tendencySlope, tendencyOffset);
                output.Enqueue(new Tuple<DataEntry, double, double>(data[i + periodLength/2], residualSS, tendencySlope));
            }

            return output;

            /*List<DataEntry> data = AllDataEntries.ToList();
            ConcurrentQueue<Tuple<DataEntry, double>> output = new ConcurrentQueue<Tuple<DataEntry, double>>();
            double maxEntry;
            double minEntry;
            double variance;
            double squareVariance;

            int periodCount = data.Count() / periodLength;
            for (int i = 0; i < periodCount; i++)
            {
                maxEntry = data.GetRange(i * periodLength, periodLength).Max(x => x.INS_Accelerometer.X);
                minEntry = data.GetRange(i * periodLength, periodLength).Min(x => x.INS_Accelerometer.X);
                variance = maxEntry - minEntry;
                squareVariance = Math.Pow(variance, 2);

                for (int j = i * periodLength; j < (i + 1) * periodLength; j++)
                {
                    output.Enqueue(new Tuple<DataEntry, double>(data[j], variance));
                }
            }
            return output;*/
        }

        private double CalculateTendencySlope(List<DataEntry> inputsTimes)
        {
            List<double> inputs = inputsTimes.Select(x => x.INS_Accelerometer.X).ToList();
            List<double> times = inputsTimes.Select(x => x.INS_Accelerometer.TimeOfData).ToList();

            if (inputs.Count != 0 || times.Count != 0)
            {
                double pointsAverage = inputs.Average();
                double timeAverage = times.Average();
                List<double> xYOffset = new List<double>();
                List<double> squareXOffset = new List<double>();

                for (int i = 0; i < inputs.Count; i++)
                {
                    xYOffset.Add((times[i] - timeAverage) * (inputs[i] - pointsAverage));
                    squareXOffset.Add(Math.Pow(times[i] - timeAverage, 2));
                }
                return xYOffset.Sum() / squareXOffset.Sum();
            }
            throw new InvalidInputException("FindStationaryRangesRanges got a different amount of times and inputs, " +
                                            "they should be that same and should contain more than 0 elements");
        }

        private double CalculateTendensyOffset(List<DataEntry> inputsTimes, double slope)
        {
            List<double> yAxis = inputsTimes.Select(x => x.INS_Accelerometer.X).ToList();
            List<double> xAxis = inputsTimes.Select(x => x.INS_Accelerometer.TimeOfData).ToList();

            return (yAxis.Sum() - slope * xAxis.Sum()) / inputsTimes.Count();
        }

        private double CalculateResidualSumOfSquares(List<DataEntry> inputsTimes, double slopeTendensy, double offsetTendensy)
        {
            List<double> residualSSList = new List<double>();
            double residual;
            foreach (DataEntry entry in inputsTimes)
            {

                var test = entry.INS_Accelerometer.X - (slopeTendensy * entry.INS_Accelerometer.TimeOfData + offsetTendensy);
                residual = Math.Pow(test, 2);
                residualSSList.Add(Math.Abs(residual));
            }
            double sSResidual = residualSSList.Sum();

            return sSResidual / inputsTimes.Count();
        }

        //Calculates variance, velocity, slope and slope difference
        public ConcurrentQueue<Tuple<DataEntry, double, double, double, double>> CalculateAcceleration(int batchSize = 200)
        {
            List<DataEntry> data = AllDataEntries.ToList();
            ConcurrentQueue<Tuple<DataEntry, double, double, double, double>> output = new ConcurrentQueue<Tuple<DataEntry, double, double, double, double>>();
            List<DataEntry> accelerationList = new List<DataEntry>();
            accelerationList.Add(new DataEntry(new XYZ(0.0,0.0,0.0), new XYZ(0.0,0.0,0.0), new XYZ(0.0,0.0,0.0), 0.0));

            foreach (DataEntry entry in data)
            {
                double value = (entry.INS_Accelerometer.X / 1000.0) * _gravitationalConst;
                double time = entry.INS_Accelerometer.TimeOfData / 1000.0;
                DataEntry pos = new DataEntry(new XYZ(0.0, 0.0, 0.0), new XYZ(value, 0.0, 0.0, time), new XYZ(0.0, 0.0, 0.0), 0);

                accelerationList.Add(pos);
            }

            List<DataEntry> velocityList = CalculateVelocity(accelerationList);

            for (int i = batchSize; i < velocityList.Count - batchSize; i++)
            {
                List<DataEntry> firstBatchList = velocityList.GetRange(i - batchSize, batchSize).ToList();
                List<DataEntry> secondBatchList = velocityList.GetRange(i - 1, batchSize).ToList();
                double thresFirst = CalculateTendencySlope(firstBatchList);
                double thresSecond = CalculateTendencySlope(secondBatchList);


                var batch = velocityList.GetRange(i - batchSize/2, batchSize).ToList();
                double tendencySlope = CalculateTendencySlope(batch);
                double tendencyOffset = CalculateTendensyOffset(batch, tendencySlope);
                double residualSS = CalculateResidualSumOfSquares(batch, tendencySlope, tendencyOffset);

                output.Enqueue(new Tuple<DataEntry, double, double, double, double>(data[i], velocityList[i].INS_Accelerometer.X, tendencySlope, tendencySlope/(Math.Pow(residualSS,2)), Math.Abs(thresFirst - thresSecond)));
            }
            return output;
        }

        public List<DataEntry> CalculateVelocity(List<DataEntry> accelerationList)
        {
            List<DataEntry> velocityList = new List<DataEntry>();
            velocityList.Add(new DataEntry(new XYZ(0.0, 0.0, 0.0), new XYZ(0.0, 0.0, 0.0), new XYZ(0.0, 0.0, 0.0), 0.0));
            for (int i = 1; i < accelerationList.Count; i++)
            {
                double time = accelerationList[i].INS_Accelerometer.TimeOfData;
                double value = (time - velocityList[i - 1].INS_Accelerometer.TimeOfData)
                    * ((accelerationList[i - 1].INS_Accelerometer.X + accelerationList[i].INS_Accelerometer.X) / 2)
                    + velocityList[i - 1].INS_Accelerometer.X;
                velocityList.Add(new DataEntry(new XYZ(0.0, 0.0, 0.0), new XYZ(value, 0.0, 0.0, time), new XYZ(0.0, 0.0, 0.0), 0));
            }
            return velocityList;
        }
    }
}
