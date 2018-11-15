using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Serial.Highpass
{
    class HPFilterToOutput
    {

        public HPFilterToOutput()
        {
            test();
        }

        public void test()
        {
            Load load = new Load("frem_5m_2_INS.csv");
            load.HandleCSV();

            string INSFile = "opdateretINS.csv";
            if (File.Exists(INSFile))
            {
                File.Delete(INSFile);
            }

            List<Tuple<double, double, double>> liste = new List<Tuple<double, double, double>>();
            List<Tuple<double, double, double>> listeNew = new List<Tuple<double, double, double>>();
            foreach (var item in load.data.AllDataEntries)
            {
                Tuple<double, double, double> tup = new Tuple<double, double, double>(item.INS_Accelerometer.TimeOfData, item.INS_Accelerometer.X, item.INS_Accelerometer.Y);
                if (!(liste.Any(val => val.Item1 == tup.Item1)))
                {
                    liste.Add(tup);
                }
            }
            HighpassFilter HPX = new HighpassFilter(25, 100, HighpassFilter.PassType.Highpass, (float)Math.Sqrt(2));
            HighpassFilter HPY = new HighpassFilter(45, 100, HighpassFilter.PassType.Highpass, (float)Math.Sqrt(2));
            liste.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            foreach (var tup in liste)
            {
                //Console.Write($"{tup.Item2}:   new value:");
                HPX.Update((float)tup.Item2);
                HPY.Update((float)tup.Item3);
                listeNew.Add(new Tuple<double, double, double>(tup.Item1, HPX.Value, HPY.Value));
                //Console.WriteLine(HP.Value);
                //Console.ReadKey();
            }
        }
    
    }
}
