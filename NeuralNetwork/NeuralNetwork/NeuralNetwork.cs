using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NeuralNetwork
{
    [Serializable]
    public class NeuralNetwork : SaveLoadableObject<NeuralNetwork>
    {
        public List<Layer> Layers;
        public double LearningRate;
        public int LayerCount
        {
            get
            {
                return Layers.Count;
            }
        }

        public NeuralNetwork(double learningRate, int[] layers)
        {
            if (layers.Length < 2) { return; }

            LearningRate = learningRate;
            Layers = new List<Layer>();

            for (int l = 0; l < layers.Length; l++)
            {
                Layer layer = new Layer(layers[l]);
                Layers.Add(layer);

                for (int n = 0; n < layers[l]; n++)
                {
                    layer.Neurons.Add(new Neuron());
                }

                layer.Neurons.ForEach((nn) =>
                {
                    if (l == 0)
                    {
                        nn.Bias = 0;
                    }
                    else
                    {
                        for (int d = 0; d < layers[l - 1]; d++)
                        {
                            nn.Dendrites.Add(new Dendrite());
                        }
                    }
                });
            }
        }

        public NeuralNetwork()
        {
        }

        public double[] Run(List<double> input)
        {
            if (input.Count != Layers[0].NeuronCount) { return null; }

            for (int l = 0; l < LayerCount; l++)
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
                            neuron.Value = neuron.Value + Layers[l - 1].Neurons[np].Value * neuron.Dendrites[np].Weight;
                        }

                        neuron.Value = Sigmoid(neuron.Value + neuron.Bias);
                    }
                }
            }

            Layer last = Layers[LayerCount - 1];
            int numOutput = last.NeuronCount;
            double[] output = new double[numOutput];

            for (int i = 0; i < last.NeuronCount; i++)
            {
                output[i] = last.Neurons[i].Value;
            }

            return output;
        }

        private double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        public bool Train(List<double> input, List<double> output)
        {
            if ((input.Count != Layers[0].Neurons.Count) || (output.Count != Layers[LayerCount - 1].NeuronCount)) return false;

            Run(input);

            for (int i = 0; i < Layers[LayerCount - 1].NeuronCount; i++)
            {
                Neuron neuron = Layers[LayerCount - 1].Neurons[i];

                neuron.Delta = neuron.Value * (1 - neuron.Value) * (output[i] - neuron.Value);

                for (int j = LayerCount - 2; j > 2; j--)
                {
                    for (int k = 0; k < Layers[j].NeuronCount; k++)
                    {
                        Neuron n = Layers[j].Neurons[k];

                        n.Delta = n.Value * (1 - n.Value) * Layers[j + 1].Neurons[i].Dendrites[k].Weight * Layers[j + 1].Neurons[i].Delta;
                    }
                }
            }

            for (int i = LayerCount - 1; i > 1; i--)
            {
                for (int j = 0; j < Layers[i].NeuronCount; j++)
                {
                    Neuron n = Layers[i].Neurons[j];
                    n.Bias = n.Bias + (LearningRate * n.Delta);

                    for (int k = 0; k < n.Dendrites.Count; k++)
                        n.Dendrites[k].Weight = n.Dendrites[k].Weight + (LearningRate * Layers[i - 1].Neurons[k].Value * n.Delta);
                }
            }

            return true;
        }

        public override void Save(string filePath, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }

        public override NeuralNetwork Load(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (NeuralNetwork)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
