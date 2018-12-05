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

            Clustering.Clustering clustering = new Clustering.Clustering("start_stop_20m_5m_3s_1_INS.csv");
            var hej = clustering.GetClusters();

            MainMenu.ShowMenu();

        }
    }
}
