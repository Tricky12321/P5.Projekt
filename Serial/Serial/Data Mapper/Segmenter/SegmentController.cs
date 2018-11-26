using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
namespace Serial.DataMapper.Segmenter
{
	public static class SegmentController
	{

		public static ConcurrentQueue<DataEntry> SegmentData(ConcurrentQueue<DataEntry> dataEntries, int NumPrSegment = 50)
		{
			ConcurrentQueue<DataEntry> SegmentedData = new ConcurrentQueue<DataEntry>(dataEntries);
			ConcurrentQueue<DataEntry> OutputSegments = new ConcurrentQueue<DataEntry>();
			int NumOfSegments = Convert.ToInt32(Math.Ceiling((decimal)SegmentedData.Count / NumPrSegment));
			var CurrentSegment = new List<DataEntry>();
			for (int i = 0; i < NumOfSegments; i++)
			{
				DataEntry SingleElement;

				for (int j = 0; j < NumPrSegment; j++)
				{
					SegmentedData.TryDequeue(out SingleElement);
					CurrentSegment.Add(SingleElement);
				}
				try
				{
					double Time = Convert.ToInt32(Math.Round(CurrentSegment.Average(X => X.INS_Accelerometer.TimeOfData), 0));
					double ACC_X = CurrentSegment.Average(X => X.INS_Accelerometer.X);
					double ACC_Y = CurrentSegment.Average(X => X.INS_Accelerometer.Y);
					double ACC_Z = CurrentSegment.Average(X => X.INS_Accelerometer.Z);
					double GYR_X = CurrentSegment.Average(X => X.INS_Gyroscope.X);
					double GYR_Y = CurrentSegment.Average(X => X.INS_Gyroscope.Y);
					double GYR_Z = CurrentSegment.Average(X => X.INS_Gyroscope.Z);
					double ANGLE = CurrentSegment.Average(X => X.INS_Angle);
					OutputSegments.Enqueue(new DataEntry(null, new XYZ(ACC_X, ACC_Y, ACC_Z, Time), new XYZ(GYR_X, GYR_Y, GYR_Z, Time), ANGLE));
				}
				catch (NullReferenceException)
				{

				}
				finally
				{
					CurrentSegment.Clear();
				}
			}
			return OutputSegments;
		}

	}
}
