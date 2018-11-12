using System;
using System.Collections.Generic;
using System.Linq;

namespace Serial.CSV
{
    public class CSVData
    {
        public List<XYZ> AccelerationData = new List<XYZ>();
        public double NormalizeValue;


        public CSVData(double normalizeValue)
        {
            NormalizeValue = normalizeValue;
        }

        public void InsertXYZ(XYZ xyz)
        {
            AccelerationData.Add(xyz);
        }

        public List<XYZ> GetRaw()
        {
            return AccelerationData;
        }

        public List<XYZ> GetNormalized()
        {
            List<XYZ> listToReturn = new List<XYZ>();
            foreach(XYZ xyz in AccelerationData)
            {
                listToReturn.Add(NormalizeXYZ(xyz));
            }
            return listToReturn;
        }

        public List<XYZ> GetRunningAverage(int subsetCount, bool normalized)
        {
            List<XYZ> listToReturn = new List<XYZ>();

            for (int i = 0; i < AccelerationData.Count-subsetCount; i++)
            {
                List<XYZ> averageList = AccelerationData.GetRange(i, subsetCount);
                XYZ placeXyz = new XYZ();
                placeXyz.X = averageList.Select(x => x.X).Average();
                placeXyz.Y = averageList.Select(x => x.Y).Average();
                placeXyz.Z = averageList.Select(x => x.Z).Average();

                if (normalized)
                {
                    listToReturn.Add(NormalizeXYZ(placeXyz));
                }
                else
                {
                    listToReturn.Add(placeXyz);
                }
            }
            return listToReturn;
        }

        private XYZ NormalizeXYZ(XYZ xyz)
        {
            return new XYZ(0.5 + 1.0 / (2 * NormalizeValue) * xyz.X, 0.5 + 1.0 / (2 * NormalizeValue) * xyz.Y, 0.5 + 1.0 / (2 * NormalizeValue) * xyz.Z, xyz.TimeOfData);
        }
    }
}
