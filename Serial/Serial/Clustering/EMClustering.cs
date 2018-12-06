using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using weka.clusterers;
using weka.core;
using java.io;
using Serial.DynamicCalibrationName;
namespace Serial.Clustering
{
	public enum ClusterColor
	{
		Blue = 0, Red = 1, Green = 2
	}

	public enum ClusterType
	{
		Still, Drift, Acceleration, Deacceleration, Noise, Unknown
	}

	public class EMClustering
	{
		public EM eM = new EM();
		public Instances dataSet;

		public EMClustering()
		{

		}

		public EMClustering(string filePath)
		{
			EMAlgorithm(filePath);
		}

		public void EMAlgorithm(string filePath)
		{
			weka.core.converters.CSVLoader cSVLoader = new weka.core.converters.CSVLoader();
			File file = new File(filePath);

			cSVLoader.setSource(file);
			dataSet = cSVLoader.getDataSet(); ;

			eM.setNumClusters(3);
			eM.setSeed(100);
			eM.setNumFolds(10);
			eM.setMaxIterations(100);
			eM.setMaximumNumberOfClusters(-1);
			eM.setNumExecutionSlots(1);
			eM.setNumKMeansRuns(10);

			eM.buildClusterer(dataSet);

		}

		public List<DataPoint> GetClusters()
		{
			List<DataPoint> dataPoints = new List<DataPoint>();
			int lengthOfDataSet = dataSet.size();

			for (int i = 0; i < lengthOfDataSet; ++i)
			{
				ClusterColor cluster = (ClusterColor)eM.clusterInstance(dataSet.get(i));

				DataPoint dataPoint = new DataPoint(cluster, i);
				dataPoints.Add(dataPoint);
			}
			return dataPoints;
		}
	}

}
