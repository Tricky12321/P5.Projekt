using System;
using System.Collections.Generic;
using NeuralNetwork;

namespace NeuralNetwork1
{
    [Serializable]
    public class Neuron
    {
        public List<Double> Weights = new List<double>();
        public double Bias;
        public double Delta;
        public double Value;

        public Neuron()
        {
            Random r = new Random(Environment.TickCount);
            Bias = r.NextDouble();
        }
    }
}
