using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial.Clustering
{
    public class DataPoint
    {
        public int Cluster;
        public int PointNumber;

        public DataPoint(int cluster, int pointNumber)
        {
            Cluster = cluster;
            PointNumber = pointNumber;
        }
    }
}
