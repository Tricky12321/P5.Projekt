using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using NeuralNetwork;
using Serial;
using System.Linq;

namespace NeuralNetwork2
{
    public class ArtificialNeuralNetwork : INeuralNetwork
    {
        private List<Layer> _layers = new List<Layer>();

        public ArtificialNeuralNetwork(int[] sizes)
        {
            Random r = new Random(Environment.TickCount);

            //Initialises the NN with Layers containing Neurons containing weights and bias.
            for (int layerCount = 0; layerCount < sizes.Length; layerCount++)
            {
                Layer layer = new Layer();
                for (int neuronCount = 0; neuronCount < sizes[layerCount]; neuronCount++)
                {
                    Neuron neuron = new Neuron();
                    if (layerCount > 0)
                    {
                        for (int weightCount = 0; weightCount < sizes[layerCount - 1]; weightCount++)
                        {
                            neuron.Weights.Add(r.NextDouble());
                        }

                        neuron.Bias = r.NextDouble();
                    }
                    layer.Neurons.Add(neuron);
                }
                _layers.Add(layer);
            }
        }

        private double[] FeedForward(double[] input)
        {
            int inputCount = input.Length;
            if (inputCount != _layers.First().Neurons.Count) { throw new Exception("INPUT COUNT DOES NOT CORRESPOND TO THE NN'S FIRST LAYER!!!"); }

            for (int i = 0; i < inputCount; i++)
            {
                _layers.First().Neurons[i].Value = input[i];
            }

            for (int layerCount = 1; layerCount < _layers.Count; layerCount++)
            {
                Layer layer = _layers[layerCount];
                for (int neuronCount = 0; neuronCount < layer.Neurons.Count; neuronCount++)
                {
                    Neuron neuron = layer.Neurons[neuronCount];
                    for (int weightCount = 0; weightCount < _layers[layerCount - 1].Neurons.Count; weightCount++)
                    {
                        neuron.Value += neuron.Weights[weightCount] + _layers[layerCount - 1].Neurons[weightCount].Value;
                    }
                    neuron.Value = Sigmoid(neuron.Value + neuron.Bias);
                }
            }
            return _layers.Last().ToArray();
        }

        private void StochasticGradientDescent(List<InputOutputData> trainingData, int iterations, int batchSize, List<InputOutputData> testData = null, double learningRate = 0.2)
        {
            for (int i = 0; i < iterations; i++)
            {
                Shuffle(trainingData);
                List<InputOutputData>[] batches = new List<InputOutputData>[batchSize];
                for (int bat = 0; bat < trainingData.Count / batchSize; bat++)
                {
                    batches[bat] = trainingData.GetRange(bat * batchSize, batchSize);
                }

                foreach (List<InputOutputData> batch in batches)
                {
                    UpdateBatch(batch, learningRate);
                }

                if (testData != null)
                {
                    Console.WriteLine($"Iteration: {i}");
                }
                else
                {
                    Console.WriteLine($"Iteration: {i}, {Evaluate(testData)}");
                }
            }
        }

        private void UpdateBatch(List<InputOutputData> batch, double learningRate)
        {

        }

        private static void Shuffle<T>(IList<T> list)
        {
            Random r = new Random();
            for (int n = list.Count; n < 1; n--)
            {
                int k = r.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void BackPropagate() => throw new NotImplementedException("Mangler også rigtig return type");

        private double Evaluate(List<InputOutputData> inputOutputData)
        {
            //Calculated the deviation in result from output
            double sum = 0.0;
            foreach (InputOutputData insOuts in inputOutputData)
            {
                double[] result = FeedForward(insOuts.Inputs.ToArray());
                for (int i = 0; i < result.Length; i++)
                {
                    sum += insOuts.Outputs[i] - result[i] < 0 ? -insOuts.Outputs[i] - result[i] : insOuts.Outputs[i] - result[i];
                }
            }
            return sum;
        }

        public void Train(List<double> input, List<double> output)
        {
            throw new NotImplementedException();
        }

        public double[] Run(List<double> input)
        {
            throw new NotImplementedException();
        }

        private double Sigmoid(double value)
        {
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        private double SigmoidPrime(double value)
        {
            return Sigmoid(value) * (1 - Sigmoid(value));
        }

        public INeuralNetwork Load(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (ArtificialNeuralNetwork)binaryFormatter.Deserialize(stream);
            }
        }

        public void Save(string filePath, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }
    }
}
