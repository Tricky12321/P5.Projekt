using System;
using Serial.DataMapper;
using Serial.Menu;
using Serial.Utility;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using Serial.DynamicCalibrationName.Points;

namespace Serial.CSVWriter
{
	public class CSVWriterController
	{
		DataMapper.DataMapper currentDataMapper;
		public string FileName = "";

		string INSFile => FileName + "_INS.csv";
        string DYNAMICCALIBFILE => FileName + "_DYNAM_CALI.csv";
        string NoCalibrateINSFile => FileName + "_INS_NOCALIBRATE.csv";
		string POZYXFile => FileName + "_POZYX.csv";
		string INSKalmanFile => FileName + "_INS_KALMAN.csv";
		string INSRollingAverageFile => FileName + "_INS_RA.csv";

		public CSVWriterController(DataMapper.DataMapper dataMapper, string Name)
		{
			currentDataMapper = dataMapper;
			FileName = Name;
			DeleteOldFiles();
		}

        public CSVWriterController( string Name)
        {
            FileName = Name;
            DeleteOldFiles();
        }

        private void DeleteOldFiles()
		{
			if (File.Exists(INSFile))
			{
				File.Delete(INSFile);
			}

			if (File.Exists(INSKalmanFile))
			{
				File.Delete(INSKalmanFile);
			}

			if (File.Exists(INSRollingAverageFile))
			{
				File.Delete(INSRollingAverageFile);
			}

			if (File.Exists(POZYXFile))
			{
				File.Delete(POZYXFile);
			}
		}

		private List<double> PrepareNormal()
		{
			List<XYZ> Accelerometer = new List<XYZ>();
			List<XYZ> GyroScope = new List<XYZ>();
			List<XYZ> Pozyx = new List<XYZ>();
			List<double> Angles = new List<double>();
			foreach (var DataEntryElement in currentDataMapper.AllDataEntries)
			{
				Accelerometer.Add(DataEntryElement.INS_Accelerometer);
				GyroScope.Add(DataEntryElement.INS_Gyroscope);
				Pozyx.Add(DataEntryElement.PoZYX);
				Angles.Add(DataEntryElement.INS_Angle);
			}
			WriteNormal(Accelerometer, GyroScope, Pozyx, Angles);
			WritePozyx(Pozyx);
			return Angles;
		}

        public void DynamicToCSV(List<TimePoint> inputList)
        {
            using (StreamWriter FileWriter = File.AppendText(DYNAMICCALIBFILE))
            {
                FileWriter.WriteLine($"Timer,Value");
                int DataCount = inputList.Count;
                for (int i = 0; i < DataCount; i++)
                {
                    if (inputList[i] != null)
                    {
                        FileWriter.WriteLine($"\"{inputList[i].Time}\"," +
                                             $"\"{inputList[i].Value}\"");
                    }
                }
                FileWriter.Close();
            }
        }

		private void WriteNormal(List<XYZ> Accelerometer, List<XYZ> GyroScope, List<XYZ> Pozyx, List<double> Angles)
		{
			using (StreamWriter FileWriter = File.AppendText(INSFile))
			{
				FileWriter.WriteLine($"Timer,AX,AY,AZ,GX,GY,GZ,A");
				int DataCount = GyroScope.Count;
				for (int i = 0; i < DataCount; i++)
				{
					if (GyroScope[i] != null && Accelerometer[i] != null)
					{
						FileWriter.WriteLine($"\"{GyroScope[i].TimeOfData}\"," +
											 $"\"{Accelerometer[i].X}\"," +
											 $"\"{Accelerometer[i].Y}\"," +
											 $"\"{Accelerometer[i].Z}\"," +
											 $"\"{GyroScope[i].X}\"," +
											 $"\"{GyroScope[i].Y}\"," +
											 $"\"{GyroScope[i].Z}\"," +
											 $"\"{Angles[i]}\"");
					}
				}
				FileWriter.Close();
			}
		}

		private void WritePozyx(List<XYZ> Pozyx) {
			using (StreamWriter FileWriter = File.AppendText(POZYXFile))
            {
                FileWriter.WriteLine($"Timer,X,Y,Z");

                foreach (var Data in Pozyx)
                {
                    if (Data != null)
                    {
                        FileWriter.WriteLine($"\"{Data.TimeOfData}\"," +
                                             $"\"{Data.X}\"," +
                                             $"\"{Data.Y}\"," +
                                             $"\"{Data.Z}\"");
                    }
                }
                FileWriter.Close();
            }
		}

		private void WriteKalman(List<double> Angles)
		{
			if (currentDataMapper.Kalman)
			{
				List<XYZ> Kalman_Accelerometer = new List<XYZ>();
				List<XYZ> Kalman_Gyroscope = new List<XYZ>();
				if (currentDataMapper.Kalman)
				{
					foreach (var Kalman in currentDataMapper.KalmanData)
					{
						Kalman_Accelerometer.Add(Kalman.Item1);
						Kalman_Gyroscope.Add(Kalman.Item2);
					}
				}

				using (StreamWriter FileWriter = File.AppendText(INSKalmanFile))
				{
					FileWriter.WriteLine($"Timer,AX,AY,AZ,GX,GY,GZ,A");
					int DataCount = Kalman_Gyroscope.Count;
					for (int i = 0; i < DataCount; i++)
					{
						if (Kalman_Gyroscope[i] != null && Kalman_Accelerometer[i] != null)
						{
							FileWriter.WriteLine($"\"{Kalman_Gyroscope[i].TimeOfData}\"," +
							                     $"\"{String.Format("{0:F20}", Math.Round(Kalman_Accelerometer[i].X, 5))}\"," +
							                     $"\"{String.Format("{0:F20}", Math.Round(Kalman_Accelerometer[i].Y, 5))}\"," +
							                     $"\"{String.Format("{0:F20}", Math.Round(Kalman_Accelerometer[i].Z, 5))}\"," +
							                     $"\"{String.Format("{0:F20}", Math.Round(Kalman_Gyroscope[i].X, 5))}\"," +
							                     $"\"{String.Format("{0:F20}", Math.Round(Kalman_Gyroscope[i].Y, 5))}\"," +
							                     $"\"{String.Format("{0:F20}", Math.Round(Kalman_Gyroscope[i].Z, 5))}\"," +
												 $"\"{Angles[i]}\"");
						}
					}
					FileWriter.Close();
				}
			}
		}

		private void WriteNonCalibrated(List<double> Angles)
		{
			if (currentDataMapper.Calibrated)
			{
				List<XYZ> AccelerometerNonCalibrated = new List<XYZ>();
				List<XYZ> GyroScopeNonCalibrated = new List<XYZ>();

				foreach (var DataEntryElement in currentDataMapper.UnCalibrated)
				{
					AccelerometerNonCalibrated.Add(DataEntryElement.INS_Accelerometer);
					GyroScopeNonCalibrated.Add(DataEntryElement.INS_Gyroscope);
				}

				using (StreamWriter FileWriter = File.AppendText(NoCalibrateINSFile))
				{
					FileWriter.WriteLine($"Timer,AX,AY,AZ,GX,GY,GZ,A");
					int DataCount = GyroScopeNonCalibrated.Count;
					for (int i = 0; i < DataCount; i++)
					{
						if (GyroScopeNonCalibrated[i] != null && GyroScopeNonCalibrated[i] != null)
						{
							FileWriter.WriteLine($"\"{GyroScopeNonCalibrated[i].TimeOfData}\"," +
												 $"\"{GyroScopeNonCalibrated[i].X}\"," +
												 $"\"{GyroScopeNonCalibrated[i].Y}\"," +
												 $"\"{GyroScopeNonCalibrated[i].Z}\"," +
												 $"\"{GyroScopeNonCalibrated[i].X}\"," +
												 $"\"{GyroScopeNonCalibrated[i].Y}\"," +
												 $"\"{GyroScopeNonCalibrated[i].Z}\"," +
												 $"\"{Angles[i]}\"");
						}
					}
					FileWriter.Close();
				}
			}
		}

		private void WriteRollingAverage(List<double> Angles)
		{
			if (currentDataMapper.RollingAverageBool)
			{
				List<XYZ> RA_Accelerometer = new List<XYZ>();
				List<XYZ> RA_Gyroscope = new List<XYZ>();
				if (currentDataMapper.RollingAverageBool)
				{
					foreach (var RollingAverage in currentDataMapper.RollingAverageData)
					{
						RA_Accelerometer.Add(RollingAverage.Item1);
						RA_Gyroscope.Add(RollingAverage.Item2);
					}
				}

				using (StreamWriter FileWriter = File.AppendText(INSRollingAverageFile))
				{
					FileWriter.WriteLine($"Timer,AX,AY,AZ,GX,GY,GZ");
					int DataCount = RA_Gyroscope.Count;
					for (int i = 0; i < DataCount; i++)
					{
						if (RA_Gyroscope[i] != null && RA_Accelerometer[i] != null)
						{
							FileWriter.WriteLine($"\"{RA_Gyroscope[i].TimeOfData}\"," +
												 $"\"{RA_Accelerometer[i].X.ToString("G17")}\"," +
												 $"\"{RA_Accelerometer[i].Y.ToString("G17")}\"," +
												 $"\"{RA_Accelerometer[i].Z.ToString("G17")}\"," +
												 $"\"{RA_Gyroscope[i].X.ToString("G17")}\"," +
												 $"\"{RA_Gyroscope[i].Y.ToString("G17")}\"," +
												 $"\"{RA_Gyroscope[i].Z.ToString("G17")}\"," +
												 $"\"{Angles[i].ToString("G17")}\"");
						}
					}
					FileWriter.Close();
				}
			}
		}

		private void WriteAll()
		{
			List<double> Angles = PrepareNormal();

			WriteKalman(Angles);
			WriteNonCalibrated(Angles);
			WriteRollingAverage(Angles);
		}

		public void Execute() {
			WriteAll();
		}
	}
}
