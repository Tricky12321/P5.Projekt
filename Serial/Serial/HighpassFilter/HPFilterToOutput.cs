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

        public void trySomething()
        {
            Load load = new Load("still10sek2_INS.csv");
            load.HandleCSV();

            double firstValue = 0;
            double secondValue = 0;
            double thirdValue = 0;

            foreach (var item in load.data.AllDataEntries)
            {

            }

        }

        public void test()
        {
            Load load = new Load("still10sek2_INS.csv");
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
            /*
            HighpassFilter HPX = new HighpassFilter(41.5f, 100, HighpassFilter.PassType.Highpass, (float)Math.Sqrt(2));
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
            }*/


            int NZEROS = 4;
            int NPOLES = 4;
            float GAIN = 5.981602912e+00f;
            double output;
            float[] xv = new float[NZEROS + 1], yv = new float[NPOLES + 1];

            foreach (var tup in liste)
            {
                xv[0] = xv[1]; xv[1] = xv[2]; xv[2] = xv[3]; xv[3] = xv[4];
                xv[4] = (float)tup.Item2 / GAIN;
                yv[0] = yv[1]; yv[1] = yv[2]; yv[2] = yv[3]; yv[3] = yv[4];
                yv[4] = (xv[0] + xv[4]) - 4 * (xv[1] + xv[3]) + 6 * xv[2]
                     + (-0.0301188750f * yv[0]) + (0.1826756978f * yv[1])
                     + (-0.6799785269f * yv[2]) + (0.7820951980f* yv[3]);
                output = yv[4];
                listeNew.Add(new Tuple<double, double, double>(tup.Item1, output, 0));

            }            

            using (StreamWriter FileWriter = File.AppendText(INSFile))
            {
                FileWriter.WriteLine($"Timer,AX,AY");
                int DataCount = liste.Count;
                for (int i = 0; i < DataCount; i++)
                {
                    if (liste[i] != null)
                    {
                        FileWriter.WriteLine($"\"{listeNew[i].Item1}\"," +
                                             $"\"{listeNew[i].Item2}\"," +
                                             $"\"{listeNew[i].Item3}\"");
                    }
                }
                FileWriter.Close();
            }
        }
    
    }
}
