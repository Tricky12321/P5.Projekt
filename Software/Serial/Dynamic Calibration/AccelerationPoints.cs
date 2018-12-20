using System;
using System.Collections.Generic;
namespace Serial.DynamicCalibrationName
{
    public class AccelerationPoints
    {
        public List<AccelerationPoint> XAccelerationPoints = new List<AccelerationPoint>();
        public List<AccelerationPoint> YAccelerationPoints = new List<AccelerationPoint>();
        public List<AccelerationPoint> ZAccelerationPoints = new List<AccelerationPoint>();
    }

    public class AccelerationPoint
    {
        public double Value;
        public int Index;

        public AccelerationPoint(double value, int index)
        {
            Value = value;
            Index = index;
        }
    }
}
