using System;
using System.Collections.Generic;
using Serial.DynamicCalibrationName.Points;
using Serial.Clustering;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
namespace Serial.DynamicCalibrationName
{
	public class ClusteringDynamicCalibration : EMClustering, IAccelerationPointController
	{
		const int BatchSize = 300;
		const double ZeroThreashold = 0.5;
		const int MinimumGroupSize = 50;
		List<IndexRangePoint> DriftPoints;
		public ClusteringDynamicCalibration(string Training, string Test) : base(Training, Test)
		{
			Console.WriteLine("Starting cluster processing");
			List<DataPoint> dataPoints = this.GetClusters();
			List<List<DataPoint>> Clusters = new List<List<DataPoint>> {
				dataPoints.Where(x => x.clusterColor == ClusterColor.Blue).ToList(),
				dataPoints.Where(x => x.clusterColor == ClusterColor.Red).ToList(),
				dataPoints.Where(x => x.clusterColor == ClusterColor.Green).ToList()
			};
			var SortedClusters = Clusters.OrderBy(X => X.Average(Y=>Math.Abs(Y.SlopeDiff))).ToList();
			// Update the clusters datapoints with information about what they are. 
			SortedClusters[0].ForEach(X => X.clusterType = ClusterType.Drift);
			SortedClusters[1].ForEach(X => X.clusterType = ClusterType.Drift);
			SortedClusters[2].ForEach(X => X.clusterType = ClusterType.Acceleration);

			Console.WriteLine($"{SortedClusters[0][0].clusterColor} = {SortedClusters[0][0].clusterType}");
			Console.WriteLine($"{SortedClusters[1][0].clusterColor} = {SortedClusters[1][0].clusterType}");
			Console.WriteLine($"{SortedClusters[2][0].clusterColor} = {SortedClusters[2][0].clusterType}");


			DriftPoints = GetDriftRanges2(dataPoints);
			int count = DriftPoints.Count;
            for (int i = 0; i < count; i++)
			{
				if (i == 0) {
					DriftPoints[i].IndexEnd += BatchSize/2;
				} else{
					DriftPoints[i].IndexStart += BatchSize/2;
					DriftPoints[i].IndexEnd += BatchSize/2;
				}
				Console.WriteLine($"Drift from {DriftPoints[i].IndexStart} to {DriftPoints[i].IndexEnd}");
			}
		}

		private List<IndexRangePoint> GetDriftRanges2(List<DataPoint> dataPoints)
		{
			List<IndexRangePoint> Output = new List<IndexRangePoint>();
			int start = 0;
			int count = dataPoints.Count;
			int end = 0;
			bool Found = false;
            for (int i = 0; i < count; i++)
			{
				if (dataPoints[i].clusterType == ClusterType.Drift) {
					end = i;
					Found = true;
				} else {
					if (Found)
					{
						Output.Add(new IndexRangePoint(start, end));
						Found = false;
					}
					start = i;
				}
			}
			Output.Add(new IndexRangePoint(start, end));
			return Output;
		}

		private List<IndexRangePoint> GetDriftRanges(List<DataPoint> dataPoints)
		{
			List<IndexRangePoint> Output = new List<IndexRangePoint>();
			int start = 0;
			int end = 0;
			int count = dataPoints.Count;
			while (end < count)
			{
				try
				{


					var Acceleration = FindCompleteRange(start, dataPoints);
					var Deacceleration = FindCompleteRange(Acceleration.IndexEnd, dataPoints);
					start = Acceleration.IndexEnd;
					end = Deacceleration.IndexStart;
					Output.Add(new IndexRangePoint(start, end));
					start = end;
				}
				catch (Exception)
				{
					return Output;
				}
			}
			return Output;

		}

		private IndexRangePoint FindCompleteRange(int Start, List<DataPoint> dataPoints)
		{
			var Range = SearchForGroup(Start, ClusterType.Acceleration, dataPoints);
			Range = FindRangeOfAcceleration(Range, dataPoints);
			return Range;
		}

		private IndexRangePoint FindRangeOfAcceleration(IndexRangePoint ClusterRange, List<DataPoint> dataPoints)
		{
			int start = ClusterRange.IndexStart;
			int end = ClusterRange.IndexEnd;
			// Check if the range found is an acceleration or deaccleration
			// This is used to search forward, or backward.
			double Max = dataPoints.GetRange(start, end - start).Max(X => X.AX);
			double Min = dataPoints.GetRange(start, end - start).Min(X => X.AX);

			bool positive = (Max - Math.Abs(Min)) > 0;
			// Return the search forward, or backward
			if (positive)
			{
				return new IndexRangePoint(start, FindZeroForwards(end, dataPoints));
			}
			else
			{
				return new IndexRangePoint(FindZeroBackwards(start, dataPoints), end);
			}
		}


		/// <summary>
		/// Searches for a given cluster group, only searches forward. Returns start and end of cluster
		/// </summary>
		/// <returns>The for group.</returns>
		/// <param name="start">Start.</param>
		/// <param name="TypeToSearchFor">Type to search for.</param>
		/// <param name="dataPoints">Data points.</param>
		private IndexRangePoint SearchForGroup(int start, ClusterType TypeToSearchFor, List<DataPoint> dataPoints)
		{
			int count = dataPoints.Count;
			for (int i = start; i < count; i++)
			{
				if (dataPoints[i].clusterType == TypeToSearchFor)
				{
					bool ClusterGroupFound = true;
					int j = i;
					int MaxRange = i + MinimumGroupSize;
					for (; j < i + MinimumGroupSize; j++)
					{
						if (dataPoints[j].clusterType != TypeToSearchFor)
						{
							ClusterGroupFound = false;
							break;
						}
					}
					// If a group is found
					if (ClusterGroupFound)
					{
						// Keep searching until there is no longer a group.
						while (ClusterGroupFound)
						{
							for (; j < count; j++)
							{
								if (!ClusterGroupFound)
								{
									break;
								}
								if (dataPoints[j].clusterType != TypeToSearchFor)
								{
									ClusterGroupFound = false;
									break;
								}
							}
						}
						// When the total size of the group has been found, return the range of the group.
						return new IndexRangePoint(i, j);
					}
					else
					{
						i = j;
					}
				}
			}
			return null;
		}

		private int FindZeroForwards(int Start, List<DataPoint> dataPoints)
		{
			int count = dataPoints.Count;
			for (int i = Start; i < count; i++)
			{
				if ((dataPoints[i].AX > 0 && dataPoints[i + 1].AX < 0) || (dataPoints[i].AX < 0 && dataPoints[i + 1].AX > 0))
				{
					return i;
				}
			}
			throw new Exception("No stop found!");
		}

		private int FindZeroBackwards(int Start, List<DataPoint> dataPoints)
		{
			for (int i = Start; i > 0; i--)
			{
				if ((dataPoints[i].AX > 0 && dataPoints[i - 1].AX < 0) || (dataPoints[i].AX < 0 && dataPoints[i - 1].AX > 0))
				{
					return i;
				}
			}
			throw new Exception("No stop found!");
		}



		public List<IndexRangePoint> GetDriftRanges()
		{
			return DriftPoints;
		}




	}
}
