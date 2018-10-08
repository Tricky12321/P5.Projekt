using System;
using System.Collections.Generic;
using System.IO;

namespace NeuralNetwork
{
    public class CSVController
    {

        int _accelerationLimit = 5;
        public List<double> AccelerationDataX = new List<double>();
        public List<double> NormalizedAccelerationDataX = new List<double>();
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
                CSVData data = new CSVData();
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
                    //csvData.X = Convert.ToDouble(FinalElements[1]);
                    //csvData.Y = Convert.ToDouble(FinalElements[2]);
                    //csvData.Z = Convert.ToDouble(FinalElements[3]);

                    CSVDataList.Add(csvData);
                }
            }
        }

        public void NormalizeData()
        {

            double avgPoints = 0;

            for (int i = 0; i < AccelerationDataX.Count; i++)
            {
                avgPoints = avgPoints + AccelerationDataX[i];

                if (i > 0 && i % 5 == 0)
                {
                    avgPoints = avgPoints / 5;
                    avgPoints = 0.5 + ((0.5 / _accelerationLimit) * (avgPoints));

                    NormalizedAccelerationDataX.Add(avgPoints);
                    avgPoints = 0;
                }
            }

        }
    }
}
