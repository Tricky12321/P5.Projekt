using System;
using System.Collections.Generic;
namespace NeuralNetwork1
{
    [Serializable]
    public class Layer
    {
        public List<Neuron> Neurons;
        public int NeuronCount
        {
            get
            {
                return Neurons.Count;
            }
        }
        public Layer(int numNeurons)
        {
            Neurons = new List<Neuron>(numNeurons);
        }
    }
}
