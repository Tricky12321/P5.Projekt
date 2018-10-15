using System;
namespace Serial
{
    public class DataPoint
    {
        public XYZ Coordinates;
		public double TimeInSec;

        public DataPoint(XYZ coordinates, double timeInSec)
        {
            Coordinates = coordinates;
            TimeInSec = timeInSec;
        }
    }
}