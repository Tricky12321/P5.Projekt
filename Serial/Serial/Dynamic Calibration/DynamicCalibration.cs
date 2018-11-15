using System;
using System.Collections.Generic;
using System.Linq;

namespace Serial.DynamicCalibrationName
{
    public class DynamicCalibration
    {
        const double _runningAverageBatchTime = 1.0;
        const double _slopeDiffenceTreshold = 0.3;
        const double _pointCoefficientOfDeterminitionTreshold = 0.08;
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

        public List<Tuple<double, double>> GetTupleListWithOneAxisAndTimes(List<XYZ> inputsTimes, char axis = 'X')
        {
            List<Tuple<double, double>> listToReturn = new List<Tuple<double, double>>();
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
                listToReturn.Add(new Tuple<double, double>(axisValue, input.TimeOfData));
            }
            return listToReturn;
        }

        public List<Tuple<double, double>> CalculatePosition(List<Tuple<double, double>> inputTimes)
        {
            List<double> inputs = inputTimes.Select(x => x.Item1).ToList();
            List<double> times = inputTimes.Select(x => x.Item2).ToList();

            List<Tuple<double, double>> distanceList = new List<Tuple<double, double>>();
            distanceList.Add(new Tuple<double, double>(0.0, 0.0));

            for (int i = 1; i < inputs.Count; i++)
            {
                double newDistance = (times[i] - times[i - 1]) * (inputs[i] + inputs[i - 1]) / 2 + distanceList[i - 1].Item1;
                double newTime = times[i];
                distanceList.Add(new Tuple<double, double>(newDistance, newTime));

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
        public List<Tuple<double, double>> CalculateDynamicVelocityList(List<Tuple<double, double>> inputTimes, bool useRunningAverage = true)
        {
            List<double> inputs = inputTimes.Select(x => x.Item1).ToList();
            List<double> times = inputTimes.Select(x => x.Item2).ToList();

            List<Tuple<double, double>> dynamicVelocityList = new List<Tuple<double, double>>();

            List<Tuple<double, double>> velocityList = useRunningAverage ? GetRunningAverageAcceleration(inputTimes) : inputTimes;

            List<Tuple<double, int>> accelerationPointsList = FindAccelerationPoints(velocityList, _runningAverageBatchTime, _gradientCalculationOffset);
            List<Tuple<int, int>> driftingIndexesList = FindDriftRanges(accelerationPointsList, velocityList.Count - 1);

            var tets = FindStationaryRanges(GetTupleListWithOneAxisAndTimes(_accelerationList), 3.0, 1);

            foreach (Tuple<double,double> point in velocityList)
            {
                dynamicVelocityList.Add(new Tuple<double, double>(point.Item1, point.Item2));
            }

            for (int i = 0; i < driftingIndexesList.Count; i++)
            {
                int startIndex = driftingIndexesList[i].Item1;
                int endIndex = driftingIndexesList[i].Item2;
                List<Tuple<double, double>> driftVelocity = dynamicVelocityList.GetRange(startIndex, endIndex - startIndex);

                double slope = CalculateTendencySlope(driftVelocity);

                for (int j = startIndex; j < velocityList.Count; j++)
                {
                    var test = slope * (dynamicVelocityList[j].Item2 - dynamicVelocityList[startIndex].Item2);
                    dynamicVelocityList[j] = new Tuple<double, double>(dynamicVelocityList[j].Item1 - test, dynamicVelocityList[j].Item2);
                }
            }
            return dynamicVelocityList;
        }

        private List<Tuple<double, int>> FindStationaryRanges(List<Tuple<double, double>> inputsTimes, double batchTime, int offset)
        {
            List<double> inputs = inputsTimes.Select(x => x.Item1).ToList();
            List<double> times = inputsTimes.Select(x => x.Item2).ToList();
            List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();

            if (inputs.Count == times.Count)
            {
                bool hasInput = true;

                int i = 0;
                while (hasInput)
                {
                    double startFromTime = inputsTimes[i * offset].Item2;
                    List<Tuple<double, double>> batchInputsTimes = inputsTimes.FindAll(x => x.Item2 >= startFromTime && x.Item2 < startFromTime + batchTime).ToList();

                    //batchInputsTimes.ForEach(x => Console.WriteLine($"\"{x.Item2.ToString().Replace(',', '.')}\",\"{x.Item1.ToString().Replace(',', '.')}\""));

                    double tendensySlope = CalculateTendencySlope(batchInputsTimes);
                    double tendensyOffset = CalculateTendensyOffset(batchInputsTimes, tendensySlope);

                    double coefficientOfDeterminition = CalculateCoefficientOfDeterminition(batchInputsTimes, tendensySlope, tendensyOffset);
                    if (coefficientOfDeterminition > _pointCoefficientOfDeterminitionTreshold)
                    {
                        Console.WriteLine(startFromTime);
                        listToReturn.Add(new Tuple<double, int>(times[i * offset], i * offset));
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
                    if (startIndex != endIndex)
                    {
                        driftRanges.Add(new Tuple<int, int>(startIndex, endIndex));
                    }
                }
            }

            driftRanges.Add(new Tuple<int, int>(accelerationPointsList[accelerationPointsList.Count - 1].Item2 + 1, lastIndex));

            return driftRanges;
        }


        /// <summary>
        /// Finds times for when the difference in velocity is over a gradient treshold.
        /// </summary>
        private List<Tuple<double, int>> FindAccelerationPoints(List<Tuple<double, double>> inputsTimes, double batchTime, int offset)
        {
            List<double> points = inputsTimes.Select(x => x.Item1).ToList();
            List<double> times = inputsTimes.Select(x => x.Item2).ToList();

            List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();

            List<Tuple<double, int>> slopeDifferencesList = CalculateSlopeDifferences(inputsTimes, batchTime, offset);

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
        /// <param name="batchTime">The size of the ranges on which the slope will be calculated, the ranges will each be the items within batchTime.</param>
        /// <param name="offset">The offset between ranges on which the slope will be calculated</param>
        private List<Tuple<double, int>> CalculateSlopeDifferences(List<Tuple<double, double>> inputsTimes, double batchTime, int offset)
        {
            List<Tuple<double, int>> listToReturn = new List<Tuple<double, int>>();
            bool hasInput = true;

            int i = 0;

            while (hasInput)
            { 
                if (inputsTimes[i].Item2 < batchTime)
                {
                    i++;
                    continue;
                }
                double midPointTime = inputsTimes[i].Item2;
                List<Tuple<double, double>> firstBatchList = inputsTimes.FindAll(x => x.Item2 >= midPointTime - batchTime && x.Item2 < midPointTime).ToList();
                double thresFirst = CalculateTendencySlope(firstBatchList);
                List<Tuple<double, double>> secondBatchList = inputsTimes.FindAll(x => x.Item2 >= midPointTime && x.Item2 < midPointTime + batchTime).ToList();
                double thresSecond = CalculateTendencySlope(secondBatchList);

                if (inputsTimes.Last() == secondBatchList.Last())
                {
                    hasInput = false;
                }

                listToReturn.Add(new Tuple<double, int>(thresSecond - thresFirst, i));
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
        private List<Tuple<double, double>> GetRunningAverageAcceleration(List<Tuple<double, double>> inputsTimes, int periodLength = 100)
        {
            List<double> inputs = inputsTimes.Select(x => x.Item1).ToList();
            List<double> times = inputsTimes.Select(x => x.Item2).ToList();

            List<Tuple<double, double>> listToReturn = new List<Tuple<double, double>>();

            List<double> averageList = CalculateRunningAverage(inputs);

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

        private double CalculateCoefficientOfDeterminition(List<Tuple<double, double>> inputsTimes, double slopeTendensy, double offsetTendensy)
        {
            List<double> yAxis = inputsTimes.Select(x => x.Item1).ToList();
            List<double> xAxis = inputsTimes.Select(x => x.Item2).ToList();
            double yAxisAverage = yAxis.Average();

            List<double> totalSSList = new List<double>();
            yAxis.ForEach(x => totalSSList.Add(Math.Pow((x - yAxisAverage), 2)));
            double sStotal = totalSSList.Sum();

            List<double> residualSSList = new List<double>();
            inputsTimes.ForEach(x => residualSSList.Add(Math.Pow(x.Item1 - (slopeTendensy * x.Item2 + offsetTendensy), 2)));
            double sSResidual = residualSSList.Sum();

            return 1 - sSResidual / sStotal;

            /*
            List<double> xTimesYList = new List<double>();
            List<double> xList = new List<double>();
            List<double> xSquaredList = new List<double>();
            List<double> yList = new List<double>();
            List<double> ySquaredList = new List<double>();
            int n = inputsTimes.Count;

            foreach (Tuple<double, double> input in inputsTimes)
            {
                xTimesYList.Add(input.Item1 * input.Item2);
                xList.Add(input.Item2);
                yList.Add(input.Item1);
                xSquaredList.Add(Math.Pow(input.Item2, 2));
                ySquaredList.Add(Math.Pow(input.Item1, 2));
            }
            double r = 1 - ((n * xTimesYList.Sum() - (xList.Sum() * yList.Sum()))
                        / Math.Sqrt((n * xSquaredList.Sum() - Math.Pow(xList.Sum(), 2)) * n * ySquaredList.Sum() - Math.Pow(yList.Sum(), 2)));

            return Math.Pow(r, 2);*/
        }

        private double CalculateTendensyOffset(List<Tuple<double, double>> inputsTimes, double slope)
        {
            List<double> yAxis = inputsTimes.Select(x => x.Item1).ToList();
            List<double> xAxis = inputsTimes.Select(x => x.Item2).ToList();

            return (yAxis.Sum()-slope*xAxis.Sum()) / inputsTimes.Count();
        }


        /// <summary>
        /// Returns the slope for the points given as parameter.
        /// </summary>
        private double CalculateTendencySlope(List<Tuple<double, double>> inputsTimes)
        {
            List<double> inputs = inputsTimes.Select(x => x.Item1).ToList();
            List<double> times = inputsTimes.Select(x => x.Item2).ToList();

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
