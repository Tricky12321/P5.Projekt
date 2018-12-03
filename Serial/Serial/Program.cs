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
            //var tsfdsljk = System.Environment.CurrentDirectory;

            var test = new FileReader("/Users/stenkaer/Documents/GitHub/P5.Projekt/Serial/Serial/bin/Debug/Test/iris-kopi.arff");
            Instances instances = new Instances(test);
            EM em = new EM();
            em.buildClusterer(instances);

            var sfjhdks = em.getClass();
            var sasasasas = em.toString();

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
			MainMenu.ShowMenu();
        }
    }
}
