using System;
using System.Collections.Generic;
using System.IO;

namespace NeuralNetwork
{
    public class CSVController
    {
        int _accelerationLimit = 5;
        public List<CSVData> CSVDataList = new List<CSVData>();

        public CSVController()
        {
        }

        public void GetFiles(){

            List<CSVData> CSVData = new List<CSVData>();

            string path = Directory.GetCurrentDirectory();
            string newPath = Path.GetFullPath(Path.Combine(path, @"..\"));

            foreach (string filePath in Directory.EnumerateFiles(newPath, "*.csv"))
            {
                CSVData data = new CSVData(PatternEnum.forward05m);
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
                            FinalElements.Add(Element.Replace(',', '.'));
                        }
                    }
                    double time = Convert.ToDouble(FinalElements[0]);

                    CSVDataList.Add(csvData);
                    csvData.AddToRawAccelerationData(double.Parse(FinalElements[1]), double.Parse(FinalElements[2]), double.Parse(FinalElements[3]));

                    NormalizeData(csvData);
                }
            }
        }

        public void NormalizeData(CSVData csvData)
        {
            double avgPoints = 0;

            for (int i = 0; i < csvData.AccelerationData.Count; i++)
            {
                avgPoints = avgPoints + csvData.AccelerationData[i].X;

                if (i > 0 && i % 5 == 0)
                {
                    avgPoints = avgPoints / 5;
                    avgPoints = 0.5 + ((0.5 / _accelerationLimit) * (avgPoints));

                    csvData.AddNormalizedAccerlerationData(avgPoints, 0, 0);
                    avgPoints = 0;
                }
            }

        }
    }
}
