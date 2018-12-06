using System;
using Serial.Menu;
using Serial.Utility;
using Serial.DynamicCalibrationName;
using System.Linq;
using System.IO;
using System.Threading;
using System.Globalization;
using weka.clusterers;
using weka.core;
using java.io;

namespace Serial
{
    class MainClass
    {
        public static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            MainMenu.ShowMenu();
        }
    }
}
