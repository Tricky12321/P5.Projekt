using System;
namespace Serial.DataMapper
{
    public class DataEntry
    {
		private static int entryNum_Incrementer = 1;
		public XYZ PoZYX;
		public XYZ INS_Accelerometer;
		public XYZ INS_Gyroscope;
		public int EntryNum = 0;
        public bool Used = false;

        public DataEntry(XYZ PoZYX, XYZ Accelerometer, XYZ Gyroscope)
        {
			this.PoZYX = PoZYX;
			INS_Accelerometer = Accelerometer;
			INS_Gyroscope = Gyroscope;
			EntryNum = entryNum_Incrementer++;
        }

        
    }
}
