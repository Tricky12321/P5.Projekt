using System;
using System.Collections.Generic;
using System.Linq;

namespace Serial.DynamicCalibrationName
{
    public class DynamicCalibration
    {
        const int _runningAverageBatchSizes = 500;
        const double _slopeDiffenceTreshold = 0.3;
        const double _pointOffsetStationaryTreshold = 0.3;
        const double _gravitationalConst = 9.81;
        const int _gradientCalculationOffset = 1;

        public List<XYZ> NaiveVelocityList = new List<XYZ>();

        private List<XYZ> _accelerationList = new List<XYZ>();

        public DynamicCalibration(List<XYZ> acceleration)
        {
            _accelerationList.Add(new XYZ(0, 0, 0, 0.0));

            foreach (XYZ acc in acceleration)
            {
                XYZ pos = new XYZ();
                pos.X = (acc.X / 1000.0) * _gravitationalConst;
                pos.Y = (acc.Y / 1000.0) * _gravitationalConst;
                pos.Z = (acc.Z / 1000.0) * _gravitationalConst;
                pos.TimeOfData = acc.TimeOfData / 1000.0;
                _accelerationList.Add(pos);
            }
        }

        public List<Tuple<double, double>> CalculatePosition(List<double> velocityList, List<double> times)
        {
            List<XYZ> distanceList = new List<XYZ>();
            distanceList.Add(new XYZ(0,0,0,0));

            for (int i = 1; i < velocityList.Count; i++)
            {
                XYZ newDistance = new XYZ();
                newDistance.X = (velocityList[i].TimeOfData - velocityList[i - 1].TimeOfData) * (velocityList[i].X + velocityList[i - 1].X) / 2 + distanceList[i - 1].X;
                newDistance.TimeOfData = velocityList[i].TimeOfData;
                distanceList.Add(newDistance);
                //(t1-t0) * (v1+v0)/2 + d0 
            }
            return distanceList;
        }


        public void CalculateNaiveVelocity()
        {
            NaiveVelocityList.Add(new XYZ(0, 0, 0, 0.0));
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
        /// <param name="inputs"> list of naive calculated velocities from an axis </param>
        /// <param name="times"> a list of times from the naive calculated velocity </param>
        public List<XYZ> CalculateDynamicVelocityList(List<double> inputs, List<double> times, bool useRunningAverage = true)
        {
            List<XYZ> dynamicVelocityList = new List<XYZ>();

            List<Tuple<double, double>> velocityList = new List<Tuple<double, double>>();

            if (useRunningAverage)
            {
                velocityList = GetRunningAverageAcceleration(inputs, times);
            }
            else
            {
                foreach (XYZ point in NaiveVelocityList)
                {
                    velocityList.Add(new Tuple<double, double>(point.X, point.TimeOfData));
                }
            }

            List<Tuple<double, int>> accelerationPointsList = FindAccelerationPoints(velocityList.Select(x => x.Item1).ToList(), velocityList.Select(x => x.Item2).ToList(), _runningAverageBatchSizes, _gradientCalculationOffset);
            List<Tuple<int, int>> driftingIndexesList = FindDriftRanges(accelerationPointsList, velocityList.Count - 1);



            foreach (Tuple<double,double> point in velocityList)
            {
                dynamicVelocityList.Add(new XYZ(point.Item1, 0.0, 0.0, point.Item2));
            }

            for (int i = 0; i < driftingIndexesList.Count; i++)
            {
                int startIndex = driftingIndexesList[i].Item1;
                int endIndex = driftingIndexesList[i].Item2;
                List<XYZ> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);

                double slope = CalculateTendencySlope(driftVelocity.Select(x => x.X).ToList(), driftVelocity.Select(x => x.TimeOfData).ToList());

                for (int j = startIndex; j < velocityList.Count; j++)
                {
                    var test = slope * (dynamicVelocityList[j].TimeOfData - dynamicVelocityList[startIndex].TimeOfData);
                    dynamicVelocityList[j].X = dynamicVelocityList[j].X - test;
                }
            }
            /*
            int thisIndex = 0;
            double defaultValue = 0;
            for (int i = 1; i < NaiveVelocityList.Count; i++)
            {
                if (accelerationPointsList.Count != thisIndex && accelerationPointsList[thisIndex].Item2 == i)
                {
                    //dynamicVelocityList[i].X -= defaultValue;
                    thisIndex++;
                }
                else
                {
                    dynamicVelocityList[i].X = dynamicVelocityList[i - 1].X;
                    defaultValue = dynamicVelocityList[i].X;
                }
            }*/
            return dynamicVelocityList;
        }

        private List<Tuple<double, int>> FindStationaryRanges(List<double> input, List<double> times, int batchSize, int offset)
        {
            if (input.Count == times.Count)
            {
                List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();
                int timesToRun = input.Count / offset - batchSize / offset - 1;

                for (int i = 0; i < timesToRun; i++)
                {
                    List<double> batchInputs = input.GetRange(i * offset, batchSize);
                    List<double> batchTimes = times.GetRange(i * offset, batchSize);

                    double pointsOffset = CalculatePointsOffset(batchInputs, batchTimes);
                    if (pointsOffset < _pointOffsetStationaryTreshold)
                    {
                        listToReturn.Add(new Tuple<double, int>(times[i * offset], i * offset));
                    }
                }
                return listToReturn;
            }
            throw new InvalidInputException("FindStationaryRangesRanges got a different amount of times and inputs, they should be that same");
        }

        /// <summary>
        /// Returns a list containing indexes for when drifting is detected.
        /// </summary>
        private List<Tuple<int, int>> FindDriftRanges(List<Tuple<double, int>> accelerationPointsList, int lastIndex)
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

            driftRanges.Add(new Tuple<int, int>(accelerationPointsList[accelerationPointsList.Count - 1].Item2 + 1, lastIndex));

            return driftRanges;
        }


        /// <summary>
        /// Finds times for when the difference in velocity is over a gradient treshold.
        /// </summary>
        private List<Tuple<double, int>> FindAccelerationPoints(List<double> points, List<double> times, int batchSize, int offset)
        {
            List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();

            List<Tuple<double, int>> slopeDifferencesList = CalculateSlopeDifferences(points, times, batchSize, offset);

            foreach(Tuple<double, int> slope in slopeDifferencesList)
            {
                if (Math.Abs(slope.Item1) > _slopeDiffenceTreshold){
                    listToReturn.Add(new Tuple<double, int>(times[slope.Item2], slope.Item2));
                }
            }
            return listToReturn;
        }

        /// <summary>
        /// Returns a Tuple containing the difference in slopes between two ranges defined be batchsize and offset
        /// </summary>
        /// <returns>The slope differences.</returns>
        /// <param name="points">A list of the raw acceleration data for one axis.</param>
        /// <param name="times">List of all times from the raw data.</param>
        /// <param name="batchSize">The size of the ranges on which the slope will be calculated, the ranges will each be batchSize/2 long.</param>
        /// <param name="offset">The offset between ranges on which the slope will be calculated</param>
        private List<Tuple<double, int>> CalculateSlopeDifferences(List<double> points, List<double> times, int batchSize, int offset)
        {
            List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();
            int timesToRun = points.Count / offset - batchSize / offset - 1;

            for (int i = 0; i < timesToRun; i++)
            {
                double thresFirst = CalculateTendencySlope(points.GetRange(i * offset, batchSize / 2).ToList(), times.GetRange(i * offset, batchSize / 2).ToList());
                double thresSecond = CalculateTendencySlope(points.GetRange(i * offset + batchSize / 2, batchSize / 2).ToList(), times.GetRange(i * offset + batchSize / 2, batchSize / 2).ToList());
                listToReturn.Add(new Tuple<double, int>(thresSecond-thresFirst, i * offset + batchSize / 2));
            }
            return listToReturn;
        }


        /// <summary>
        /// Gets the running average acceleration.
        /// </summary>
        /// <returns>A list of tuples containing acceleration and time, the list will be without the last range of <paramref name="periodLength"/> length.</returns>
        /// <param name="input">Input acceleration data.</param>
        /// <param name="times">Times related to the input data.</param>
        /// <param name="periodLength">Period Lenght of the running averages.</param>
        private List<Tuple<double, double>> GetRunningAverageAcceleration(List<double> input, List<double> times, int periodLength = 100)
        {
            List<Tuple<double, double>> listToReturn = new List<Tuple<double, double>>();

            List<double> averageList = CalculateRunningAverage(input);

            for (int i = 0; i < averageList.Count; i++)
            {
                listToReturn.Add(new Tuple<double, double>(averageList[i], times[i]));
            }
            return listToReturn;
        }

        /// <summary>
        /// Calculates the running average of a list of doubles.
        /// </summary>
        /// <returns>List of doubles containing the runnging average.</returns>
        /// <param name="input">List of doubles containing the input.</param>
        /// <param name="periodLength">The length of the periodes for the average.</param>
        private List<double> CalculateRunningAverage(List<double> input, int periodLength = 100)
        {
            List<double> listToReturn = new List<double>();
            for (int i = 0; i < input.Count - periodLength; i++)
            {
                List<double> subRange = input.GetRange(i, periodLength);
                listToReturn.Add(subRange.Average());
            }
            return listToReturn;
        }

        private double CalculatePointsOffset(List<double> points, List<double> times)
        {
            double pointsAverage = points.Average();
            double timeAverage = times.Average();
            List<double> xYOffset = new List<double>();

            for (int i = 0; i < points.Count; i++)
            {
                xYOffset.Add((times[i] - timeAverage) * (points[i] - pointsAverage));
            }

            return xYOffset.Sum();
        }


        /// <summary>
        /// Returns the slope for the points given as parameter.
        /// </summary>
        private double CalculateTendencySlope(List<double> points, List<double> times)
        {
            if (points.Count != 0 || times.Count != 0)
            {
                double timeAverage = times.Average();
                List<double> squareXOffset = new List<double>();

                for (int i = 0; i < points.Count; i++)
                {
                    squareXOffset.Add(Math.Pow(times[i] - timeAverage, 2));
                }

                return CalculatePointsOffset(points, times) / squareXOffset.Sum();

            }
            return 0;
        }
    }
}
