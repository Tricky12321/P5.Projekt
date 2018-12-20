using System;
namespace Serial.DataMapper
{
    public class DataEntry
    {
		static int entryNum_Incrementer = 1;
		public XYZ PoZYX;
		public XYZ INS_Accelerometer;
		public XYZ INS_Gyroscope;
		public int EntryNum;
        public bool Used;
		public double INS_Angle;

        public DataEntry(XYZ PoZYX, XYZ Accelerometer, XYZ Gyroscope, double Angle)
        {
			this.PoZYX = PoZYX;
			INS_Accelerometer = Accelerometer;
			INS_Gyroscope = Gyroscope;
			INS_Angle = Angle;
			EntryNum = entryNum_Incrementer++;
        }

        
    }
}
