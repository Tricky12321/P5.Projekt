using System;
using System.Collections.Generic;
using NeuralNetwork;

namespace NeuralNetwork1
{
    [Serializable]
    public class Neuron
    {
        public List<Dendrite> Dendrites;
        public double Bias;
        public double Delta;
        public double Value;

        public int DentriesCount
        {
            get
            {
                return Dendrites.Count;
            }
        }

        public Neuron()
        {
            CryptoRandom n = new CryptoRandom();
            Bias = n.RandomValue;

            Dendrites = new List<Dendrite>();
        }
    }
}
