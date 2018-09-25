using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
namespace Serial
{
	public class DataClass
	{
		private const int StackSize = 100;
        public Int64 X;
        public Int64 Y;
        public Int64 Z;

        private Queue<int> X_Queue = new Queue<int>();
        private Queue<int> Y_Queue = new Queue<int>();
        private Queue<int> Z_Queue = new Queue<int>();
        
		private Queue<int> X_Log = new Queue<int>();
        private Queue<int> Y_Log = new Queue<int>();
        private Queue<int> Z_Log = new Queue<int>();

        // Used to calculate Hz Rate
		private double _hz_Rate = 0;
		private double _hz = 0;
		private object _hzLock = new object();
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

		public void UpdateX(int XVal)
		{
			X_Queue.Enqueue(XVal);
			X_Log.Enqueue(XVal);
			X_Queue.Dequeue();
		}

		public void UpdateY(int YVal)
		{
			Y_Queue.Enqueue(YVal);
			Y_Log.Enqueue(YVal);
			Y_Queue.Dequeue();
		}

		public void UpdateZ(int ZVal)
		{
			Z_Queue.Enqueue(ZVal);
			Z_Log.Enqueue(ZVal);
			Z_Queue.Dequeue();
		}

		public int GetX()
		{
			return Convert.ToInt32(Math.Round(X_Queue.Average(), 0));

		}

		public int GetY()
		{
			return Convert.ToInt32(Math.Round(Y_Queue.Average(), 0));

		}

		public int GetZ()
		{
			return Convert.ToInt32(Math.Round(Z_Queue.Average(), 0));
		}

		public void NewNum() {
			lock (_hzLock) {
				_hz++;
            }
		}

		private void CalculateHz() {
			Thread.Sleep(1000);
			_hz_Rate = _hz;
            lock (_hzLock)
            {
				_hz = 0;
			}
			CalculateHz();
		}

		public double GetHz() {
			return _hz_Rate;
		}


		public void WriteToCSV() {
			if (File.Exists("output.csv")) {
				File.Delete("output.csv");
			}
			File.Create("output.csv");
			using (StreamWriter streamWriter = new StreamWriter("output.csv")) {
				streamWriter.WriteLine("Timer,X,Y,Z");
				int i = 0;
                while (X_Log.Count > 0 && Y_Log.Count > 0 && Z_Log.Count > 0)
				{
					i++;
					int X_local = X_Log.Dequeue();
					int Y_local = Y_Log.Dequeue();
                    int Z_local = Z_Log.Dequeue();
					streamWriter.WriteLine($"{i},{X_local},{Y_local},{Z_local}");
				}
			}
		}
	}
}
