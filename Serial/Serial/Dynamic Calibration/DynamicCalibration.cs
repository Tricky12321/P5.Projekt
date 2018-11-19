using System;
using System.Collections.Generic;
using System.Linq;
using Serial.DynamicCalibrationName.Points;

namespace Serial.DynamicCalibrationName
{
    public class DynamicCalibration
    {
        const double _runningAverageBatchTime = 1.0;
        private double _slopeDiffenceTreshold = 0.2;                      //The lower the value is, the more acceleration points will be found.
        private double _pointCoefficientOfDeterminitionTreshold;   //defines the upper value for when the scrubber is stationary.
        const double _stationaryDetectionBatchTime = 2.0;
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
            foreach(XYZ input in inputsTimes)
            {
                double axisValue;
                switch(axis){
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
            List<double> inputs = inputTimes.Select(x => x.Value).ToList();
            List<double> times = inputTimes.Select(x => x.Time).ToList();

            List<TimePoint> distanceList = new List<TimePoint>();
            distanceList.Add(new TimePoint(0.0, 0.0));

            for (int i = 1; i < inputs.Count; i++)
            {
                double newDistance = (times[i] - times[i - 1]) * (inputs[i] + inputs[i - 1]) / 2 + distanceList[i - 1].Value;
                double newTime = times[i];
                distanceList.Add(new TimePoint(newDistance, newTime));

                //(t1-t0) * (v1+v0)/2 + d0 
            }
            return distanceList;
        }

        public void CalibrateCoefficientofDeterminition(double calibrationTime)
        {
            List<TimePoint> accelerationCalibrationBatch = GetPointListWithOneAxisAndTimes(_accelerationList.TakeWhile(x => x.TimeOfData <= calibrationTime).ToList());

            double slope = CalculateTendencySlope(accelerationCalibrationBatch);
            double offset = CalculateTendensyOffset(accelerationCalibrationBatch, slope);
            double errorMarginCalibration = CalculateResidualCoefficient(accelerationCalibrationBatch, slope, offset);
            _pointCoefficientOfDeterminitionTreshold = errorMarginCalibration * 1.2;
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
        public List<TimePoint> CalculateDynamicVelocityList(List<TimePoint> inputTimes, bool useRunningAverage = false, bool useDriftCalibration = true, bool useStationaryDetection = false)
        {
            List<TimePoint> dynamicVelocityList = new List<TimePoint>();
            List<TimePoint> velocityList = useRunningAverage ? GetRunningAverageAcceleration(inputTimes) : inputTimes;

            List<IndexRangePoint> driftingIndexesList = FindDriftRanges(velocityList, _runningAverageBatchTime, _gradientCalculationOffset, velocityList.Count - 1);

            foreach (TimePoint point in velocityList)
            {
                dynamicVelocityList.Add(new TimePoint(point.Value, point.Time));
            }

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
                }
            }

            #endregion
            //Done trying to remove drift/

            #region StationaryCalibrate

            if (useStationaryDetection)
            {
                List<IndexRangePoint> stationaryIndexList = GetStationaryIndexes(GetPointListWithOneAxisAndTimes(_accelerationList), _stationaryDetectionBatchTime, _gradientCalculationOffset);
                List<IndexRangePoint> drivingIndexList = GetDrivingRangesFromStationaryIndex(stationaryIndexList);

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

                        dynamicVelocityList[j] = new TimePoint(dynamicVelocityList[j].Value - slopeOffset - startPointOffset, dynamicVelocityList[j].Time);
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
        /// Finds the stationary ranges.
        /// </summary>
        /// <returns>The stationary ranges.</returns>
        /// <param name="inputsTimes">Inputs times.</param>
        /// <param name="batchTime">Batch time in seconds (the time for the batch size).</param>
        /// <param name="offset">Offset.</param>
        private List<IndexPoint> FindStationaryRanges(List<TimePoint> inputsTimes, double batchTime, int offset)
        {
            List<double> inputs = inputsTimes.Select(x => x.Value).ToList();
            List<double> times = inputsTimes.Select(x => x.Time).ToList();

            List<IndexPoint> listToReturn = new List<IndexPoint>();

            if (inputs.Count == times.Count)
            {
                bool hasInput = true;

                int i = 0;
                while (hasInput)
                {
                    if (inputsTimes[i].Time < batchTime / 2)
                    {
                        i++;
                        continue;
                    }

                    double midPointTime = inputsTimes[i * offset].Time;
                    List<TimePoint> batchInputsTimes = inputsTimes.FindAll(x => x.Time >= midPointTime - batchTime / 2 && x.Time < midPointTime + batchTime / 2).ToList();
                    
                    double tendensySlope = CalculateTendencySlope(batchInputsTimes);
                    double tendensyOffset = CalculateTendensyOffset(batchInputsTimes, tendensySlope);

                    double coefficientOfDeterminition = CalculateResidualCoefficient(batchInputsTimes, tendensySlope, tendensyOffset);
                    //Console.WriteLine($"\"{inputsTimes[i].Item2.ToString().Replace(",",".")}\",\"{coefficientOfDeterminition.ToString().Replace(",",".")}\"");

                    if (coefficientOfDeterminition < _pointCoefficientOfDeterminitionTreshold)
                    {
                        listToReturn.Add(new IndexPoint(times[i * offset], i * offset));
                    }

                    if (inputsTimes.Last() == batchInputsTimes.Last())
                    {
                        hasInput = false;
                    }

                    i++;
                }
                return listToReturn;
            }
            throw new InvalidInputException("FindStationaryRangesRanges got a different amount of times and inputs, they should be that same");
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
            List<double> times = inputsTimes.Select(x => x.Time).ToList();

            List<IndexPoint> listToReturn = new List<IndexPoint>();

            List<IndexPoint> slopeDifferencesList = CalculateSlopeDifferences(inputsTimes, batchTime, offset);

            foreach(IndexPoint slope in slopeDifferencesList)
            {
                if (Math.Abs(slope.Value) > _slopeDiffenceTreshold){
                    listToReturn.Add(new IndexPoint(times[slope.Index], slope.Index));
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
            List<IndexPoint> listToReturn = new List<IndexPoint>();
            bool hasInput = true;

            int i = 0;

            while (hasInput)
            { 
                if (inputsTimes[i].Time < batchTime)
                {
                    i++;
                    continue;
                }

                double midPointTime = inputsTimes[i].Time;
                List<TimePoint> firstBatchList = inputsTimes.FindAll(x => x.Time >= midPointTime - batchTime && x.Time < midPointTime).ToList();
                double thresFirst = CalculateTendencySlope(firstBatchList);
                List<TimePoint> secondBatchList = inputsTimes.FindAll(x => x.Time >= midPointTime && x.Time < midPointTime + batchTime).ToList();
                double thresSecond = CalculateTendencySlope(secondBatchList);

                if (inputsTimes.Last() == secondBatchList.Last())
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
        /// <param name="slopeTendensy">Tendensy line offset.</param>
        /// <param name="offsetTendensy">Tendensy line offset.</param>
        private double CalculateResidualCoefficient(List<TimePoint> inputsTimes, double slopeTendensy, double offsetTendensy)
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

            return (yAxis.Sum()-slope*xAxis.Sum()) / inputsTimes.Count();
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
