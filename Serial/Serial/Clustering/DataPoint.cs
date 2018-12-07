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
		public ClusterType clusterType;
		public double SlopeVarians;
		public double Slope;
		public double AX;
		public double[] Weights;

		public DataPoint(ClusterColor clusterColor, int pointNumber, double[] Weights)
        {
			this.Weights = Weights;
			this.clusterColor = clusterColor;
            PointNumber = pointNumber;
        }

    }
}
