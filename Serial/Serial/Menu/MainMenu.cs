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
using Serial.Utility;
using Serial.CSVWriter;
using Serial.DynamicCalibrationName;
using Serial.DynamicCalibrationName.Points;
using Serial.Clustering;
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
                    case "cluster":
                        RunClustering();
                        break;
					case "dc":
						RunDynamicCalibration();
						break;
					default:
						Exit = MenuController.DefaultCommands(Input);
						break;
				}
			} while (!Exit);
		}

		private static void RunDynamicCalibration()
		{
			string filePath = Directory.GetCurrentDirectory() + "/Test";
			string[] fileNamesArray = Directory.GetFiles(filePath).Where(x => x.EndsWith(".csv")).ToArray();

			for (int i = 0; i < fileNamesArray.Count(); i++)
			{
				Console.WriteLine($"{fileNamesArray[i]} : Number {i}");
			}

			int number = 0;

			while (!(int.TryParse(Console.ReadLine(), out number) && number < fileNamesArray.Count() && number >= 0))
			{
				Console.WriteLine("Wrong input!");
			}

			string fileName = fileNamesArray[number];

			Load csvController = new Load(fileName);
			csvController.HandleCSV();
			//var test = csvController.AccDataList[20];
			var tesadsasdas = csvController.data.GetAccelerationXYZFromCSV();


			//TODO: HERE!
			ClusteringDynamicCalibration Clustering = new ClusteringDynamicCalibration(fileName);
			DynamicCalibration dyn = new DynamicCalibration(tesadsasdas, Clustering);
			//dyn.CalibrateResidualSumOfSquares(2.0);
			//dyn.CalibrateAccelerationPointCoefficient();

			List<TimePoint> accelerationList = dyn.AccelerationList;
			List<TimePoint> velocityList = dyn.CalculateDynamicVelocityList(dyn.NaiveVelocityList);
			List<TimePoint> distanceList = dyn.CalculatePosition(velocityList);

			while (Console.ReadLine() != "q")
			{
				Console.WriteLine("What do you want to print? (acc, vel, dis or toCSV)");

				string input = Console.ReadLine();
				switch (input)
				{
					case "acc":
						accelerationList.ForEach(x => Console.WriteLine($"\"{x.Time.ToString().Replace(',', '.')}\",\"{x.Value.ToString().Replace(',', '.')}\""));
						break;
					case "vel":
						velocityList.ForEach(x => Console.WriteLine($"\"{x.Time.ToString().Replace(',', '.')}\",\"{x.Value.ToString().Replace(',', '.')}\""));
						break;
					case "dis":
						distanceList.ForEach(x => Console.WriteLine($"\"{x.Time.ToString().Replace(',', '.')}\",\"{x.Value.ToString().Replace(',', '.')}\""));
						break;
					case "toCSV":
						string FileName = fileName.Replace(".csv", string.Empty).Split('/').Last();
						CSVWriterController csvWriter = new CSVWriterController(FileName + "_acc");
						csvWriter.DynamicToCSV(accelerationList);
						csvWriter.FileName = FileName + "_vel";
						csvWriter.DynamicToCSV(velocityList);
						csvWriter.FileName = FileName + "_dic";
						csvWriter.DynamicToCSV(distanceList);
						Console.WriteLine("Done writing to files!");
						break;
					default:
						Console.WriteLine("acc, vel, dis or toCSV?");
						break;
				}
			}
		}

        private static void RunClustering()
        {
            Console.WriteLine("Enter file name:");
            string filePath = Console.ReadLine() + ".csv";
            if (File.Exists(filePath))
            {
                Clustering.Clustering clustering = new Clustering.Clustering(filePath);
                foreach (var data in clustering.GetClusters())
                {
                    Console.WriteLine($"Point: {data.PointNumber}, in cluster {data.Cluster}");
                }
            }
            else
            {
                Console.WriteLine("File does not exist!");
            }
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
				case "fixtimer":
					if (MenuController.Confirm("Are you sure you want to replace everything?", false))
					{
						if (Input.Length == 3)
						{
							int Seperation = Convert.ToInt32(Input[2]);
							FixTime(Seperation);
						}
						else
						{
							FixTime();
						}
					}
					break;

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

				case "latex":
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

							Output = Regex.Replace(Output, @"""\d+""\,", delegate (Match match)
							{
								string substringed = match.ToString().Substring(1, match.Length - 3);
								double newNum = Convert.ToDouble(substringed) / 1000f;
								return newNum.ToString().Replace(",", ".") + ",";
							});

							Output = Regex.Replace(Output, @"""(|-)\d+(|\.|\,)\d*""", delegate (Match match)
							{
								string v = match.ToString().Replace(@"""", "");
								return v;
							});

							Output = Regex.Replace(Output, @"""(|-)\d+(\.|\,)\d+""", delegate (Match match)
							{
								string v = match.ToString().Replace(@"""", "");
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
						if (Input.Length == 3)
						{
							int Timer = 0;
							try
							{
								Timer = Convert.ToInt32(Input[2]) * 1000;
								dataMapper.CalibrateINS(Timer);
							}
							catch (FormatException ex)
							{
								Console.WriteLine("Not a number givin!");
							}
						}
						else
						{
							dataMapper.CalibrateINS();
						}
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
									CSVWriterController csvWriter = new CSVWriterController(dataMapper, FileName);
									csvWriter.Execute();
									Console.WriteLine($"Saved to {FileName}");
								}
								else
								{
									Console.WriteLine("Cancelled!");
								}
							}
							else
							{
								CSVWriterController csvWriter = new CSVWriterController(dataMapper, FileName);
								csvWriter.Execute();
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
					ConcurrentQueue<DataEntry> OutputSegments = dataMapper.SegmentData();
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
                case "variance":
                    ConcurrentQueue<Tuple<DataEntry, double, double>> varianceData = dataMapper.CalculateVariance();
                    string VarianceFile = "Variance.csv";

                    if (File.Exists(VarianceFile))
                    {
                        File.Delete(VarianceFile);
                    }

                    using (var test = File.AppendText(VarianceFile))
                    {
                        test.WriteLine("Timer,AX,AY,AZ,GX,GY,GZ,Angle,Variance,Slope");
                        foreach (var item in varianceData)
                        {
                            string output = $"\"{item.Item1.INS_Accelerometer.TimeOfData}\",\"{item.Item1.INS_Accelerometer.X}\",\"{item.Item1.INS_Accelerometer.Y}\",\"{item.Item1.INS_Accelerometer.Z}\",\"{item.Item1.INS_Gyroscope.X}\",\"{item.Item1.INS_Gyroscope.Y}\",\"{item.Item1.INS_Gyroscope.Z}\",\"{item.Item1.INS_Angle}\",\"{item.Item2}\",\"{item.Item3}\"";
                            test.WriteLine(output);
                        }
                    }
                    Console.WriteLine("Done!");
                    break;
                case "acc":
                    ConcurrentQueue<Tuple<DataEntry, double, double, double, double>> accData = dataMapper.CalculateAcceleration();

                    string appendedFile = "Timer_AX_AY_AZ_GX_GY_GZ_Angle_Velocity_Slope_Variance_SlopeDiff.csv";
                    string SlopeDivVariance_Slope = "SlopeDivVariance_Slope.csv";

                    if (File.Exists(appendedFile))
                    {
                        File.Delete(appendedFile);
                    }
                    using (var test = File.AppendText(appendedFile))
                    {
                        test.WriteLine("Timer,AX,AY,AZ,GX,GY,GZ,Angle,Velocity,Slope,Variance,SlopeDiff");
                        foreach (var item in accData)
                        {
                            string outPut = $"\"{item.Item1.INS_Accelerometer.TimeOfData}\",\"{item.Item1.INS_Accelerometer.X}\",\"{item.Item1.INS_Accelerometer.Y}\",\"{item.Item1.INS_Accelerometer.Z}\",\"{item.Item1.INS_Gyroscope.X}\",\"{item.Item1.INS_Gyroscope.Y}\",\"{item.Item1.INS_Gyroscope.Z}\",\"{item.Item1.INS_Angle}\",\"{item.Item2}\",\"{item.Item3}\",\"{item.Item4}\",\"{item.Item5}\"";
                            test.WriteLine(outPut);
                        }
                    }

                    if (File.Exists(SlopeDivVariance_Slope))
                    {
                        File.Delete(SlopeDivVariance_Slope);
                    }
                    using (var test = File.AppendText(SlopeDivVariance_Slope))
                    {
                        test.WriteLine("Slope/Variance,SlopeDiff");
                        foreach (var item in accData)
                        {
                            string outPut = $"\"{item.Item3/item.Item4}\",\"{item.Item3}\"";
                            test.WriteLine(outPut);
                        }
                    }

                    Console.WriteLine("Done!");
                    break;
                default:
					Console.WriteLine("Invalid input format, use help command!");
					break;
			}
		}

		public static void FixTime(int TimeInterval = 1)
		{
			string[] entries = Directory.GetFileSystemEntries(".", "*_INS*.csv", SearchOption.AllDirectories);
			foreach (var FilePath in entries)
			{
				try
				{
					double TimerIntervalMS = TimeInterval * 100;
					Load load = new Load(FilePath);
					if (load.GetCSVType() == CSVTypes.INS)
					{
						load.HandleCSV();
						DataMapper.DataMapper SingleDataMapper = load.data;
						int iteration = 1;
						List<DataEntry> datas = new List<DataEntry>(SingleDataMapper.AllDataEntries);
						double test = datas.Max(X => X.INS_Accelerometer.TimeOfData);
						double CurrentTimerVal = TimerIntervalMS * iteration;
						while (test > CurrentTimerVal)
						{
							double min = TimerIntervalMS * (iteration - 1);
							double max = TimerIntervalMS * iteration;
							var dataEntries = datas.Where(X =>
														  X.INS_Accelerometer.TimeOfData < max &&
														  X.INS_Accelerometer.TimeOfData > min).ToList();
							int got = dataEntries.Count();
							double SectionTimer = ((dataEntries.Max(X => X.INS_Accelerometer.TimeOfData) - min) / got);
							double entryTimer = SectionTimer + min;
							foreach (var entry in dataEntries)
							{
								entry.INS_Accelerometer.TimeOfData = Math.Round(entryTimer, 5);
								entry.INS_Gyroscope.TimeOfData = Math.Round(entryTimer, 5);
								entryTimer += SectionTimer;
								if (entryTimer > max)
								{

								}
							}
							CurrentTimerVal = TimerIntervalMS * ++iteration;
						}
						CSVWriterController INSWriter = new CSVWriterController(FilePath, SingleDataMapper, load.GetCSVType());
						string POZYX_FilePath = FilePath.Replace("_INS.csv", "_POZYX.csv");



						if (File.Exists(POZYX_FilePath))
						{
							Console.WriteLine($"FOUND POZYX FILE, {POZYX_FilePath}");
							load = new Load(POZYX_FilePath);
							load.HandleCSV();
							var PozyxDatamapper = load.data;
							var PozyxData = PozyxDatamapper.AllDataEntries.ToList();
							int count = PozyxDatamapper.AllDataEntries.Count();
							for (int i = 0; i < count; i++)
							{
								PozyxData[i].PoZYX.TimeOfData = datas[i].INS_Gyroscope.TimeOfData;
							}
							CSVWriterController PozyxWriter = new CSVWriterController(POZYX_FilePath, PozyxDatamapper, load.GetCSVType());
						}

					}
				}
				catch (Exception)
				{

				}
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

        private static void RunClustering()
        {
            Console.WriteLine("Enter file name:");
            string filePath = Console.ReadLine() + ".csv";
            if (File.Exists(filePath))
            {
                Clustering.Clustering clustering = new Clustering.Clustering(filePath);
                foreach (var data in clustering.GetClusters())
                {
                    Console.WriteLine($"Point: {data.PointNumber}, in cluster {data.Cluster}");
                }
            }
            else
            {
                Console.WriteLine("File does not exist!");
            }