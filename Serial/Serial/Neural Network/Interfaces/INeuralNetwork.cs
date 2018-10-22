using System;
using System.Collections.Generic;
using NeuralNetwork;

namespace Serial
{
    public interface INeuralNetwork : ISaveLoadableObject<INeuralNetwork>
    {
        double[] Run(List<double> input);
        void Train(List<double> input, List<double> output);
    }
}
