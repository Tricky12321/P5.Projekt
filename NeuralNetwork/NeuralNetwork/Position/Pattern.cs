﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetwork
{
    public enum PatternEnum
    {
        forward5m, forward05m
    }

    public class Pattern
    {
        public int Count;
        public PatternEnum Value;

        public Pattern(PatternEnum pattern)
        {
            Value = pattern;
            Count = Enum.GetValues(typeof(PatternEnum)).Cast<int>().Max() + 1;
        }

        public Pattern(List<int> outPuts)
        {
            Value = (PatternEnum)outPuts.IndexOf(1);
            Count = Enum.GetValues(typeof(PatternEnum)).Cast<int>().Max() + 1;
        }

        public List<double> GetArray()
        {
            double[] outList = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                if ((int)Value == i)
                {
                    outList[i] = 1.0;
                }
                else
                {
                    outList[i] = 0.0;
                }
            }
            return outList.ToList();
        }
    }
}
