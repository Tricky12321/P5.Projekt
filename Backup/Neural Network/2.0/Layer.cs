using System;
using System.Collections.Generic;

namespace NeuralNetwork2
{
    [Serializable]
    public class Layer
    {
        public List<Neuron> Neurons = new List<Neuron>();

        public double[] ToArray()
        {
            double[] valueArray = new double[Neurons.Count];
            for (int neuronCount = 0; neuronCount < Neurons.Count; neuronCount++)
            {
                valueArray[neuronCount] = Neurons[neuronCount].Value;
            }
            return valueArray;
        }
    }
}
