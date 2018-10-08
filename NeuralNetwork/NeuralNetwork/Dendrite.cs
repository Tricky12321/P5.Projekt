using System;
namespace NeuralNetwork
{
    [Serializable]
    public class Dendrite
    {
        public double Weight;

        public Dendrite()
        {
            CryptoRandom n = new CryptoRandom();
            Weight = n.RandomValue;
        }
    }
}
