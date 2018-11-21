﻿using System;
using System.Collections.Generic;
using System.Linq;
using Serial.DynamicCalibrationName.Points;

namespace Serial.DynamicCalibrationName
{
    public class DynamicCalibration
    {
        const double _runningAverageBatchTime = 1.0;
        private double _slopeDiffenceTreshold = 0.25;                      //The lower the value is, the more acceleration points will be found.
        private double _pointResidualSSTreshold;   //defines the upper value for when the scrubber is stationary.
        const double _stationaryDetectionBatchTime = 1.0;
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

        public List<TimePoint> GetPointListWithOneAxisAndTimes(List<XYZ> inputsTimes, char axis = 'X')
        {
            List<TimePoint> listToReturn = new List<TimePoint>();
            foreach (XYZ input in inputsTimes)
            {
                double axisValue;
                switch (axis)
                {
                    case 'X':
                        axisValue = input.X;
                        break;
                    case 'Y':
                        axisValue = input.Y;
                        break;
                    case 'Z':
                        axisValue = input.Z;
                        break;
                    default: throw new InvalidInputException("The axis name has to be uppercase, either X, Y or Z");
                }
                listToReturn.Add(new TimePoint(axisValue, input.TimeOfData));
            }
            return listToReturn;
        }

        public List<TimePoint> CalculatePosition(List<TimePoint> inputTimes)
        {
            List<TimePoint> distanceList = new List<TimePoint>();
            distanceList.Add(new TimePoint(0.0, 0.0));

            for (int i = 1; i < inputTimes.Count; i++)
            {
                double newDistance = (inputTimes[i].Time - inputTimes[i - 1].Time) * (inputTimes[i].Value + inputTimes[i - 1].Value) / 2 + distanceList[i - 1].Value;
                double newTime = inputTimes[i].Time;
                distanceList.Add(new TimePoint(newDistance, newTime));

                //(t1-t0) * (v1+v0)/2 + d0 
            }
            return distanceList;
        }

        public void CalibrateResidualSumOfSquares(double calibrationTime)
        {
            List<TimePoint> accelerationCalibrationBatch = GetPointListWithOneAxisAndTimes(_accelerationList.TakeWhile(x => x.TimeOfData <= calibrationTime).ToList());

            double slope = CalculateTendencySlope(accelerationCalibrationBatch);
            double offset = CalculateTendensyOffset(accelerationCalibrationBatch, slope);
            double errorMarginCalibration = CalculateResidualSumOfSquares(accelerationCalibrationBatch, slope, offset);
            _pointResidualSSTreshold = errorMarginCalibration * 3;
        }


        /// <summary>
        /// Calculates the naive velocity.
        /// </summary>
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
        public List<TimePoint> CalculateDynamicVelocityList(List<TimePoint> inputTimes, bool useRunningAverage = false, bool useDriftCalibration = false, bool useStationaryDetection = true)
        {
            List<TimePoint> dynamicVelocityList = new List<TimePoint>();
            List<TimePoint> velocityList = useRunningAverage ? GetRunningAverageAcceleration(inputTimes) : inputTimes;

            velocityList.ForEach(x => dynamicVelocityList.Add(new TimePoint(x.Value, x.Time)));
            /*
            #region DriftRemoval

            //Trying to remove drift//

            if (useDriftCalibration)
            {
                for (int i = 0; i < driftingIndexesList.Count; i++)
                {
                    int startIndex = driftingIndexesList[i].IndexStart;
                    int endIndex = driftingIndexesList[i].IndexEnd;

                    List<TimePoint> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);
                    double slope = CalculateTendencySlope(driftVelocity);

                    for (int j = startIndex; j < velocityList.Count; j++)
                    {
                        double slopeOffset = slope * (dynamicVelocityList[j].Time - dynamicVelocityList[startIndex].Time);
                        dynamicVelocityList[j].Value = (dynamicVelocityList[j].Value - slopeOffset);
                    }

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        dynamicVelocityList[j].Value = dynamicVelocityList[startIndex].Value;
                    }
                }
            }

            #endregion
            //Done trying to remove drift/
*/
            List<IndexRangePoint> stationaryIndexList = GetStationaryIndexes(GetPointListWithOneAxisAndTimes(_accelerationList), _stationaryDetectionBatchTime, _gradientCalculationOffset);
            List<IndexRangePoint> drivingIndexList = GetDrivingRangesFromStationaryIndex(stationaryIndexList);

            #region StationaryCalibrate

            if (useStationaryDetection)
            {

                for (int i = 0; i < drivingIndexList.Count; i++)
                {
                    int startIndex = drivingIndexList[i].IndexStart;
                    int endIndex = drivingIndexList[i].IndexEnd;

                    List<TimePoint> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);

                    List<TimePoint> driftVelocityStartEnd = new List<TimePoint>
                {
                    driftVelocity.First(),
                    driftVelocity.Last()
                };

                    double slope = CalculateTendencySlope(driftVelocityStartEnd);
                    double startPointOffset = dynamicVelocityList[startIndex].Value;

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        double slopeOffset = slope * (dynamicVelocityList[j].Time - dynamicVelocityList[startIndex].Time);

                        dynamicVelocityList[j].Value = dynamicVelocityList[j].Value - slopeOffset - startPointOffset;
                    }
                }
                foreach (IndexRangePoint startEndIndex in stationaryIndexList)
                {
                    for (int j = startEndIndex.IndexStart; j <= startEndIndex.IndexEnd; j++)
                    {
                        dynamicVelocityList[j].Value = 0.0;
                    }
                }
            }
            #endregion
            /*
            for (int i = 0; i < drivingIndexList.Count; i++)
            {
                int startIndex = drivingIndexList[i].IndexStart;
                int endIndex = drivingIndexList[i].IndexEnd;

                List<TimePoint> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);

                List<IndexRangePoint> driftingIndexesList = FindDriftRanges(driftVelocity, _runningAverageBatchTime, _gradientCalculationOffset, driftVelocity.Count - 1);
                driftingIndexesList.ForEach(x => { x.IndexStart += startIndex; x.IndexEnd += startIndex; });

                foreach (IndexRangePoint indexRange in driftingIndexesList)
                {
                    double slope = CalculateTendencySlope(dynamicVelocityList.GetRange(indexRange.IndexStart, indexRange.IndexEnd - indexRange.IndexStart));

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        double slopeOffset = 1.0/(endIndex-startIndex) * (endIndex-j) * slope * (dynamicVelocityList[j].Time - dynamicVelocityList[startIndex].Time);
                        dynamicVelocityList[j].Value = (dynamicVelocityList[j].Value - slopeOffset);
                    }
                }

                /*
                for (int i = 0; i < driftingIndexesList.Count; i++)
                {
                    int startIndex = driftingIndexesList[i].IndexStart;
                    int endIndex = driftingIndexesList[i].IndexEnd;

                    List<TimePoint> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);
                    double slope = CalculateTendencySlope(driftVelocity);

                    for (int j = startIndex; j < velocityList.Count; j++)
                    {
                        double slopeOffset = slope * (dynamicVelocityList[j].Time - dynamicVelocityList[startIndex].Time);
                        dynamicVelocityList[j].Value = (dynamicVelocityList[j].Value - slopeOffset);
                    }

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        dynamicVelocityList[j].Value = dynamicVelocityList[startIndex].Value;
                    }
                }

            }*/

            return dynamicVelocityList;

        }

        private List<IndexRangePoint> GetDrivingRangesFromStationaryIndex(List<IndexRangePoint> stationaryIndexes)
        {
            int startDrivingIndex = 0;
            int endDrivingIndex = 0;

            List<IndexRangePoint> drivingIndexList = new List<IndexRangePoint>();
            for (int i = 0; i < stationaryIndexes.Count; i++)
            {
                endDrivingIndex = stationaryIndexes[i].IndexStart;
                if (i != 0)
                {
                    drivingIndexList.Add(new IndexRangePoint(startDrivingIndex, endDrivingIndex));
                }
                startDrivingIndex = stationaryIndexes[i].IndexEnd;
            }

            return drivingIndexList;
        }

        /// <summary>
        /// Gets the stationary indexes.
        /// </summary>
        /// <returns>The stationary indexes.</returns>
        /// <param name="stationaryRanges">Stationary ranges.</param>
        private List<IndexRangePoint> GetStationaryIndexes(List<TimePoint> inputsTimes, double batchTime, int offset)
        {
            List<IndexPoint> stationaryRanges = FindStationaryRanges(inputsTimes, batchTime, offset);

            List<IndexRangePoint> stationaryIndexRanges = new List<IndexRangePoint>();

            int startIndex = 0;
            int endIndex = stationaryRanges[0].Index;

            //stationaryIndexRanges.Add(new Tuple<int, int>(0, stationaryRanges[0].Item2 - 1));

            for (int i = 1; i < stationaryRanges.Count; i++)
            {
                if (stationaryRanges[i].Index == stationaryRanges[i - 1].Index + 1)
                {
                    endIndex = stationaryRanges[i].Index;
                }
                else
                {
                    if (startIndex != endIndex)
                    {
                        stationaryIndexRanges.Add(new IndexRangePoint(startIndex, endIndex));
                    }
                    startIndex = stationaryRanges[i].Index;
                }
            }

            stationaryIndexRanges.Add(new IndexRangePoint(startIndex, inputsTimes.Count - 1));

            return stationaryIndexRanges.FindAll(x => x.IndexStart < x.IndexEnd).ToList();
        }


        /// <summary>
        /// Finds the stationary ranges for the scrubber.
        /// </summary>
        /// <returns>A list containing the indexPoints for when the scrubber is stationary.</returns>
        /// <param name="inputsTimes">The inputs which should be accelerometer data.</param>
        /// <param name="batchTime">Batch time in seconds (the time for the batch size).</param>
        /// <param name="offset">Offset between each batch on which the residualSS is calculated.</param>
        private List<IndexPoint> FindStationaryRanges(List<TimePoint> inputsTimes, double batchTime, int offset)
        {
            List<IndexPoint> stationaryRanges = new List<IndexPoint>();
            bool hasInput = true;
            int i = 0;

            while (hasInput)
            {
                if (inputsTimes[i].Time <= batchTime / 2)
                {
                    //Increments i until i represents the index for the midpoint in the first batch.
                    i++;
                    continue;
                }

                double midPointTime = inputsTimes[i].Time;
                //Takes a batch containing datapoints for a batchTime period. Takes half batchTime before midpoint and half after.
                List<TimePoint> batchInputsTimes = inputsTimes.FindAll(x => x.Time >= midPointTime - batchTime / 2 && x.Time < midPointTime + batchTime / 2).ToList();

                double tendensySlope = CalculateTendencySlope(batchInputsTimes);
                double tendensyOffset = CalculateTendensyOffset(batchInputsTimes, tendensySlope);
                double residualSS = CalculateResidualSumOfSquares(batchInputsTimes, tendensySlope, tendensyOffset);



                if (residualSS < _pointResidualSSTreshold)
                {
                    //If the residualSS is under a previous set treshold, the point is said to be stationary.
                    stationaryRanges.Add(new IndexPoint(inputsTimes[i].Time, i));
                }

                if (inputsTimes.Last() == batchInputsTimes.Last())
                {
                    hasInput = false;
                }

                i += offset;
            }
            return stationaryRanges;
        }

        /// <summary>
        /// Returns a list containing indexes for when drifting is detected.
        /// </summary>
        private List<IndexRangePoint> FindDriftRanges(List<TimePoint> inputsTimes, double batchTime, int offset, int lastIndex)
        {
            List<IndexPoint> accelerationPointsList = FindAccelerationPoints(inputsTimes, batchTime, offset);

            List<IndexRangePoint> driftRanges = new List<IndexRangePoint>();

            int startIndex = 0;
            int endIndex = accelerationPointsList[0].Index;

            driftRanges.Add(new IndexRangePoint(0, accelerationPointsList[0].Index - 1));

            for (int i = 1; i < accelerationPointsList.Count; i++)
            {
                if (accelerationPointsList[i].Index == accelerationPointsList[i - 1].Index + 1)
                {
                    startIndex = accelerationPointsList[i].Index + 1;
                }
                else
                {
                    endIndex = accelerationPointsList[i].Index - 1;
                    if (startIndex != endIndex)
                    {
                        driftRanges.Add(new IndexRangePoint(startIndex, endIndex));
                    }
                }
            }

            driftRanges.Add(new IndexRangePoint(accelerationPointsList[accelerationPointsList.Count - 1].Index + 1, lastIndex));

            return driftRanges;
        }


        /// <summary>
        /// Finds times for when the difference in velocity is over a gradient treshold.
        /// </summary>
        private List<IndexPoint> FindAccelerationPoints(List<TimePoint> inputsTimes, double batchTime, int offset)
        {
            List<IndexPoint> listToReturn = new List<IndexPoint>();

            List<IndexPoint> slopeDifferencesList = CalculateSlopeDifferences(inputsTimes, batchTime, offset);

            foreach (IndexPoint slope in slopeDifferencesList)
            {
                if (Math.Abs(slope.Value) > _slopeDiffenceTreshold)
                {
                    listToReturn.Add(new IndexPoint(inputsTimes[slope.Index].Time, slope.Index));
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
        /// <param name="batchTime">The size of the ranges on which the slope will be calculated, the ranges will each be the items within batchTime.</param>
        /// <param name="offset">The offset between ranges on which the slope will be calculated</param>
        private List<IndexPoint> CalculateSlopeDifferences(List<TimePoint> inputsTimes, double batchTime, int offset)
        {
            List<TimePoint> inputTimesDyn = new List<TimePoint>();

            inputsTimes.ForEach(x => inputTimesDyn.Add(new TimePoint(x.Value, x.Time)));
            List<IndexPoint> listToReturn = new List<IndexPoint>();
            bool hasInput = true;

            double firstTime = inputTimesDyn.First().Time;
            inputTimesDyn.ForEach(x => x.Time = x.Time - firstTime);

            int i = 0;

            while (hasInput)
            {
                if (inputTimesDyn[i].Time < batchTime)
                {
                    i++;
                    continue;
                }

                double midPointTime = inputTimesDyn[i].Time;
                List<TimePoint> firstBatchList = inputTimesDyn.FindAll(x => x.Time >= midPointTime - batchTime && x.Time < midPointTime).ToList();
                double thresFirst = CalculateTendencySlope(firstBatchList);
                List<TimePoint> secondBatchList = inputTimesDyn.FindAll(x => x.Time >= midPointTime && x.Time < midPointTime + batchTime).ToList();
                double thresSecond = CalculateTendencySlope(secondBatchList);

                if (inputTimesDyn.Last() == secondBatchList.Last())
                {
                    hasInput = false;
                }

                listToReturn.Add(new IndexPoint(thresSecond - thresFirst, i));
                i++;
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
        private List<TimePoint> GetRunningAverageAcceleration(List<TimePoint> inputsTimes, int periodLength = 100)
        {
            List<double> inputs = inputsTimes.Select(x => x.Value).ToList();
            List<double> times = inputsTimes.Select(x => x.Time).ToList();

            List<TimePoint> listToReturn = new List<TimePoint>();

            List<double> averageList = CalculateRunningAverage(inputs);

            for (int i = 0; i < averageList.Count; i++)
            {
                listToReturn.Add(new TimePoint(averageList[i], times[i]));
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

        /// <summary>
        /// Calculates the residual coefficient, which is not the sum of Squared errors, but something alike, 
        /// that calculates the offset of the raw data to the tendensyline . 
        /// </summary>
        /// <returns>The residual coefficient.</returns>
        /// <param name="inputsTimes">Input which should be acceleration and times</param>
        /// <param name="slopeTendensy">Tendensy line slope.</param>
        /// <param name="offsetTendensy">Tendensy line offset.</param>
        private double CalculateResidualSumOfSquares(List<TimePoint> inputsTimes, double slopeTendensy, double offsetTendensy)
        {
            List<double> residualSSList = new List<double>();
            inputsTimes.ForEach(x => residualSSList.Add(Math.Pow(x.Value - (slopeTendensy * x.Time + offsetTendensy), 2)));
            double sSResidual = residualSSList.Sum();

            return sSResidual / inputsTimes.Count();
        }

        private double CalculateTendensyOffset(List<TimePoint> inputsTimes, double slope)
        {
            List<double> yAxis = inputsTimes.Select(x => x.Value).ToList();
            List<double> xAxis = inputsTimes.Select(x => x.Time).ToList();

            return (yAxis.Sum() - slope * xAxis.Sum()) / inputsTimes.Count();
        }

        /// <summary>
        /// Calculates the tendency slope.
        /// </summary>
        /// <returns>The tendency slope.</returns>
        /// <param name="inputsTimes">The data from which the slope will be calculated.</param>
        private double CalculateTendencySlope(List<TimePoint> inputsTimes)
        {
            List<double> inputs = inputsTimes.Select(x => x.Value).ToList();
            List<double> times = inputsTimes.Select(x => x.Time).ToList();

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
    }
}
