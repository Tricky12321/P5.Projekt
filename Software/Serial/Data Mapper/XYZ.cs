﻿using System; using System.Collections.Generic;  namespace Serial {     public class XYZ     {         public double X, Y, Z; 		public double TimeOfData = 0;
		public double FixedTimeOfData = 0;         public XYZ() { }  		public XYZ(double x, double y, double z)         {             X = x;             Y = y;             Z = z;         }  		public XYZ(double x, double y, double z, double Tid)         {             X = x;             Y = y;             Z = z; 			TimeOfData = Tid;         }  		public XYZ(double x, double y, double z, double Tid, double FixedTime)         {             X = x;             Y = y;             Z = z;             TimeOfData = Tid; 			FixedTimeOfData = FixedTime;         }

		public override string ToString()
		{
			return $"X: {X}\nY: {Y}\nZ: {Z}";
		}          public List<double> ToList()         {             return new List<double>() { X, Y, Z };         }          public List<double> GetNormalizedList(double maxValue = 10000.0)         {             double x, y, z;             x = X / maxValue;             y = Y / maxValue;             z = Z / maxValue;             return new List<double>() { x, y, z };         }
	} }