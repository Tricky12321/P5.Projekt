using System;
using System.Collections.Generic;
using System.Linq;

namespace Serial.DynamicCalibrationName
{
    public class DynamicCalibration
    {
        private int _runningAverageBatchSizes = 100;
        private double _slopeDiffenceTreshold = 0.70;
        private double _gravitationalConst = 9.82;

        public List<XYZ> VelocityList = new List<XYZ>();
        public List<XYZ> DistanceList = new List<XYZ>();
        public List<XYZ> DynamicVelocityList = new List<XYZ>();

        public List<XYZ> DynamicDistanceList = new List<XYZ>();

        private List<XYZ> _accelerationList = new List<XYZ>();

        public DynamicCalibration(List<XYZ> acceleration)
        {
            _accelerationList.Add(new XYZ(0, 0, 0, 1.0));

            foreach (XYZ acc in acceleration)
            {
                XYZ pos = new XYZ();
                pos.X = acc.X / 1000.0 * _gravitationalConst;
                pos.Y = acc.Y / 1000.0 * _gravitationalConst;
                pos.Z = acc.Z / 1000.0 * _gravitationalConst;
                pos.TimeOfData = acc.TimeOfData / 1000.0;
                _accelerationList.Add(pos);
            }
        }

        public void CalculateNaiveVelocity()
        {
            VelocityList.Add(new XYZ(0, 0, 0, 1.0));
            for (int i = 1; i < _accelerationList.Count; i++)
            {
                XYZ placeXYZ = new XYZ();

                placeXYZ.TimeOfData = _accelerationList[i].TimeOfData;
                placeXYZ.X = (placeXYZ.TimeOfData - VelocityList[i - 1].TimeOfData)
                                            * ((_accelerationList[i - 1].X + _accelerationList[i].X) / 2)
                                            + VelocityList[i - 1].X;
                placeXYZ.Y = (placeXYZ.TimeOfData - VelocityList[i - 1].TimeOfData)
                                            * ((_accelerationList[i - 1].Y + _accelerationList[i].Y) / 2)
                                            + VelocityList[i - 1].Y;
                placeXYZ.Z = (placeXYZ.TimeOfData - VelocityList[i - 1].TimeOfData)
                                            * ((_accelerationList[i - 1].Z + _accelerationList[i].Z) / 2)
                                            + VelocityList[i - 1].Z;
                VelocityList.Add(placeXYZ);
            }

            List<double> inputs = VelocityList.Select(x => x.X).ToList();
            List<double> times = VelocityList.Select(x => x.TimeOfData).ToList();
            List<Tuple<double, int>> accelerationPointsList = FindAccelerationPoints(inputs, times, _runningAverageBatchSizes, 1);

            List<Tuple<int, int>> driftingIndexesList = FindDriftRanges(accelerationPointsList);

            double bestSlopeTreshold = 1.5;
            double lastValue = 5;

            for (double i = 0.3; i < 1.5; i += 0.01)
            {

                _slopeDiffenceTreshold = i;
                CalculateDynamicVelocityList(driftingIndexesList);
                var test = Math.Abs(DynamicVelocityList.First().X - DynamicVelocityList.Last().X);
                if (test < lastValue)
                {
                    lastValue = test;
                    bestSlopeTreshold = i;
                }
                DynamicVelocityList.Clear();
            }

            _slopeDiffenceTreshold = bestSlopeTreshold;
            CalculateDynamicVelocityList(driftingIndexesList);
        }

        private void CalculateDynamicVelocityList(List<Tuple<int, int>> accelerationPoints)
        {
            foreach (XYZ point in VelocityList)
            {
                DynamicVelocityList.Add(new XYZ(point.X, point.Y, point.Z, point.TimeOfData));
            }

            for (int i = 0; i < accelerationPoints.Count; i++)
            {
                int startIndex = accelerationPoints[i].Item1;
                int endIndex = accelerationPoints[i].Item2;
                List<XYZ> driftVelocity = DynamicVelocityList.GetRange(startIndex, endIndex - startIndex);

                double slope = CalculateTendencySlope(driftVelocity.Select(x => x.X).ToList(), driftVelocity.Select(x => x.TimeOfData).ToList());

                for (int j = startIndex; j < VelocityList.Count; j++)
                {
                    DynamicVelocityList[j].X = DynamicVelocityList[j].X - slope * (DynamicVelocityList[j].TimeOfData - DynamicVelocityList[startIndex].TimeOfData);
                }
            }
            /*
            int thisIndex = 0;
            double defaultValue = 0;
            for (int i = 1; i < VelocityList.Count; i++)
            {
                if (accelerationPoints[thisIndex].Item2 == i)
                {
                    DynamicVelocityList[i].X += defaultValue;
                    thisIndex++;
                }
                else
                {
                    DynamicVelocityList[i].X = DynamicVelocityList[i - 1].X;
                    defaultValue = DynamicVelocityList[i].X;
                }
            }*/
        }

        private List<Tuple<int, int>> FindDriftRanges(List<Tuple<double, int>> accelerationPointsList)
        {
            List<Tuple<int, int>> driftRanges = new List<Tuple<int, int>>();

            int startIndex = 0;
            int endIndex = accelerationPointsList[0].Item2;

            driftRanges.Add(new Tuple<int, int>(0, accelerationPointsList[0].Item2 - 1));

            for (int i = 1; i < accelerationPointsList.Count; i++)
            {
                if (accelerationPointsList[i].Item2 == accelerationPointsList[i - 1].Item2 + 1)
                {
                    startIndex = accelerationPointsList[i].Item2 + 1;
                }
                else 
                {
                    endIndex = accelerationPointsList[i].Item2 - 1;
                    driftRanges.Add(new Tuple<int, int>(startIndex, endIndex));
                }
            }

            driftRanges.Add(new Tuple<int, int>(accelerationPointsList[accelerationPointsList.Count - 1].Item2 + 1, VelocityList.Count - 1));

            return driftRanges;
        }

        private List<Tuple<double, int>> FindAccelerationPoints(List<double> points, List<double> times, int batchSize, int offset)
        {
            List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();

            int timesToRun = points.Count / offset - batchSize / offset - 1;

            for (int i = 0; i < timesToRun; i++)
            {
                double thresFirst = CalculateTendencySlope(points.GetRange(i * offset, batchSize / 2).ToList(), times.GetRange(i * offset, batchSize / 2).ToList());
                double thresSecond = CalculateTendencySlope(points.GetRange(i * offset + batchSize / 2, batchSize / 2).ToList(), times.GetRange(i * offset + batchSize / 2, batchSize / 2).ToList());
                if (!(thresFirst + _slopeDiffenceTreshold > thresSecond && thresFirst - _slopeDiffenceTreshold < thresSecond))
                {
                    listToReturn.Add(new Tuple<double, int>(times[i * offset + batchSize / 2], i * offset + batchSize / 2));
                }
            }
            return listToReturn;
        }

        private double CalculateTendencySlope(List<double> points, List<double> times)
        {
            if (points.Count != 0 || times.Count != 0)
            {
                double pointsAverage = points.Average();
                double timeAverage = times.Average();
                List<double> xYOffset = new List<double>();
                List<double> squareXOffset = new List<double>();

                for (int i = 0; i < points.Count; i++)
                {
                    xYOffset.Add((times[i] - timeAverage) * (points[i] - pointsAverage));
                    squareXOffset.Add(Math.Pow(times[i] - timeAverage, 2));
                }
                return xYOffset.Sum() / squareXOffset.Sum();
            }
            return 0;
        }
    }
}
