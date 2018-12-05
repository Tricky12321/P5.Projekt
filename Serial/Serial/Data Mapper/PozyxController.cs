using System;
using System.Collections.Generic;
using System.Linq;
using Serial.DynamicCalibrationName.Points;
using Serial.Utility;

namespace Serial.DataMapper
{
    public class PozyxController
    {
        private Load load;

        public PozyxController()
        {
            string filePath = Environment.CurrentDirectory;
            filePath += "/Test/virkelighedstro_3s_1_POZYX.csv";
            load = new Load(filePath);
            load.HandleCSV();
        }

        public List<TimePoint> CalculateVelocity()
        {
            List<TimePoint> velocityList = new List<TimePoint>();
            velocityList.Add(new TimePoint(0.0, 0.0));

            for (int i = 1; i < load.data.AllDataEntries.Count; i++)
            {
                DataEntry prewPoint = load.data.AllDataEntries.ToArray()[i-1];

                DataEntry currentPoint = load.data.AllDataEntries.ToArray()[i];

                while (prewPoint.PoZYX.X == currentPoint.PoZYX.X && prewPoint.PoZYX.Y == currentPoint.PoZYX.Y)
                {
                    i++;
                    currentPoint = load.data.AllDataEntries.ToArray()[i];
                }

                double timeDif = (currentPoint.PoZYX.TimeOfData - prewPoint.PoZYX.TimeOfData) / 1000;
                double distance = CalculateDistance(prewPoint.PoZYX.X, prewPoint.PoZYX.Y, currentPoint.PoZYX.X, currentPoint.PoZYX.Y);
                velocityList.Add(new TimePoint(1 / timeDif * distance, currentPoint.PoZYX.TimeOfData));
            }
            velocityList.ForEach(x => Console.WriteLine($"\"{(x.Time / 1000).ToString().Replace(',', '.')}\", \"{x.Value.ToString().Replace(',', '.')}\""));

            return velocityList;
        }

        public List<TimePoint> CalculateDistance()
        {
            List<TimePoint> distanceList = new List<TimePoint>();
            distanceList.Add(new TimePoint(0.0, 0.0));

            for (int i = 1; i < load.data.AllDataEntries.Count; i++)
            {
                DataEntry prewPoint = load.data.AllDataEntries.ToList()[i - 1];

                DataEntry currentPoint = load.data.AllDataEntries.ToList()[i];

                while (prewPoint.PoZYX.X == currentPoint.PoZYX.X && prewPoint.PoZYX.Y == currentPoint.PoZYX.Y)
                {
                    i++;
                    currentPoint = load.data.AllDataEntries.ToArray()[i];
                }

                double lastDistance = distanceList.Last().Value;
                double distance = CalculateDistance(prewPoint.PoZYX.X, prewPoint.PoZYX.Y, currentPoint.PoZYX.X, currentPoint.PoZYX.Y);
                distanceList.Add(new TimePoint(distance + lastDistance, currentPoint.PoZYX.TimeOfData));
            }
            distanceList.ForEach(x => Console.WriteLine($"\"{(x.Time / 1000).ToString().Replace(',', '.')}\", \"{x.Value.ToString().Replace(',', '.')}\""));

            return distanceList;
        }

        private double CalculateDistance(double startX, double startY, double endX, double endY)
        {
            return Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)) / 1000;
        }
    }
}
