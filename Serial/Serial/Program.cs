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


		INS_POSZYX_NeuralNetworkTester nn = new INS_POSZYX_NeuralNetworkTester();
        public static void Main()
        {
            ShowMenu();
        }



		public static void ShowMenu() {

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
					case "exit":
						Environment.Exit(0);
                        break;
					case "clear":
						Console.Clear();;
                        break;
					case "devices":
						PrintDevices();
                        break;
					default:
						Console.WriteLine("Unknown command!");
						break;
				}
			} while (!Exit);
		}



		public static void PrintCommands() {
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("nn - Neural network");
			Console.WriteLine(" - start - Starts the Neural Network");
			Console.WriteLine(" - stop - Stops the Neural Network");
			Console.WriteLine(" - save - Saves the Neural Network to a file");
			Console.WriteLine(" - load - Loads the Neural Network from a file");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("devices - Prints arduino devices");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("help - shows this page");
            Console.WriteLine("clear - clear this page");
			Console.WriteLine("-----------------------------------");
		}

		public static void PrintDevices() {
			foreach (var Port in SerialReader.GetOpenPorts())
			{
				Console.WriteLine($"Port: {Port}");
			}
		}

		public static void NeuralNetwork(string[] Input) {
			List<string> InputList = new List<string>(Input);

			if (InputList.Count == 1) {
				Console.WriteLine("Invalid input format, use help command!");
				return;
			}

			switch (Input[1]) {
				case "start":
					nn.Start();
					break;
				case "stop":
                    nn.Stop();
                    break;
				default:
					Console.WriteLine("Invalid input format, use help command!");
                    return;
			}
		}




    }
}
