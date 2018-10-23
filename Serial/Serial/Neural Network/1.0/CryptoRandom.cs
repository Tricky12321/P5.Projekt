using System;
using System.Security.Cryptography;

namespace NeuralNetwork1
{
    public class CryptoRandom
    {
        public double RandomValue;
        public CryptoRandom()
        {
            using (RNGCryptoServiceProvider p = new RNGCryptoServiceProvider()) {
                Random r = new Random(p.GetHashCode());
                RandomValue = r.NextDouble();

            }
        }
    }
}
