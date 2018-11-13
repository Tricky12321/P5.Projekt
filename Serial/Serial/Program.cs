﻿using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NeuralNetwork;
using System.IO;
using NeuralNetwork1;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Concurrent;
namespace Serial
{


	class MainClass
	{
		static INS_POSZYX_NeuralNetworkTester nn;
		static DataMapper.DataMapper dataMapper;

		static int MapperTimer = 0;
		public static void Main()
		{
			/*var test = new NeuralNetwork1.NeuralNetwork(0.2, new int[]{2, 3, 3, 3, 1});

            Random r = new Random(Environment.TickCount);
            for (int i = 0; i < 1000000; i++)
            {
                test.Train(new List<double>() { 1, 1 }, new List<double>() { 1 });
            }*/
			ShowMenu();

			/*double[] tesfds = test.Run(new List<double>() { 1, 1 });
            Console.WriteLine("teststset");
            tesfds.ToList().ForEach(Console.WriteLine)*/
		}

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
					case "nn":
						NeuralNetwork(Input);
						break;
					case "exit":
						Exit = true;
						Environment.Exit(0);
						break;
					case "clear":
						Console.Clear(); ;
						break;
					case "devices":
						PrintDevices();
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
						Console.WriteLine("Unknown command!");
						break;
				}
			} while (!Exit);
		}

		public static void Test()
		{
			List<double> TestData = new List<double>() { 0, 0, 0, 0, 5, 7, 4, 0, -3, -6, -8, -5, -4, -1, 0, 2, 3, 4, 4, 7, 6, 4, 0, 0, -3, 0, 0, 0, 0, -1, -1, 0, 2 };
			List<double> test = KalmanFilter.RunFilter(TestData);
			for (int i = 0; i < TestData.Count; i++)
			{
				Console.WriteLine($"Raw: {TestData[i]} - Filter: {test[i]}");
			}
		}

		public static void PrintCommands()
		{
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
			Console.WriteLine(" - calibrate - Calibrates INS");
			Console.WriteLine(" - load - load a CSV file into a datamapper");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("devices - Prints arduino devices");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("help - shows this page");
			Console.WriteLine("clear - clears the console");
			Console.WriteLine("test - run test code");
			Console.WriteLine("-----------------------------------");
		}

		public static void PrintDevices()
		{
			foreach (var Port in SerialReader.GetOpenPorts())
			{
				Console.WriteLine($"Port: {Port}");
			}
		}

		public static void NeuralNetwork(string[] Input)
		{
			List<string> InputList = new List<string>(Input);

			if (InputList.Count == 1)
			{
				Console.WriteLine("Invalid input format, use help command!");
				return;
			}

			switch (Input[1])
			{
				case "start":
					if (nn == null)
					{
						nn = new INS_POSZYX_NeuralNetworkTester();
					}
					nn.Start();
					break;
				case "stop":
					if (nn == null)
					{
						Console.WriteLine("There is no Neural Network to stop");
						break;
					}
					nn.Stop();
					break;
				case "save":
					if (nn == null)
					{
						Console.WriteLine("There is no Neural Network to start");
						break;
					}
					if (InputList.Count == 3)
					{
						if (File.Exists(Input[2] + ".nn"))
						{
							if (Confirm("This file already exists, Overwrite?", false))
							{
								nn.nn.Save(Input[2] + ".nn");
								Console.WriteLine($"Neural Network is saved as {Input[2]}");
							}
						}
						else
						{
							nn.nn.Save(Input[2] + ".nn");
							Console.WriteLine($"Neural Network is saved as {Input[2]}");
						}
					}
					else
					{
						Console.WriteLine("Invalid input format, use help command!");
					}
					break;
				case "load":
					if (InputList.Count == 3)
					{
						if (File.Exists(Input[2] + ".nn"))
						{
							nn = new INS_POSZYX_NeuralNetworkTester();
							nn.nn.Load(Input[2] + ".nn");
							Console.WriteLine($"Loaded Neural Network {Input[2]}");
						}
						else
						{
							Console.WriteLine("Neural Network does not exist!");
						}
					}
					else
					{
						Console.WriteLine("Invalid input format, use help command!");
					}
					break;
				case "new":
					nn = new INS_POSZYX_NeuralNetworkTester();
					Console.WriteLine("Cleared current Neural Network, and created a new one");
					break;
				default:
					Console.WriteLine("Invalid input format, use help command!");
					return;
			}
		}

		public static bool Confirm(string Message, bool? Default = null)
		{
			string YN = "[y/n]";
			if (Default == true)
			{
				YN = "[Y/n]";
			}
			else if (Default == false)
			{
				YN = "[y/N]";
			}

			Console.Write($"{Message} {YN}:");
			bool ValidInput = false;
			do
			{
				string Output = Console.ReadLine();
				switch (Output.ToLower())
				{
					case "y":
						ValidInput = true;
						return true;
					case "n":
						ValidInput = true;

						return false;
					case "":
						if (Default != null)
						{
							ValidInput = true;
							return Default.GetValueOrDefault();
						}
						break;
				}
			} while (!ValidInput);
			throw new Exception("Something went wrong in Confirm"); // It should never get here
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
					if (Confirm("Are you sure you want to replace everything?", false))
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
							if (Input[2] == "nem") {
                                dataMapper.StartReading();
								if (Confirm("Stop?", true)) {
									dataMapper.StopReading();
								}
							} else {
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
								if (Confirm("This file already exists, Overwrite?", false))
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
					var test = File.AppendText(SegmentFile);

					test.WriteLine("Timer,AX,AY,AZ,GX,GY,GZ,Angle");
                    foreach (var item in OutputSegments)
                    {
						test.WriteLine($"\"{item.INS_Accelerometer.TimeOfData}\",\"{item.INS_Accelerometer.X}\",\"{item.INS_Accelerometer.Y}\",\"{item.INS_Accelerometer.Z}\",\"{item.INS_Gyroscope.X}\",\"{item.INS_Gyroscope.Y}\",\"{item.INS_Gyroscope.Z}\",\"{item.INS_Angle}\",");
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
