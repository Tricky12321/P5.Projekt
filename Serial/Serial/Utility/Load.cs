using System;
using Serial.DataMapper;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace Serial.Utility
{
	public enum CSVTypes
	{
		POZYX, INS, UNKNOWN
	}

	public class Load
	{
		string Path = "";
		public DataMapper.DataMapper data = new DataMapper.DataMapper(false, false);
		public Load(string Path)
		{
			this.Path = Path;
		}

        private double StringToDouble(string doubleString){
            double value;
            // Probably want to use a more specific NumberStyles selection here.
            if (!double.TryParse(doubleString, NumberStyles.Any, new CultureInfo("en-US", false), out value))
            {
                throw new InvalidCastException($"There are errors in the CSV file : File string was {doubleString} : Path was {Path}");
            }
            return value;
        }

		public void HandleCSV()
		{
			if (!File.Exists(Path)) {
				Console.WriteLine("File does not exist!");
				return;
			}

			data.ClearEntries();
			CSVTypes Type = GetCSVType();
			List<string> DataList = new List<string>(File.ReadAllLines(Path));
			DataList.RemoveAt(0);
			if (Type == CSVTypes.INS) {
				foreach (var line in DataList)
				{
					string LineReplaced = line.Replace("\",\"", "#");
					int Length = LineReplaced.Length;
					string[] LineSplit = LineReplaced.Substring(1, LineReplaced.Length - 2).Split('#');
					// INS
                    double Timer = StringToDouble(LineSplit[0]);
					double AX = StringToDouble(LineSplit[1]);
					double AY = StringToDouble(LineSplit[2]);
					double AZ = StringToDouble(LineSplit[3]);
					double GX = StringToDouble(LineSplit[4]);
					double GY = StringToDouble(LineSplit[5]);
					double GZ = StringToDouble(LineSplit[6]);
					double Angle = 0f;
					if (LineSplit.Length == 8) {
						Angle = Convert.ToDouble(LineSplit[7]);
                    }
					data.AddDataEntry(new DataEntry(null, new XYZ(AX, AY, AZ, Timer), new XYZ(GX, GY, GZ, Timer),Angle));
				}
				Console.WriteLine($"Loaded {DataList.Count} lines from {Path} [{Type}]");
                
			} else if (Type == CSVTypes.POZYX) {
				foreach (var line in DataList)
                {
					// Replace "," with # and skip first char " and last char "
                    string[] LineSplit = line.Replace("\",\"", "#").Substring(1, line.Length - 2).Split('#');
                    // POZYX
                    double Timer = StringToDouble(LineSplit[0]);
                    double X = StringToDouble(LineSplit[1]);
                    double Y = StringToDouble(LineSplit[2]);
                    double Z = StringToDouble(LineSplit[3]);
					data.AddDataEntry(new DataEntry(new XYZ(X, Y, Z, Timer), null, null,0));
                }
				Console.WriteLine($"Loaded {DataList.Count} lines from {Path} [{Type}]");
			} else {
				Console.WriteLine("Type UNKNOWN!!!!");
			}
        }

		public CSVTypes GetCSVType()
		{
			if (Path.ToLower().Contains("ins"))
			{
				return CSVTypes.INS;
			}
			else if (Path.ToLower().Contains("pozyx"))
			{
				return CSVTypes.POZYX;
			}
			else
			{
				return CSVTypes.UNKNOWN;
			}
		}
	}
}
