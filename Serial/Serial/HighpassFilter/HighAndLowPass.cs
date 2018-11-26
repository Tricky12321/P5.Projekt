using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Serial.Utility;

namespace Serial.Highpass
{
    public enum PassType
    {
        Highpass,
        Lowpass,
    }
    class HighAndLowPass
    {
        public HighAndLowPass(PassType passType, int period)
        {
            Load load = new Load("new_calibrate_still1_INS.csv");
            load.HandleCSV();
            List<Tuple<double, double>> input = new List<Tuple<double, double>>();
            foreach (var item in load.data.AllDataEntries)
            {
                Tuple<double, double> tup = new Tuple<double, double>(item.INS_Accelerometer.TimeOfData, item.INS_Accelerometer.X);
                input.Add(tup);
            }

            if (passType == PassType.Lowpass)
            {
                printToCSV(doLowPass(input, period));

            }
            else if (passType == PassType.Highpass)
            {
                printToCSV(doHighPass(input, period));
            }
            else
            {
                throw new NotImplementedException("PassType doesnt exist");
            }
        }

        private List<Tuple<double, double>> doLowPass(List<Tuple<double, double>> input, int period)
        {
            List<Tuple<double, double>> listToReturn = new List<Tuple<double, double>>();
            for (int i = 0; i < input.Count - period; i++)
            {
                List<Tuple<double, double>> subRange = input.GetRange(i, period);
                double sumValue = 0;
                foreach (var item in subRange)
                {
                    sumValue += item.Item2;
                }
                sumValue /= subRange.Count;
                double middleTime = subRange[subRange.Count / 2].Item1;
                listToReturn.Add(new Tuple<double, double>(middleTime, sumValue));
            }

            return listToReturn;
        }

        private List<Tuple<double, double>> doHighPass(List<Tuple<double, double>> input, int period)
        {
            List<Tuple<double, double>> lowPassedInput = doLowPass(input, period);
            List<Tuple<double, double>> highPassedInput = input;

            
            for (int i = 0; i < lowPassedInput.Count; i++)
            {
                double newValue = highPassedInput[period / 2 + i].Item2 - lowPassedInput[i].Item2;
                highPassedInput[period/2+i] = new Tuple<double, double>(highPassedInput[period / 2 + i].Item1, newValue);
            }

            return highPassedInput;
        }

        private void printToCSV(List<Tuple<double, double>> output)
        {
            string INSFile = "HPLP_INSdataUpdated.csv";
            if (File.Exists(INSFile))
            {
                File.Delete(INSFile);
            }
            using (StreamWriter FileWriter = File.AppendText(INSFile))
            {
                FileWriter.WriteLine($"Timer,AX,AY");
                int DataCount = output.Count;
                for (int i = 0; i < DataCount; i++)
                {
                    if (output[i] != null)
                    {
                        FileWriter.WriteLine($"\"{output[i].Item1}\"," +
                                             $"\"{output[i].Item2}\"");
                    }
                }
                FileWriter.Close();
            }
        }
    }
}
