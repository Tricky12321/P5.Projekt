using System;
using System.Collections.Generic;
using Serial.DynamicCalibrationName.Points;

namespace Serial.DynamicCalibration
{
    public interface IAccelerationPointController
    {
        List<IndexRangePoint> GetDriftRanges();
        void InsertValocityList(List<TimePoint> velocityList);
    }
}
