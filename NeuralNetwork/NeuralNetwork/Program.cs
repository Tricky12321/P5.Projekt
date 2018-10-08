using System;
using System.Collections.Generic;
using System.Threading;

namespace NeuralNetwork
{
    class Program
    {
        static List<InputOutputData> insOuts;

        static void Main(string[] args)
        {
            StartNN();
        }

        private static void StartNN(){
            insOuts = new List<InputOutputData>();

            GenerateRandom();

            UserInterfaceNeuralNetwork UI = new UserInterfaceNeuralNetwork(100000, 0.2, new int[] { 2, 4, 4, 1 }, insOuts);
        }

        private static void GenerateRandom(){
            Random random = new Random(Environment.TickCount);
            for (int i = 0; i < 100; i++)
            {
                List<double> ins = new List<double>();
                double value1 = 0;
                double value2 = 0;

                value1 = random.Next(0, 50);
                value2 = random.Next(0, 50);

                ins.Add(value1 / 100.0);
                ins.Add(value2 / 100.0);

                List<double> outs = new List<double>();
                outs.Add(value1 / 100.0 + value2 / 100.0);
                InputOutputData inOut = new InputOutputData(ins, outs);
                insOuts.Add(inOut);
            }

        }
    }
}
