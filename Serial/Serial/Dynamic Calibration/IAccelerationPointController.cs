using System;
using System.Collections.Generic;
using Serial.DynamicCalibrationName.Points;

namespace Serial.DynamicCalibrationName
{
    public interface IAccelerationPointController
    {
        List<IndexRangePoint> GetDriftRanges();
    }
}
