using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Serial.CSV;
using Serial.DynamicCalibrationName;

namespace Serial
{
    class MainClass
    {
        public static void Main()
        {
            CSVController csvController = new CSVController();
            csvController.GetFiles();
            //var test = csvController.AccDataList[20];
            DynamicCalibration dyn = new DynamicCalibration(csvController.AccDataList[0].AccelerationData);
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
            }*/
            foreach (var csdc in testet)
            {
                Console.WriteLine($"\"{csdc.Time.ToString().Replace(',', '.')}\", \"{csdc.Value.ToString().Replace(',', '.')}\"");
            }
        }
    }
}
