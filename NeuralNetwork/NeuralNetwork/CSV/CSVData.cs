using System;
using System.Collections.Generic;
namespace NeuralNetwork
{
    public class CSVData
    {
        public List<XYZ> AccelerationData = new List<XYZ>();

        public List<XYZ> NormalizedAccelerationData = new List<XYZ>();

        public CSVData()
        {

        }


        public void AddToRawAccelerationData(double x, double y, double z)
        {
            XYZ xyz = new XYZ(x, y, z);
        }

        public void AddNormalizedAccerlerationData(double x, double y, double z)
        {
            XYZ xyz = new XYZ(x, y, z);
        }


    }
}
