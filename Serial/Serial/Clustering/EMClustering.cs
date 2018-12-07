using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using weka.clusterers;
using weka.core;
using java.io;
using Serial.DynamicCalibrationName;
using Serial.Utility;

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
		private string path;
		public EMClustering(string filePath)
		{
			path = filePath;
			EMAlgorithm(filePath);
		}

		public void EMAlgorithm(string filePath)
		{

			weka.core.converters.CSVLoader cSVLoader = new weka.core.converters.CSVLoader();
			File file = new File(filePath);

			cSVLoader.setSource(file);
			dataSet = cSVLoader.getDataSet();

			eM.setNumClusters(3);
			eM.setSeed(100);
			eM.setNumFolds(10);
			eM.setMaxIterations(100);
			eM.setMaximumNumberOfClusters(-1);
			eM.setNumExecutionSlots(1);
			eM.setNumKMeansRuns(10);

			eM.buildClusterer(dataSet);
		}

		private List<Tuple<double, double, double>> GetColums(string FilePath)
		{
			string[] FilePathSplit = FilePath.Split('.');
			FilePath = FilePathSplit[0] + "_WITH_X." + FilePathSplit[1];
			List<Tuple<double, double, double>> Output = new List<Tuple<double, double, double>>();
			List<string> Lines = System.IO.File.ReadAllLines(FilePath).ToList();
			Lines.RemoveAt(0);
            foreach (var Line in Lines)
			{
				string[] LineSplit = Line.Replace("\"","").Split(',');
				double AX = Convert.ToDouble(LineSplit[0]);
				double SlopeVarians = Convert.ToDouble(LineSplit[1]);
				double Slope = Convert.ToDouble(LineSplit[2]);
				Output.Add(new Tuple<double, double, double>(AX, SlopeVarians, Slope));
			}
			return Output;
		}

		public List<DataPoint> GetClusters()
		{
			List<Tuple<double, double, double>> DataValues = GetColums(path);
			List<DataPoint> dataPoints = new List<DataPoint>();
			int lengthOfDataSet = dataSet.size();

			for (int i = 0; i < lengthOfDataSet; ++i)
			{
				ClusterColor cluster = (ClusterColor)eM.clusterInstance(dataSet.get(i));
				var test = eM.clusterPriors();
				DataPoint dataPoint = new DataPoint(cluster, i, eM.distributionForInstance(dataSet.get(i)));

				dataPoint.AX = DataValues[i].Item1;
				dataPoint.SlopeVarians = DataValues[i].Item2;
				dataPoint.Slope = DataValues[i].Item3;
				dataPoints.Add(dataPoint);
			}
			return dataPoints;
		}
        
	}

}
