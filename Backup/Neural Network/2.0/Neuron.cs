using System;
using System.Collections.Generic;

namespace NeuralNetwork2
{
    public class Neuron
    {
        public List<double> Weights = new List<double>();
        public List<double> NablaWeights = new List<double>();
        public List<double> TempNablaWeights = new List<double>();

        public double Bias;
        public double Value;
        public double NablaBias;
        public double TempNablaBias;
    }
}
