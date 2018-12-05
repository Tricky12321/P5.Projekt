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


            var path = System.Environment.CurrentDirectory;

            //var fileReader = new FileReader("/home/fryd/Repos/P5.Projekt/Serial/Serial/bin/Debug/iris1.arff");



            //var fileReader1 = new FileReader("/home/fryd/Repos/P5.Projekt/Serial/Serial/bin/Debug/forsøg1_slut_afstand_INS.csv");
            weka.core.converters.CSVLoader cSVLoader = new weka.core.converters.CSVLoader();
            java.io.File file = new java.io.File("start_stop_20m_5m_3s_1_INS1.csv");
            //cSVLoader.setFile(file);
            cSVLoader.setSource(file);


            var tilter = cSVLoader.getDataSet();

            Instances instances = new Instances(tilter);
            
            EM em = new EM();
            em.setNumClusters(3);
            em.setSeed(100);
            em.setNumFolds(10);
            em.setMaxIterations(100);
            em.setMaximumNumberOfClusters(-1);
            em.setNumExecutionSlots(1);
            em.setNumKMeansRuns(10);


            em.buildClusterer(instances);

            var sfjhdks = em.getClass();
            var sasasasas = em.toString();
            var tim = em.getClusterModelsNumericAtts();

            var sdds = em.getClusterPriors();
            var hahah = em.getRevision();

			MainMenu.ShowMenu();
        }
    }
}
