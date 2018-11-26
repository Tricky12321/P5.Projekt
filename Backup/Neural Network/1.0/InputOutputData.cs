using System;
using System.Collections.Generic;

namespace NeuralNetwork1
{
    public class InputOutputData
    {
        public List<double> Inputs;
        public List<double> Outputs;

        public InputOutputData(List<double> inputs, List<double> outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }
    }
}
