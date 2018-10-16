using System; using System.Collections.Generic;  namespace Serial {     public class XYZ     {         public double X, Y, Z; 		public UInt32 TimeOfData = 0;             public XYZ() { }  		public XYZ(double x, double y, double z)         {             X = x;             Y = y;             Z = z;         }  		public XYZ(double x, double y, double z, UInt32 Tid)         {             X = x;             Y = y;             Z = z; 			TimeOfData = Tid;         }

		public override string ToString()
		{
			return $"X: {X}\nY: {Y}\nZ: {Z}";
		}          public List<double> ToList()         {             return new List<double>() { X, Y, Z };         }
	} }