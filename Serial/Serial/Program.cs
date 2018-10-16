using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NeuralNetwork;

namespace Serial
{
	class MainClass
	{ 
		public static void Main()
		{
            INS_POSZYX_NeuralNetworkTester nnTester = new INS_POSZYX_NeuralNetworkTester();
            nnTester.Start();

		}
	}
}
