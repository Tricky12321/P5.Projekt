using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
namespace Serial
{
	public class DataClass
	{
		private const int StackSize = 100;
		private const int CalibrateTimerMs = 10000; // ms of calibrating time
		private const int Margin = 5;
		private const bool UseMargin = true;
		private const int DecimalCount = 1;
		public double X;
		public double Y;
		public double Z;

		public double X_last = 0;
		public double Y_last = 0;
		public double Z_last = 0;

		public double X_calibration = 0;
		public double Y_calibration = 0;
		public double Z_calibration = 0;

		private Queue<double> X_Queue = new Queue<double>();
		private Queue<double> Y_Queue = new Queue<double>();
		private Queue<double> Z_Queue = new Queue<double>();

		private Queue<double> X_Log = new Queue<double>();
		private Queue<double> Y_Log = new Queue<double>();
		private Queue<double> Z_Log = new Queue<double>();

		private Queue<double> X_Calibrate = new Queue<double>();
		private Queue<double> Y_Calibrate = new Queue<double>();
		private Queue<double> Z_Calibrate = new Queue<double>();

		private KalmanFilter X_Kalman = new KalmanFilter(2, 2, 0.525, 2, 0.5, 0);
		private KalmanFilter Y_Kalman = new KalmanFilter(2, 2, 0.525, 2, 0.5, 0);
		private KalmanFilter Z_Kalman = new KalmanFilter(2, 2, 0.525, 2, 0.5, 0);

		private bool _calibrating = false;
		private bool _calibrated = false;
		private bool _useCalibration = true;
		private SerialPort _serialPort;

		// Used to calculate Hz Rate
		private double _hz_Rate = 0;
		private double _hz = 0;
		private object _hzLock = new object();

		private Stopwatch CalibrateTimer = new Stopwatch();
		public DataClass()
		{
			for (int i = 0; i < StackSize; i++)
			{
				X_Queue.Enqueue(0);
				Y_Queue.Enqueue(0);
				Z_Queue.Enqueue(0);
			}
			Thread HzThread = new Thread(CalculateHz);
			HzThread.Start();

		}
		#region Updating stuff
		public void UpdateXYZ(double XVal, double YVal, double ZVal)
		{
			UpdateX(XVal);
			UpdateY(YVal);
			UpdateZ(ZVal);
			NewNum();
		}

		public decimal FindDifference(decimal nr1, decimal nr2)
		{
			return Math.Abs(nr1 - nr2);
		}

		public void UpdateX(double XVal)
		{
			if (_calibrating)
			{
				X_Calibrate.Enqueue(XVal);
			}
			X = XVal;
			X_Queue.Enqueue(XVal);
			X_Log.Enqueue(XVal);
			X_Queue.Dequeue();
		}

		public void UpdateY(double YVal)
		{
			if (_calibrating)
			{
				Y_Calibrate.Enqueue(YVal);
			}
			Y = YVal;
			Y_Queue.Enqueue(YVal);
			Y_Log.Enqueue(YVal);
			Y_Queue.Dequeue();
		}

		public void UpdateZ(double ZVal)
		{
			if (_calibrating)
			{
				Z_Calibrate.Enqueue(ZVal);
			}
			Z = ZVal;
			Z_Queue.Enqueue(ZVal);
			Z_Log.Enqueue(ZVal);
			Z_Queue.Dequeue();
		}
		#endregion
		#region Average output
		public double GetAvgX()
		{
			return Math.Round(X_Queue.Average(), DecimalCount);

		}

		public double GetAvgY()
		{
			return Math.Round(Y_Queue.Average(), DecimalCount);

		}

		public double GetAvgZ()
		{
			return Math.Round(Z_Queue.Average(), DecimalCount);
		}
		#endregion
		#region HzCalculation
		public void NewNum()
		{
			lock (_hzLock)
			{
				_hz++;
			}
		}
		private void CalculateHz()
		{
			Thread.Sleep(1000);
			_hz_Rate = _hz;
			lock (_hzLock)
			{
				_hz = 0;
			}
			CalculateHz();
		}

		public double GetHz()
		{
			return _hz_Rate;
		}
		#endregion

		public void WriteToCSV()
		{
			if (File.Exists("output.csv"))
			{
				File.Delete("output.csv");
			}
			using (StreamWriter FileWriter = File.AppendText("output.csv"))
			{
				FileWriter.WriteLine("Timer,X,Y,Z");
				int i = 0;
				while (X_Log.Count > 0 && Y_Log.Count > 0 && Z_Log.Count > 0)
				{
					i++;
					double X_local = X_Log.Dequeue();
					double Y_local = Y_Log.Dequeue();
					double Z_local = Z_Log.Dequeue();
					FileWriter.WriteLine($"\"{i}\",\"{X_local}\",\"{Y_local}\",\"{Z_local}\"");
				}
				FileWriter.Close();
			}
		}


		#region PrintStuff

		public void PrintXYZ()
		{
			if (_useCalibration)
			{
				PrintXYZ_true(X_Calibrated(), Y_Calibrated(), Z_Calibrated());
			}
			else
			{
				PrintXYZ_true(X, Y, Z);
			}
		}

		public void PrintHz()
		{

			Console.WriteLine($"Hz: {GetHz()}");
		}

		public void PrintXYZ_true(double x, double y, double z)
		{
			string X = Math.Round(x, DecimalCount).ToString();
			string Y = Math.Round(y, DecimalCount).ToString();
			string Z = Math.Round(z, DecimalCount).ToString();
			Console.WriteLine($"X: {X.PadLeft(5)}");
			Console.WriteLine($"Y: {Y.PadLeft(5)}");
			Console.WriteLine($"Z: {Z.PadLeft(5)}");

		}
		#endregion



		public void Calibrate()
		{
			if (!_useCalibration)
			{
				Console.WriteLine("Skipping calibration");

				return;
			}
			_calibrating = true;
			Console.Clear();
			Console.WriteLine($"Calibrating... ({CalibrateTimerMs/1000} sec)");
			Console.WriteLine("LEAVE SENSOR LEVEL");

			// Start calibrate thread
			Thread CalibrateThread = new Thread(CalibrateInput);
			CalibrateThread.Start();
			CalibrateTimer.Start();
			CalibrateThread.Join();
			// var CalibrateThread = Task.Factory.StartNew(() => CalibrateInput());
			// CalibrateThread.Wait();

		}

		public void HandleRawData(string data)
		{
			try
			{
				if (data.Contains("AC") && data.Contains(":"))
				{
					data = data.Substring(2, data.Length - 3);

					var message_split = data.Split(':');
					double Xx = Convert.ToDouble(message_split[0]) / 100;
					double Yy = Convert.ToDouble(message_split[1]) / 100;
					double Zz = Convert.ToDouble(message_split[2]) / 100;

					UpdateXYZ(Xx, Yy, Zz);
				}

			}
			catch (TimeoutException) { }
			catch (FormatException)
			{

			}
		}

		public double X_KalmanVal()
		{
			if (_useCalibration)
			{
				return X_Kalman.Output(X_Calibrated());

			}
			return X_Kalman.Output(X);

		}

		public double Y_KalmanVal()
		{
			if (_useCalibration)
			{
				return Y_Kalman.Output(Y_Calibrated());

			}
			return Y_Kalman.Output(Y);

		}

		public double Z_KalmanVal()
		{
			if (_useCalibration)
			{
				return Z_Kalman.Output(Z_Calibrated());

			}
			return Z_Kalman.Output(Z);
		}

		public void PrintXYZKalman()
		{
			Console.WriteLine("Kalman");
			PrintXYZ_true(X_KalmanVal(), Y_KalmanVal(), Z_KalmanVal());
		}

		public double X_Calibrated()
		{
			return X - X_calibration;
		}

		public double Y_Calibrated()
		{
			return Y - Y_calibration;
		}

		public double Z_Calibrated()
		{
			return Z - Z_calibration;
		}


		public void SetInput(SerialPort port)
		{
			_serialPort = port;

		}

		public void CalibrateInput()
		{
			while (_calibrating)
			{
				if (CalibrateTimer.ElapsedMilliseconds >= CalibrateTimerMs)
				{
					_calibrating = false;
					_calibrated = true;
					X_calibration = X_Calibrate.Average();
					Y_calibration = Y_Calibrate.Average();
					Z_calibration = Z_Calibrate.Average();
					return;
				}
				try
				{
					var input = _serialPort.ReadLine();
					HandleRawData(input);
				}
				catch (TimeoutException) { }
			}
		}


	}
}
