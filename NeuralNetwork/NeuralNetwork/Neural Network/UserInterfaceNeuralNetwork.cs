using System;
using System.Collections.Generic;

namespace NeuralNetwork
{
    public class UserInterfaceNeuralNetwork
    {
        public int TrainingIterations;
        public double TrainingRate;
        public NeuralNetwork nn;
        public List<InputOutputData> InputOutputsList;

        public UserInterfaceNeuralNetwork(int trainingIterations, double trainingRate, int[] layerArray, List<InputOutputData> inputOutputsList)
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
                        while (DoneInput)
                        {
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
                        }
                        Try(inputsList);
                        break;
                    case "Train":
                    case "train":
                        Train();
                        break;
                    case "Save":
                    case "save":
                        string FileNameS = Console.ReadLine();
                        Save(FileNameS);
                        break;
                    case "Load":
                    case "load":
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
            Console.WriteLine("TRAINING STARTED!");
            for (int i = 0; i < TrainingIterations; i++)
            {
                Console.WriteLine(i);
                int count = InputOutputsList.Count;
                for (int j = 0; j < count; j++)
                {
                    nn.Train(InputOutputsList[j].Inputs, InputOutputsList[j].Outputs);
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