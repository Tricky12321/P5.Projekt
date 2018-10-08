using System;
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
            Count = Enum.GetValues(typeof(PatternEnum)).Cast<int>().Max();
        }

        public Pattern(List<int> outPuts)
        {
            Value = (PatternEnum)outPuts.IndexOf(1);
            Count = Enum.GetValues(typeof(PatternEnum)).Cast<int>().Max();
        }

        public List<int> GetArray()
        {
            int[] outList = new int[Count];
            for (int i = 0; i < Count; i++)
            {
                if ((int)Value == i)
                {
                    outList[i] = 1;
                }
                else
                {
                    outList[i] = 0;
                }
            }
            return outList.ToList();
        }
    }
}
