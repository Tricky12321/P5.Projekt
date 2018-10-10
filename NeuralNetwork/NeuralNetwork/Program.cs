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

            CSVController data = new CSVController();
            data.GetFiles();

            List<Tuple<List<double>, List<double>>> inputOutputlist = new List<Tuple<List<double>, List<double>>>();
            foreach(CSVData inout in data.CSVDataList){

                List<double> xValues = new List<double>();
                foreach(XYZ xval in inout.NormalizedAccelerationData){
                    xValues.Add(xval.X);
                }

                inputOutputlist.Add(Tuple.Create(xValues, inout.Pattern.GetArray()));
            }

            UserInterfaceNeuralNetwork UI = new UserInterfaceNeuralNetwork(1000000, 100, new int[] { 27, 50, 50, 50, 2 }, inputOutputlist);
        }

        private static void GenerateRandom(){
            Random random = new Random(Environment.TickCount);
            for (int i = 0; i < 1000; i++)
            {
                List<double> ins = new List<double>();
                double value1 = 0;
                double value2 = 0;
                double value3 = 0;

                value1 = random.Next(0, 30);
                value2 = random.Next(0, 30);
                value3 = random.Next(0, 30);

                ins.Add(value1 / 100.0);
                ins.Add(value2 / 100.0);
                ins.Add(value3 / 100.0);

                List<double> outs = new List<double>();
                outs.Add(value1 / 100.0 + value2 / 100.0 + value3 / 100.0);
                InputOutputData inOut = new InputOutputData(ins, outs);
                insOuts.Add(inOut);
            }

        }
    }
}
