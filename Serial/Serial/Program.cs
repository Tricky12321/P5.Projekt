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
            dyn.CalculateNaiveVelocity();

            var test = dyn.CalculateDynamicVelocityList(dyn.NaiveVelocityList.Select(x => x.X).ToList(), dyn.NaiveVelocityList.Select(x => x.TimeOfData).ToList());

            var testet = dyn.CalculatePosition(test);
            foreach (var csdc in testet)
            {
                Console.WriteLine($"\"{csdc.TimeOfData}\", \"{csdc.X}\"");
            }
        }
    }
}
