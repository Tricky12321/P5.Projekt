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
		private const int CalibrateTimerMs = 1000; // ms of calibrating time
		private const int Margin = 5;
		private const bool UseMargin = true;
		private const int DecimalCount = 1;

		private const int StillStandRequirements = 150;

		public double X;
		public double Y;
		public double Z;

		public double X_Speed => KalmanLowPass_log.Sum(item => item.X);
		public double Y_Speed => KalmanLowPass_log.Sum(item => item.Y);
		public double Z_Speed => KalmanLowPass_log.Sum(item => item.Z);

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

		private Queue<XYZ> Raw_Log = new Queue<XYZ>();
		private Queue<XYZ> Kalman_Log = new Queue<XYZ>();
		private Queue<XYZ> Highpass_log = new Queue<XYZ>();
		private Queue<XYZ> Lowpass_log = new Queue<XYZ>();
		private Queue<XYZ> KalmanLowPass_log = new Queue<XYZ>();
		private Queue<XYZ> RawCalibrated_log = new Queue<XYZ>();

		private Queue<double> X_Calibrate = new Queue<double>();
		private Queue<double> Y_Calibrate = new Queue<double>();
		private Queue<double> Z_Calibrate = new Queue<double>();

		private KalmanFilter X_Kalman = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
		private KalmanFilter Y_Kalman = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
		private KalmanFilter Z_Kalman = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);

		private FilterButterworth X_HighPass = new FilterButterworth(0.5, 5, FilterButterworth.PassType.Highpass, 0.25);
		private FilterButterworth Y_HighPass = new FilterButterworth(0.5, 5, FilterButterworth.PassType.Highpass, 0.25);
		private FilterButterworth Z_HighPass = new FilterButterworth(0.5, 5, FilterButterworth.PassType.Highpass, 0.25);

		private FilterButterworth X_LowPass = new FilterButterworth(0.5, 120, FilterButterworth.PassType.Lowpass, 0.25);
		private FilterButterworth Y_LowPass = new FilterButterworth(0.5, 120, FilterButterworth.PassType.Lowpass, 0.25);
		private FilterButterworth Z_LowPass = new FilterButterworth(0.5, 120, FilterButterworth.PassType.Lowpass, 0.25);

		private KalmanFilter X_KalmanLowpass = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
		private KalmanFilter Y_KalmanLowpass = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);
		private KalmanFilter Z_KalmanLowpass = new KalmanFilter(1, 75, 1.425, 0, 0.2, 0);

		private FilterButterworth X_Kalman_LowPass = new FilterButterworth(0.8, 3, FilterButterworth.PassType.Lowpass, 0.25);
		private FilterButterworth Y_Kalman_LowPass = new FilterButterworth(0.8, 3, FilterButterworth.PassType.Lowpass, 0.25);
		private FilterButterworth Z_Kalman_LowPass = new FilterButterworth(0.8, 3, FilterButterworth.PassType.Lowpass, 0.25);


		private bool _calibrating = false;
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
		}
		#region Updating stuff
		public void UpdateXYZ(double XVal, double YVal, double ZVal)
		{
			UpdateX(XVal);
			UpdateY(YVal);
			UpdateZ(ZVal);

			X_Kalman_LowPass.Update(X_Calibrated());
			Y_Kalman_LowPass.Update(Y_Calibrated());
			Z_Kalman_LowPass.Update(Z_Calibrated());

			X_HighPass.Update(X_Calibrated());
			Y_HighPass.Update(Y_Calibrated());
			Z_HighPass.Update(Z_Calibrated());

			X_LowPass.Update(X_Calibrated());
			Y_LowPass.Update(Y_Calibrated());
			Z_LowPass.Update(Z_Calibrated());

			if (_useCalibration)
			{
				CheckStandStill();
			}

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

		public void WriteToCSV(string fileName, Queue<XYZ> Data, bool NewFiles = false)
		{
			string NewFileName = fileName+".csv";
			if (NewFiles) {
				int fileInt = 0;
                while (File.Exists(fileName + fileInt+".csv"))
                {
                    fileInt++;
                }
				NewFileName = fileName + fileInt+".csv";
			} else {
				if (File.Exists(NewFileName)) {
					File.Delete(NewFileName);
				}
			}

			using (StreamWriter FileWriter = File.AppendText(NewFileName))
			{
				FileWriter.WriteLine($"Timer,X,Y,Z,Timer,XI1,YI1,ZI1,Timer,XI2,YI2,ZI2");
				int i = 0;
				double X_I1 = 0;
				double Y_I1 = 0;
				double Z_I1 = 0;
				double X_I2 = 0;
				double Y_I2 = 0;
				double Z_I2 = 0;
				while (Data.Count > 0)
				{
					i++;
					XYZ SingleElement = Data.Dequeue();
					double X_local = SingleElement.X;
					double Y_local = SingleElement.Y;
					double Z_local = SingleElement.Z;
                    // Single Integral
					X_I1 += X_local;
					Y_I1 += Y_local;
					Z_I1 += Z_local;
                    // Double Integral
					X_I2 += X_I1;
					Y_I2 += Y_I1;
					Z_I2 += Z_I1;
					FileWriter.WriteLine($"\"{i}\",\"{X_local}\",\"{Y_local}\",\"{Z_local}\"" +
					                     $",\"{i}\",\"{X_I1}\",\"{Y_I1}\",\"{Z_I1}\"" +
					                     $",\"{i}\",\"{X_I2}\",\"{Y_I2}\",\"{Z_I2}\"");
				}
				FileWriter.Close();
			}
		}

		#region PrintStuff

		public void PrintXYZKalman()
		{
			Console.WriteLine("Kalman");
			PrintXYZ_true(X_KalmanVal(), Y_KalmanVal(), Z_KalmanVal());
		}

		public void PrintXYZSpeed()
        {
            Console.WriteLine("Speed m/s");
            PrintXYZ_true(X_Speed, Y_Speed, Z_Speed);
        }

		public void PrintXYZHighpass()
		{
			Console.WriteLine("Highpass");
			PrintXYZ_true(X_HighPassVal(), Y_HighPassVal(), Z_HighPassVal());
		}

		public void PrintXYZLowpass()
		{
			Console.WriteLine("Lowpass");
			PrintXYZ_true(X_LowPass.Value, Y_LowPass.Value, Z_LowPass.Value);
		}

		public void PrintXYZKalmanLowpass()
		{
			Console.WriteLine("Kalman Lowpass");

			double X = X_KalmanLowpass.Output(X_Kalman_LowPass.Value);
			double Y = Y_KalmanLowpass.Output(Y_Kalman_LowPass.Value);
			double Z = Z_KalmanLowpass.Output(Z_Kalman_LowPass.Value);
			XYZ Data = new XYZ(X, Y, Z);
			KalmanLowPass_log.Enqueue(Data);
			PrintXYZ_true(X, Y, Z);
		}

		public void PrintXYZRawCalibrated()
		{
			Console.WriteLine("Raw Calibrated");

			double X = X_Calibrated();
			double Y = Y_Calibrated();
			double Z = Z_Calibrated();
			XYZ Data = new XYZ(X, Y, Z);
			RawCalibrated_log.Enqueue(Data);
			PrintXYZ_true(X, Y, Z);
		}

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
			Console.WriteLine($"Calibrating... ({CalibrateTimerMs / 1000} sec)");
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
					double Xx = Convert.ToDouble(message_split[0]);
					double Yy = Convert.ToDouble(message_split[1]);
					double Zz = Convert.ToDouble(message_split[2]);

					UpdateXYZ(Xx, Yy, Zz);
				}

			}
			catch (TimeoutException) { }
			catch (FormatException) { }
			catch (IndexOutOfRangeException) {}
		}

		#region Values
		public double X_HighPassVal()
		{
			return X_HighPass.Value;
		}

		public double Y_HighPassVal()
		{
			return Y_HighPass.Value;
		}

		public double Z_HighPassVal()
		{
			return Z_HighPass.Value;
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
		#endregion


		public void SetInput(SerialPort port)
		{
			_serialPort = port;

		}

		public void SnapData()
		{
			XYZ Raw = new XYZ(X_Calibrated(), Y_Calibrated(), Z_Calibrated());
			XYZ Kalman = new XYZ(X_KalmanVal(), Y_KalmanVal(), Z_KalmanVal());
			XYZ HighPass = new XYZ(X_HighPassVal(), Y_HighPassVal(), Z_HighPassVal());
			XYZ LowPass = new XYZ(X_LowPass.Value, Y_LowPass.Value, Z_LowPass.Value);

			Raw_Log.Enqueue(Raw);
			Kalman_Log.Enqueue(Kalman);
			Highpass_log.Enqueue(HighPass);
			Lowpass_log.Enqueue(LowPass);
		}

		public void WriteData()
		{
			// WriteToCSV("Raw", Raw_Log);
			// WriteToCSV("Kalman", Kalman_Log);
			// WriteToCSV("Highpass", Highpass_log);
			// WriteToCSV("Lowpass", Lowpass_log);
			// WriteToCSV("KalmanLowpass", KalmanLowPass_log);
			// WriteToCSV("RawCalibrated", RawCalibrated_log);
			WriteToCSV("Kalman-50cm", Kalman_Log,true );
		}

		public void CalibrateInput()
		{
			while (_calibrating)
			{
				if (CalibrateTimer.ElapsedMilliseconds >= CalibrateTimerMs)
				{
					_calibrating = false;
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

		public void CheckStandStill()
		{
			if (X_Queue.Average() < 1)
			{
				X_calibration = X_Queue.Average();

			}
			if (Y_Queue.Average() < 1)
			{
				Y_calibration = Y_Queue.Average();

			}
			if (Z_Queue.Average() < 1)
			{
				Z_calibration = Z_Queue.Average();

			}
		}


	}
}
