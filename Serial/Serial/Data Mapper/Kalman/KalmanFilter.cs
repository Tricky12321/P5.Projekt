using System;
using KalmanFilters;
using System.Collections.Generic;
using System.Linq;
namespace Serial.DataMapper.Kalman
{
	public class KalmanFilter
	{
		private double A, H, Q, R, P, x;

		public static List<XYZ> KalmanData(List<XYZ> Input) {
			List<double> X = new List<double>();
			List<double> Y = new List<double>();
			List<double> Z = new List<double>();
			List<double> Tid = new List<double>();
			foreach (var item in Input)
			{
				Tid.Add(item.TimeOfData);
				X.Add(item.X);
				Y.Add(item.Y);
				Z.Add(item.Z);
			}
			X = RunFilter(X);
			Y = RunFilter(Y);
			Z = RunFilter(Z);
			int count = X.Count();
			List<XYZ> Output = new List<XYZ>();
            for (int i = 0; i < count; i++)
			{
				Output.Add(new XYZ(X[i], Y[i], Z[i],Tid[i]));
			}
			return Output;
		}

		public static List<double> RunFilter(List<double> measurements)
		{
			List<double> motion = new List<double>();
			motion.Add(0);
			int counts = measurements.Count();
			for (int i = 1; i < counts; i++)
			{
				motion.Add(measurements[i] - measurements[i - 1]);
			}
			double measurement_sigma = measurements.Max() - measurements.Min();
			double motion_sigma = motion.Max()-motion.Min();
			double mu = 0;
			double sigma = 10000;
			List<double> Output = new List<double>();
			KalmanFilter1D filter = new KalmanFilter1D(mu, sigma, measurement_sigma, motion_sigma);
			for (int t = 0; t < counts; ++t)
			{
				filter.Update(measurements[t]);
				filter.Predict(motion[t]);
				Console.WriteLine(filter.State);
				Output.Add(filter.State);
			}
			return Output;
		}

		public KalmanFilter(double A, double H, double Q, double R, double initial_P, double initial_x)
		{
			this.A = A;
			this.H = H;
			this.Q = Q;
			this.R = R;
			this.P = initial_P;
			this.x = initial_x;
		}

		public double Output(double input)
		{
			// time update - prediction
			x = A * x;
			P = A * P * A + Q;

			// measurement update - correction
			double K = P * H / (H * P * H + R);
			x = x + K * (input - H * x);
			P = (1 - K * H) * P;

			return x;
		}
	}
}
