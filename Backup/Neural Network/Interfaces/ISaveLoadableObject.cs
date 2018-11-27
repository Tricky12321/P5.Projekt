using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NeuralNetwork
{
    public interface ISaveLoadableObject<T>
    {
        void Save(string filePath, bool append = false);
        T Load(string filePath);
    }
}
