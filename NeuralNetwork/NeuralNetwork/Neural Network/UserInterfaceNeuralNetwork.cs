using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeuralNetwork
{
    public class UserInterfaceNeuralNetwork
    {
        public int TrainingIterations;
        public double TrainingRate;
        public NeuralNetwork nn;
        public List<Tuple<List<double>,List<double>>> InputOutputsList;

        public UserInterfaceNeuralNetwork(int trainingIterations, double trainingRate, int[] layerArray, List<Tuple<List<double>, List<double>>> inputOutputsList)
        {
            TrainingIterations = trainingIterations;
            TrainingRate = trainingRate;
            InputOutputsList = inputOutputsList;

            nn = new NeuralNetwork(trainingRate, layerArray);

            Start();
           
        }

        private void Start(){
            while (true)
            {
                String rLine = Console.ReadLine();
                switch (rLine)
                {
                    case "Try":
                    case "try":
                        bool DoneInput = true;
                        String readLine;
                        List<double> inputsList = new List<double>();
                        inputsList = InputOutputsList[9].Item1;
                        /*while (DoneInput)
                        {
                            Console.WriteLine("WRITE A DOUBLE!");
                            readLine = Console.ReadLine();
                            if (readLine == "done" || readLine == "Done")
                            {
                                DoneInput = false;
                            }
                            double lineValue = 0.0;
                            if (double.TryParse(readLine, out lineValue) && DoneInput)
                            {
                                inputsList.Add(lineValue);
                            }
                        }*/

                        Try(inputsList);
                        break;
                    case "Train":
                    case "train":
                        Train();
                        break;
                    case "Save":
                    case "save":
                        Console.WriteLine("FILENAME:");
                        string FileNameS = Console.ReadLine();
                        Save(FileNameS);
                        break;
                    case "Load":
                    case "load":
                        Console.WriteLine("FILENAME:");
                        string FileNameL = Console.ReadLine();
                        Load(FileNameL);
                        break;
                    default:
                        Console.WriteLine("INPUT NOT ACCEPTED!");
                        break;
                }
            }
        }

        private void Try(List<double> input)
        {
            input.ForEach(x => Console.WriteLine(x));

            double[] outs = nn.Run(input);
            foreach (double outx in outs)
            {
                Console.WriteLine($"Result: {outx}");
            }
        }

        private void Train()
        {
            int percent = 0;
            double timePrPercent = 0;
            Stopwatch stopWatch = new Stopwatch();
            Console.WriteLine("TRAINING STARTED!");
            for (int i = 0; i < TrainingIterations; i++)
            {
                if (Convert.ToInt32(100.0 / TrainingIterations * i) != percent)
                {
                    timePrPercent = stopWatch.ElapsedMilliseconds / 1000.0;
                    stopWatch.Restart();

                    Console.WriteLine($"{percent + 1}% : {Math.Round((100 - percent) * timePrPercent, 0)} sec left");
                    percent = Convert.ToInt32(100.0 / TrainingIterations * i);
                }

                int count = InputOutputsList.Count;

                for (int j = 0; j < count; j++)
                {
                    nn.Train(InputOutputsList[j].Item1, InputOutputsList[j].Item2);
                }
            }
            Console.WriteLine("TRAINING DONE!");
        }

        private void Save(String fileName)
        {
            nn.Save($"{fileName}.nn", false);
            Console.WriteLine("NEURAL NETWORK SAVED!");
        }

        private void Load(String fileName)
        {
            nn = nn.Load($"{fileName}.nn");
            Console.WriteLine("NEURAL NETWORK LOADED!");
        }
    }
}