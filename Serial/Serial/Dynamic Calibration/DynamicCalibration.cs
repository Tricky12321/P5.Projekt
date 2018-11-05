using System;
using System.Collections.Generic;
using System.Linq;

namespace Serial.DynamicCalibrationName
{
    public class DynamicCalibration
    {
        const int _runningAverageBatchSizes = 100;
        const double _slopeDiffenceTreshold = 0.69;
        const double _gravitationalConst = 9.81;
        const int _gradientCalculationOffset = 1;

        public List<XYZ> NaiveVelocityList = new List<XYZ>();

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
            NaiveVelocityList.Add(new XYZ(0, 0, 0, 1.0));
            for (int i = 1; i < _accelerationList.Count; i++)
            {
                XYZ placeXYZ = new XYZ();

                placeXYZ.TimeOfData = _accelerationList[i].TimeOfData;
                placeXYZ.X = (placeXYZ.TimeOfData - NaiveVelocityList[i - 1].TimeOfData)
                                            * ((_accelerationList[i - 1].X + _accelerationList[i].X) / 2)
                                            + NaiveVelocityList[i - 1].X;
                placeXYZ.Y = (placeXYZ.TimeOfData - NaiveVelocityList[i - 1].TimeOfData)
                                            * ((_accelerationList[i - 1].Y + _accelerationList[i].Y) / 2)
                                            + NaiveVelocityList[i - 1].Y;
                placeXYZ.Z = (placeXYZ.TimeOfData - NaiveVelocityList[i - 1].TimeOfData)
                                            * ((_accelerationList[i - 1].Z + _accelerationList[i].Z) / 2)
                                            + NaiveVelocityList[i - 1].Z;
                NaiveVelocityList.Add(placeXYZ);
            }
        }

        /// <summary>
        /// Returns a list containing velocity for the dynamic calibration.
        /// </summary>
        /// <param name="inputs"> list of naive calculated valied from an axis </param>
        /// <param name="times"> a list of times from the naive calculated velocity </param>
        public List<XYZ> CalculateDynamicVelocityList(List<double> inputs, List<double> times)
        {
            List<XYZ> dynamicVelocityList = new List<XYZ>();

            List<Tuple<double, int>> accelerationPointsList = FindAccelerationPoints(inputs, times, _runningAverageBatchSizes, _gradientCalculationOffset);
            List<Tuple<int, int>> driftingIndexesList = FindDriftRanges(accelerationPointsList);

            foreach (XYZ point in NaiveVelocityList)
            {
                dynamicVelocityList.Add(new XYZ(point.X, point.Y, point.Z, point.TimeOfData));
            }

            for (int i = 0; i < driftingIndexesList.Count; i++)
            {
                int startIndex = driftingIndexesList[i].Item1;
                int endIndex = driftingIndexesList[i].Item2;
                List<XYZ> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);

                double slope = CalculateTendencySlope(driftVelocity.Select(x => x.X).ToList(), driftVelocity.Select(x => x.TimeOfData).ToList());

                for (int j = startIndex; j < NaiveVelocityList.Count; j++)
                {
                    dynamicVelocityList[j].X = dynamicVelocityList[j].X - slope * (dynamicVelocityList[j].TimeOfData - dynamicVelocityList[startIndex].TimeOfData);
                }
            }

            return dynamicVelocityList;
        }

        /// <summary>
        /// Returns a list containing indexes for when drifting is detected.
        /// </summary>
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

            driftRanges.Add(new Tuple<int, int>(accelerationPointsList[accelerationPointsList.Count - 1].Item2 + 1, NaiveVelocityList.Count - 1));

            return driftRanges;
        }


        /// <summary>
        /// Finds times for when the difference in velocity is over a gradient treshold.
        /// </summary>
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

        /// <summary>
        /// Returns the slope for the points given as parameter.
        /// </summary>
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
