using System;
using Serial.DataMapper;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Serial.DataMapper.Highpass;
using System.Linq;
namespace Serial.Menu
{
	public static class MainMenu
	{
		static DataMapper.DataMapper dataMapper;
		static int MapperTimer = 0;
		public static void ShowMenu()
		{
			bool Exit = false;
			do
			{
				Console.Write("Command > ");
				string[] Input = Console.ReadLine().ToLower().Split(' ');
				switch (Input[0])
				{
					case "help":
						PrintCommands();
						break;
					case "logdata":
					case "ld":
					case "log":
						LogData(Input);
						break;
					case "test":
						Test();
						break;
					default:
						Exit = MenuController.DefaultCommands(Input);
						break;
				}
			} while (!Exit);
		}

		public static void Test()
		{
			Load load = new Load("frem_5m_2_INS.csv");
			load.HandleCSV();

			string INSFile = "opdateretINS.csv";
			if (File.Exists(INSFile))
			{
				File.Delete(INSFile);
			}

			List<Tuple<double, double, double>> liste = new List<Tuple<double, double, double>>();
			List<Tuple<double, double, double>> listeNew = new List<Tuple<double, double, double>>();
			foreach (var item in load.data.AllDataEntries)
			{
				Tuple<double, double, double> tup = new Tuple<double, double, double>(item.INS_Accelerometer.TimeOfData, item.INS_Accelerometer.X, item.INS_Accelerometer.Y);
				if (!(liste.Any(val => val.Item1 == tup.Item1)))
				{
					liste.Add(tup);
				}
			}
			HighpassFilter HPX = new HighpassFilter(25, 100, HighpassFilter.PassType.Highpass, (float)Math.Sqrt(2));
			HighpassFilter HPY = new HighpassFilter(45, 100, HighpassFilter.PassType.Highpass, (float)Math.Sqrt(2));
			liste.Sort((x, y) => x.Item1.CompareTo(y.Item1));

			foreach (var tup in liste)
			{
				HPX.Update((float)tup.Item2);
				HPY.Update((float)tup.Item3);
				listeNew.Add(new Tuple<double, double, double>(tup.Item1, HPX.Value, HPY.Value));
			}
		}

		public static void PrintCommands()
		{
			MenuController.DefaultHelp();
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("nn - Neural network");
			Console.WriteLine(" - start - Starts the Neural Network");
			Console.WriteLine(" - stop - Stops the Neural Network");
			Console.WriteLine(" - save <Path> - Saves the Neural Network to a file");
			Console.WriteLine(" - load <Path> - Loads the Neural Network from a file");
			Console.WriteLine(" - new - Creates a new CC");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("logdata - logging of test data");
			Console.WriteLine(" - start [Time in sed]- Start the logger");
			Console.WriteLine(" - stop - Stops the logger");
			Console.WriteLine(" - save <Path> - Saves the data to files");
			Console.WriteLine(" - new [pozyx/ins/[BLANK]]- Creates a DataMapper for logging, Blank uses both");
			Console.WriteLine(" - kalman - Generates Kalman values for INS");
			Console.WriteLine(" - ra - Generates Rolling Average for INS");
			Console.WriteLine(" - segment - Segments data from buffer and dumps to segmented.csv");
			Console.WriteLine(" - calibrate - Calibrates INS");
			Console.WriteLine(" - load - load a CSV file into a datamapper");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("test - run test code");
			Console.WriteLine("-----------------------------------");
		}


		public static void LogData(string[] Input)
		{
			List<string> InputList = new List<string>(Input);
			if (!(Input.Length > 1))
			{
				Console.WriteLine("Invalid input, use help to get data");
				return;
			}
			switch (Input[1])
			{
				case "weka":
					if (MenuController.Confirm("Are you sure you want to replace everything?", false))
					{
						string[] entries = Directory.GetFileSystemEntries(".", "*.csv", SearchOption.AllDirectories);
						foreach (var FilePath in entries)
						{
							string FileContents = File.ReadAllText(FilePath);
							string Output = Regex.Replace(FileContents, @"\d+,\d+", delegate (Match match)
							{
								string v = match.ToString().Replace(",", ".");
								return v;
							});
							File.WriteAllText(FilePath, Output);
						}
					}
					break;
				case "combine":
					if (true)
					{
						StringBuilder FileContents = new StringBuilder();
						string[] entries = Directory.GetFileSystemEntries(".", "*INS_KALMAN.csv", SearchOption.AllDirectories);
						foreach (var FilePath in entries)
						{
							FileContents.Append(File.ReadAllText(FilePath));
						}
						FileContents.Replace("Timer,AX,AY,AZ,GX,GY,GZ\n", "");
						File.WriteAllText("Combined.csv", "Timer,AX,AY,AZ,GX,GY,GZ\n" + FileContents.ToString());
					}
					break;
				case "ra":
					if (Input.Length == 3)
					{
						try
						{

							dataMapper.CalculateRollingAverage(Convert.ToInt32(Input[2]));
						}
						catch (FormatException)
						{
							Console.WriteLine("Invalid format!");
						}
					}
					else
					{
						dataMapper.CalculateRollingAverage(10);
					}
					break;
				case "load":
					if (Input.Length == 3)
					{
						Load load = new Load(Input[2] + ".csv");
						load.HandleCSV();
						dataMapper = load.data;
					}
					break;

				case "calibrate":
					if (dataMapper == null)
					{
						Console.WriteLine("No Data Mapper has been created!");
					}
					else
					{
						dataMapper.CalibrateINS();
					}
					break;

				case "start":
					if (dataMapper == null)
					{
						Console.WriteLine("No Data Mapper has been created!");
					}
					else
					{
						if (InputList.Count == 3)
						{
							if (Input[2] == "nem")
							{
								dataMapper.StartReading();
								if (MenuController.Confirm("Stop?", true))
								{
									dataMapper.StopReading();
								}
							}
							else
							{
								try
								{
									MapperTimer = Convert.ToInt32(InputList[2]);
									Thread TimerThread = new Thread(dataMapperTimer);
									dataMapper.StartReading();
									TimerThread.Start();
									Console.WriteLine($"Started Datamapper for {MapperTimer} sec");
									TimerThread.Join();

								}
								catch (Exception)
								{
									Console.WriteLine("Invalid format!");
								}
							}
						}
						else
						{
							dataMapper.StartReading();
							Console.WriteLine("Started Datamapper");
						}
					}
					break;
				case "stop":
					if (dataMapper == null)
					{
						Console.WriteLine("No Data Mapper has been created!");
					}
					else
					{
						dataMapper.StopReading();
						Console.WriteLine("Stopped data logging");
					}
					break;
				case "clear":
					if (dataMapper == null)
					{
						Console.WriteLine("No Data Mapper has been created!");
					}
					else
					{
						dataMapper.ClearEntries();
						Console.WriteLine("Cleared the DataList");
					}
					break;
				case "save":
					if (dataMapper == null)
					{
						Console.WriteLine("No Data Mapper has been created!");
					}
					else
					{
						if (InputList.Count == 3)
						{
							string FileName = InputList[2];
							if (File.Exists(FileName + "_INS.csv") || File.Exists(FileName + "_POZYX.csv"))
							{
								if (MenuController.Confirm("This file already exists, Overwrite?", false))
								{
									WriteToCSV(FileName);
									Console.WriteLine($"Saved to {FileName}");
								}
								else
								{
									Console.WriteLine("Cancelled!");
								}
							}
							else
							{
								WriteToCSV(FileName);
								Console.WriteLine($"Saved to {FileName}");
							}
						}
					}
					break;
				case "new":
					if (Input.Length == 2)
					{
						dataMapper = new DataMapper.DataMapper();
					}
					else
					{
						switch (Input[2])
						{
							case "ins":
								dataMapper = new DataMapper.DataMapper(false, true);
								break;
							case "pozyx":
								dataMapper = new DataMapper.DataMapper(true, false);
								break;
							default:
								break;
						}
					}

					Console.WriteLine("Created new DataMapper!");
					break;
				case "kalman":
					dataMapper.GenerateKalman();
					Console.WriteLine("Generated data kalman filtered data of INS data.");
					break;
				case "segment":
					string SegmentFile = "Segmented.csv";
					if (File.Exists(SegmentFile))
					{
						File.Delete(SegmentFile);
					}
					ConcurrentQueue<DataMapper.DataEntry> OutputSegments = dataMapper.SegmentData();
					using (var test = File.AppendText(SegmentFile))
					{
						test.WriteLine("Timer,AX,AY,AZ,GX,GY,GZ,Angle");
						int i = 0;
						foreach (var item in OutputSegments)
						{
							Console.WriteLine($"{i++}");
							string output = $"\"{item.INS_Accelerometer.TimeOfData}\",\"{item.INS_Accelerometer.X}\",\"{item.INS_Accelerometer.Y}\",\"{item.INS_Accelerometer.Z}\",\"{item.INS_Gyroscope.X}\",\"{item.INS_Gyroscope.Y}\",\"{item.INS_Gyroscope.Z}\",\"{item.INS_Angle}\"";
							Console.WriteLine($"{output}");
							test.WriteLine(output);
						}
					}
					Console.WriteLine($"Done segmenting data! {OutputSegments.Count}");
					break;
				default:
					Console.WriteLine("Invalid input format, use help command!");
					break;
			}
		}

		public static void WriteToCSV(string Name)
		{
			string INSFile = Name + "_INS.csv";
			string POZYXFile = Name + "_POZYX.csv";
			string INSKalmanFile = Name + "_INS_KALMAN.csv";
			string INSRollingAverageFile = Name + "_INS_RA.csv";
			List<DataMapper.DataEntry> DataList = new List<DataMapper.DataEntry>(dataMapper.AllDataEntries.ToArray());
			List<XYZ> Accelerometer = new List<XYZ>();
			List<XYZ> GyroScope = new List<XYZ>();
			List<XYZ> Pozyx = new List<XYZ>();
			List<double> Angles = new List<double>();
			List<XYZ> Kalman_Accelerometer = new List<XYZ>();
			List<XYZ> Kalman_Gyroscope = new List<XYZ>();
			List<XYZ> RA_Accelerometer = new List<XYZ>();
			List<XYZ> RA_Gyroscope = new List<XYZ>();
			foreach (var DataEntryElement in DataList)
			{
				Accelerometer.Add(DataEntryElement.INS_Accelerometer);
				GyroScope.Add(DataEntryElement.INS_Gyroscope);
				Pozyx.Add(DataEntryElement.PoZYX);
				Angles.Add(DataEntryElement.INS_Angle);
			}

			if (dataMapper.Kalman)
			{
				foreach (var Kalman in dataMapper.KalmanData)
				{
					Kalman_Accelerometer.Add(Kalman.Item1);
					Kalman_Gyroscope.Add(Kalman.Item2);
				}
			}

			if (dataMapper.RollingAverageBool)
			{
				foreach (var RollingAverage in dataMapper.RollingAverageData)
				{
					RA_Accelerometer.Add(RollingAverage.Item1);
					RA_Gyroscope.Add(RollingAverage.Item2);
				}
			}

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

			// WRITE INS
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
			// Write INS KALMAN
			if (dataMapper.Kalman)
			{
				using (StreamWriter FileWriter = File.AppendText(INSKalmanFile))
				{
					FileWriter.WriteLine($"Timer,AX,AY,AZ,GX,GY,GZ,A");
					int DataCount = Kalman_Gyroscope.Count;
					for (int i = 0; i < DataCount; i++)
					{
						if (Kalman_Gyroscope[i] != null && Kalman_Accelerometer[i] != null)
						{
							FileWriter.WriteLine($"\"{Kalman_Gyroscope[i].TimeOfData}\"," +
												 $"\"{Kalman_Accelerometer[i].X}\"," +
												 $"\"{Kalman_Accelerometer[i].Y}\"," +
												 $"\"{Kalman_Accelerometer[i].Z}\"," +
												 $"\"{Kalman_Gyroscope[i].X}\"," +
												 $"\"{Kalman_Gyroscope[i].Y}\"," +
												 $"\"{Kalman_Gyroscope[i].Z}\"," +
												 $"\"{Angles[i]}\"");
						}
					}
					FileWriter.Close();
				}
			}
			// Write RollingAverage
			if (dataMapper.RollingAverageBool)
			{
				using (StreamWriter FileWriter = File.AppendText(INSRollingAverageFile))
				{
					FileWriter.WriteLine($"Timer,AX,AY,AZ,GX,GY,GZ");
					int DataCount = RA_Gyroscope.Count;
					for (int i = 0; i < DataCount; i++)
					{
						if (RA_Gyroscope[i] != null && RA_Accelerometer[i] != null)
						{
							FileWriter.WriteLine($"\"{RA_Gyroscope[i].TimeOfData}\"," +
												 $"\"{RA_Accelerometer[i].X}\"," +
												 $"\"{RA_Accelerometer[i].Y}\"," +
												 $"\"{RA_Accelerometer[i].Z}\"," +
												 $"\"{RA_Gyroscope[i].X}\"," +
												 $"\"{RA_Gyroscope[i].Y}\"," +
												 $"\"{RA_Gyroscope[i].Z}\"," +
												 $"\"{Angles[i]}\"");
						}
					}
					FileWriter.Close();
				}
			}
			// WRITE POZYX
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

		public static void dataMapperTimer()
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			while (timer.ElapsedMilliseconds < (MapperTimer * 1000))
			{
				Thread.Sleep(1);
			}
			dataMapper.StopReading();
			Console.WriteLine("Done reading (TIMER)");
			timer.Stop();
		}

	}
}
