using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Serial
{
	public class DataClass
	{
		private const int StackSize = 100;
        public Int64 X;
        public Int64 Y;
        public Int64 Z;

        private Queue<int> X_Stack = new Queue<int>();
        private Queue<int> Y_Stack = new Queue<int>();
        private Queue<int> Z_Stack = new Queue<int>();
        
		private double _hz_Rate = 0;
		private double _hz = 0;
		private object _hzLock = new object();
		public DataClass()
		{
			for (int i = 0; i < StackSize; i++)
			{
				X_Stack.Enqueue(0);
				Y_Stack.Enqueue(0);
				Z_Stack.Enqueue(0);
			}
			Thread HzThread = new Thread(CalculateHz);
			HzThread.Start();

		}

		public void UpdateX(int XVal)
		{
			X_Stack.Enqueue(XVal);
			X_Stack.Dequeue();
		}

		public void UpdateY(int YVal)
		{
			Y_Stack.Enqueue(YVal);
			Y_Stack.Dequeue();
		}

		public void UpdateZ(int ZVal)
		{
			Z_Stack.Enqueue(ZVal);
			Z_Stack.Dequeue();
		}

		public int GetX()
		{
			return Convert.ToInt32(Math.Round(X_Stack.Average(), 0));

		}

		public int GetY()
		{
			return Convert.ToInt32(Math.Round(Y_Stack.Average(), 0));

		}

		public int GetZ()
		{
			return Convert.ToInt32(Math.Round(Z_Stack.Average(), 0));
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
	}
}
