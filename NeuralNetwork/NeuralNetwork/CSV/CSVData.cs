using System;
using System.Collections.Generic;
namespace NeuralNetwork
{
    public class CSVData
    {
        public Pattern Pattern;
        public List<XYZ> AccelerationData = new List<XYZ>();
        public List<XYZ> NormalizedAccelerationData = new List<XYZ>();

        public CSVData(PatternEnum patternEnum)
        {
            Pattern pattern = new Pattern(patternEnum);
            Pattern = pattern;
        }


        public void AddToRawAccelerationData(double x, double y, double z)
        {
            XYZ xyz = new XYZ(x, y, z);
            AccelerationData.Add(xyz);
        }

        public void AddNormalizedAccerlerationData(double x, double y, double z)
        {
            XYZ xyz = new XYZ(x, y, z);
            NormalizedAccelerationData.Add(xyz);
        }


    }
}
