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

        private string _filePath;


        public CSVController()
        {
        }



        public void CSVReader()
        {

            string[] Result = File.ReadAllLines(_filePath);
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
                    double X = Convert.ToDouble(FinalElements[1]);
                    double Y = Convert.ToDouble(FinalElements[2]);
                    double Z = Convert.ToDouble(FinalElements[3]);


                    AccelerationDataX.Add(X);
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
