using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace NeuralNetwork
{
    public class CSVController
    {
        public enum Point
        {
            X, Y, Z
        }

        int _accelerationLimit = 5;
        public List<CSVData> CSVDataList = new List<CSVData>();

        public CSVController()
        {
        }

        public void GetFiles()
        {

            string currentPath = Directory.GetCurrentDirectory();
            string newPath = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                newPath = Path.GetFullPath(Path.Combine(currentPath, @"..\"));
                newPath = Path.GetFullPath(Path.Combine(newPath, @"..\"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                newPath = currentPath + "/bin";
            }

            foreach (string filePath in Directory.EnumerateFiles(newPath, "*.csv"))
            {
                CSVData data = new CSVData(PatternEnum.forward05m);
                CSVDataList.Add(data);
                CSVReader(filePath, data);
            }
        }

        public void CSVReader(string filePath, CSVData csvData)
        {
            string[] Result = File.ReadAllLines(filePath);
            bool first = true;
            foreach (var item in Result)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    string[] Elements = item.Split('"');
                    List<string> FinalElements = new List<string>();
                    foreach (var Element in Elements)
                    {
                        if (Element != "" && Element != ",")
                        {
                            FinalElements.Add(Element);
                        }
                    }
                    double time = Convert.ToDouble(FinalElements[0]);
                    csvData.AddToRawAccelerationData(double.Parse(FinalElements[1], new NumberFormatInfo() { NumberDecimalSeparator = "," }), double.Parse(FinalElements[2], new NumberFormatInfo() { NumberDecimalSeparator = "," }), double.Parse(FinalElements[3], new NumberFormatInfo() { NumberDecimalSeparator = "," }));

                }
            }
            NormalizeData(csvData, Point.X);
        }

        public void NormalizeData(CSVData csvData, Point pointToNormalize)
        {
            double avgPointsX = 0;
            double avgPointsY = 0;
            double avgPointsZ = 0;

            for (int i = 0; i < csvData.AccelerationData.Count; i++)
            {
                avgPointsX = avgPointsX + csvData.AccelerationData[i].X;
                avgPointsY = avgPointsY + csvData.AccelerationData[i].Y;
                avgPointsZ = avgPointsZ + csvData.AccelerationData[i].Z;

                if (i > 0 && i % 5 == 0)
                {
                    avgPointsX = avgPointsX / 5;
                    avgPointsY = avgPointsY / 5;
                    avgPointsZ = avgPointsZ / 5;

                    avgPointsX = 0.5 + ((0.5 / _accelerationLimit) * (avgPointsX));
                    avgPointsY = 0.5 + ((0.5 / _accelerationLimit) * (avgPointsY));
                    avgPointsZ = 0.5 + ((0.5 / _accelerationLimit) * (avgPointsZ));

                    csvData.AddNormalizedAccerlerationData(avgPointsX, avgPointsY, avgPointsZ);
                    avgPointsX = avgPointsY = avgPointsZ = 0;
                }
            }
        }
    }
}
