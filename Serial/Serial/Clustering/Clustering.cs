using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using weka.clusterers;
using weka.core;
using java.io;

namespace Serial.Clustering
{
    public class Clustering
    {
        public EM eM = new EM();
        public Instances dataSet;

        public Clustering(string filePath)
        {
            EMAlgorithm(filePath);
        }

        public void EMAlgorithm(string filePath)
        {
            weka.core.converters.CSVLoader cSVLoader = new weka.core.converters.CSVLoader();
            File file = new File(filePath);

            cSVLoader.setSource(file);
            dataSet = cSVLoader.getDataSet(); ;

            #region EM SETTINGS
            eM.setNumClusters(3);
            eM.setSeed(100);
            eM.setNumFolds(10);
            eM.setMaxIterations(100);
            eM.setMaximumNumberOfClusters(-1);
            eM.setNumExecutionSlots(1);
            eM.setNumKMeansRuns(10);
            #endregion
            eM.buildClusterer(dataSet);
 
        }

        public List<DataPoint> GetClusters()
        {
            List<DataPoint> dataPoints = new List<DataPoint>();
            int lengthOfDataSet = dataSet.size();

            for (int i = 0; i < lengthOfDataSet; ++i)
            {
                if (dataSet.get(i) != null)
                {
                    int cluster = eM.clusterInstance(dataSet.get(i));
                    DataPoint dataPoint = new DataPoint(cluster, i);
                    dataPoints.Add(dataPoint);
                }
            }
            return dataPoints;
        }
    }

}
