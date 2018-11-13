using System;
using System.Collections.Generic;
using System.IO;

namespace Serial.CSV
{
    public class CSVController
    {
        public List<CSVData> AccDataList = new List<CSVData>();
        public List<CSVData> GyroDataList = new List<CSVData>();
        public List<CSVData> PozyxDataList = new List<CSVData>();

        public CSVController()
        {

        }

        public void GetFiles()
        {

            List<CSVData> CSVData = new List<CSVData>();

            string path = Directory.GetCurrentDirectory();
            path += "/Test";

            foreach (string filePath in Directory.EnumerateFiles(path, "*.csv"))
            {
                CSVReader(filePath);
            }

        }

        public void CSVReader(string filePath)
        {
            if (filePath.Contains("POZYX"))
            {
                CSVData dataPos = new CSVData(20000.0);

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

                        long timeStamp = Convert.ToInt64(FinalElements[0]);
                        double posX = Convert.ToDouble(FinalElements[1]);
                        double posY = Convert.ToDouble(FinalElements[2]);
                        double posZ = Convert.ToDouble(FinalElements[3]);

                        dataPos.InsertXYZ(new XYZ(posX, posY, posZ, timeStamp));
                    }
                }
                PozyxDataList.Add(dataPos);
            }
            else
            {
                CSVData dataAcc = new CSVData(5000.0);
                CSVData dataGyro = new CSVData(5000.0);

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

                        long timeStamp = Convert.ToInt64(FinalElements[0]);
                        double accX = Convert.ToDouble(FinalElements[1]);
                        double accY = Convert.ToDouble(FinalElements[2]);
                        double accZ = Convert.ToDouble(FinalElements[3]);

                        double gyrX = Convert.ToDouble(FinalElements[4]);
                        double gyrY = Convert.ToDouble(FinalElements[5]);
                        double gyrZ = Convert.ToDouble(FinalElements[6]);

                        dataAcc.InsertXYZ(new XYZ(accX, accY, accZ, timeStamp));
                        dataGyro.InsertXYZ(new XYZ(gyrX, gyrY, gyrZ, timeStamp));
                    }
                }
                AccDataList.Add(dataAcc);
                GyroDataList.Add(dataGyro);
            }
        }
    }
}
