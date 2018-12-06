using System;
using System.Collections.Generic;
using Serial.DynamicCalibrationName.Points;
using Serial.Clustering;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Serial.DynamicCalibrationName
{
	public class ClusteringDynamicCalibration : EMClustering, IAccelerationPointController
    {
		public ClusteringDynamicCalibration(string Path) : base (Path)
        {
			
        }

		public List<IndexRangePoint> GetDriftRanges()
		{
			return new List<IndexRangePoint>();	
		}




	}
}
