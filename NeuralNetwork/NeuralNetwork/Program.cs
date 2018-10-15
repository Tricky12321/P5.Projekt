using System;
using System.Collections.Generic;
using System.Threading;

namespace NeuralNetwork
{
    class Program
    {
        static List<InputOutputData> insOuts = new List<InputOutputData>();

        static void Main(string[] args)
        {
            StartNN();
        }

        private static void StartNN(){
            /*
            CSVController data = new CSVController();
            data.GetFiles();

            List<InputOutputData> inputOutputlist = new List<InputOutputData>();
            foreach(CSVData inout in data.CSVDataList){

                List<double> xValues = new List<double>();
                foreach(XYZ xval in inout.NormalizedAccelerationData){
                    xValues.Add(xval.X);
                }

                inputOutputlist.Add(new InputOutputData(xValues, inout.Pattern.GetArray()));
            }*/
            RandomData();

            UserInterfaceNeuralNetwork UI = new UserInterfaceNeuralNetwork(10000, 0.2, new int[] { 1, 3, 3, 3, 1 }, insOuts);
        }

        private static void RandomData()
        {
            Random r = new Random(Environment.TickCount);
            for (int i = 0; i < 10000; i++)
            {
                double value1 = r.Next(0, 50) / 100.0;
                List<double> ins = new List<double>();
                ins.Add(value1);

                List<double> outs = new List<double>();
                outs.Add(value1 * 2);
                insOuts.Add(new InputOutputData(ins, outs));
            }
        }
    }
}
