using System;
namespace NeuralNetwork1
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
