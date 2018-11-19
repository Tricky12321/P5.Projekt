using System;
namespace Serial.DynamicCalibrationName.Points
{
    public class IndexPoint
    {
        public double Value;
        public int Index;

        public IndexPoint(double value, int index)
        {
            Index = index;
            Value = value;
        }
    }

    public class TimePoint
    {
        public double Value;
        public double Time;

        public TimePoint(double value, double time)
        {
            Value = value;
            Time = time;
        }
    }

    public class IndexRangePoint
    {
        public int IndexStart;
        public int IndexEnd;

        public IndexRangePoint(int indexStart, int indexEnd)
        {
            IndexStart = indexStart;
            IndexEnd = indexEnd;
        }
    }
}
