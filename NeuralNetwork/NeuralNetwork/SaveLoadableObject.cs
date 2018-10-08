using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NeuralNetwork
{
    [Serializable]
    public abstract class SaveLoadableObject<T>
    {
        public abstract void Save(string filePath, bool append = false);

        public abstract T Load(string filePath);
    }
}
