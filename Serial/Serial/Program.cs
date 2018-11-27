using System;
using Serial.Menu;
using Serial.Utility;
using Serial.DynamicCalibrationName;
using System.Linq;
using System.IO;
namespace Serial
{
    class MainClass
    {
        public static void Main()
        {
            string filePath = Directory.GetCurrentDirectory() + "/Test";
            string[] fileNamesArray = Directory.GetFiles(filePath);

            string fileName = string.Empty;

            foreach(string file in fileNamesArray)
            {
                if (file.EndsWith(".csv"))
                {
                    fileName = file;
                    break;
                }
            }

            Load csvController = new Load(fileName);
            csvController.HandleCSV();
            //var test = csvController.AccDataList[20];
            var tesadsasdas = csvController.data.GetAccelerationXYZFromCSV();
            DynamicCalibration dyn = new DynamicCalibration(tesadsasdas);
            dyn.CalibrateResidualSumOfSquares(2.0);
            dyn.CalibrateAccelerationPointCoefficient();

            var test = dyn.CalculateDynamicVelocityList(dyn.NaiveVelocityList);
            //var test = dyn.NaiveVelocityList;
            var testet = dyn.CalculatePosition(test);

            /*
            foreach (var csdc in dyn.AccelerationListRAW)
            {
                Console.WriteLine($"\"{csdc.TimeOfData.ToString().Replace(',','.')}\", \"{csdc.X.ToString().Replace(',', '.')}\"");
            }


            foreach (var csdc in test)
            {
                Console.WriteLine($"\"{csdc.Time.ToString().Replace(',', '.')}\", \"{csdc.Value.ToString().Replace(',', '.')}\"");
            }
            */
            foreach (var csdc in testet)
            {
                Console.WriteLine($"\"{csdc.Time.ToString().Replace(',', '.')}\", \"{csdc.Value.ToString().Replace(',', '.')}\"");
            }
        }
    }
}
