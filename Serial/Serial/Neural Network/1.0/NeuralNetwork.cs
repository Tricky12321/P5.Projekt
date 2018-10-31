using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Serial;

namespace NeuralNetwork1
{
    [Serializable]
    public class NeuralNetwork : INeuralNetwork
    {
        public List<Layer> Layers;
        public double LearningRate;
        private double _errors;
        private int _layerCount;

        public NeuralNetwork() { }

        public NeuralNetwork(double learningRate, int[] layers)
        {
            if (layers.Length < 2) { return; }

            LearningRate = learningRate;
            Layers = new List<Layer>();

            Random r = new Random(Environment.TickCount);

            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = new Layer(layers[l]);
                Layers.Add(layer);

                for (int n = 0; n < layers[l]; n++)
                {
                    layer.Neurons.Add(new Neuron());
                }

                layer.Neurons.ForEach(nn =>
                {
                    if (l == 0)
                    {
                        nn.Bias = 0;
                    }
                    else
                    {
                        for (int d = 0; d < layers[l - 1]; d++)
                        {
                            nn.Weights.Add(r.NextDouble());
                        }
                    }
                });
            }
            _layerCount = Layers.Count;
        }

        public double[] Run(List<double> input)
        {
            if (input.Count != Layers[0].NeuronCount) { return null; }

            for (int l = 0; l < _layerCount; l++)
            {
                Layer layer = Layers[l];

                for (int n = 0; n < layer.NeuronCount; n++)
                {
                    Neuron neuron = layer.Neurons[n];

                    if (l == 0)
                    {
                        neuron.Value = input[n];
                    }
                    else
                    {
                        neuron.Value = 0;
                        for (int np = 0; np < Layers[l - 1].NeuronCount; np++)
                        {
                            neuron.Value = neuron.Value + Layers[l - 1].Neurons[np].Value * neuron.Weights[np];
                        }

                        neuron.Value = Sigmoid(neuron.Value + neuron.Bias);
                    }
                }
            }

            Layer last = Layers[_layerCount - 1];
            int numOutput = last.NeuronCount;
            double[] output = new double[numOutput];

            for (int i = 0; i < last.NeuronCount; i++)
            {
                output[i] = last.Neurons[i].Value;
            }

            return output;
        }

        private static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        public void Train(List<double> input, List<double> output)
        {
            if ((input.Count != Layers[0].Neurons.Count) || (output.Count != Layers[_layerCount - 1].NeuronCount)) return;

            Run(input);

            for (int i = 0; i < Layers[_layerCount - 1].NeuronCount; i++)
            {
                Neuron neuron = Layers[_layerCount - 1].Neurons[i];
                Console.WriteLine($"Neuron Index : {i}");
                Console.WriteLine($"Forventet Output: {output[i]}");
                Console.WriteLine($"Beregnet Output: {neuron.Value}");
                _errors = output[i] - neuron.Value;
                Console.WriteLine($"Error margin: {_errors}");
                neuron.Delta = output[i] - neuron.Value;
            }

            for (int i = _layerCount - 1; i > 1; i--)
            {
                for (int j = 0; j < Layers[i].NeuronCount; j++)
                {
                    Neuron n = Layers[i].Neurons[j];
                    n.Bias = n.Bias + (LearningRate * n.Delta);

                    for (int k = 0; k < n.Weights.Count; k++)
                    {
                        n.Weights[k] = n.Weights[k] + (LearningRate * Layers[i - 1].Neurons[k].Value * n.Delta);
                    }
                }
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

        public INeuralNetwork Load(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (NeuralNetwork)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
