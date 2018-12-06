using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial.Clustering
{
    public class DataPoint
    {
        public int PointNumber;
		public ClusterColor clusterColor;

		public DataPoint(ClusterColor clusterColor, int pointNumber)
        {
			this.clusterColor = clusterColor;
            PointNumber = pointNumber;
        }

    }
}
