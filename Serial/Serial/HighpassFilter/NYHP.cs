using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial.Highpass
{
    class NYHP
    {
        public NYHP()
        {
            
        }
        
        public double filterloop(double value)
        {
            int NZEROS = 4;
            int NPOLES = 4;
            float GAIN = 5.387057197e+01f;
            double output;
            float[] xv = new float[NZEROS+1], yv = new float[NPOLES+1];

            for (; ; )
            {
                xv[0] = xv[1]; xv[1] = xv[2]; xv[2] = xv[3]; xv[3] = xv[4];
                xv[4] = (float)value / GAIN;
                yv[0] = yv[1]; yv[1] = yv[2]; yv[2] = yv[3]; yv[3] = yv[4];
                yv[4] = (xv[0] + xv[4]) - 4 * (xv[1] + xv[3]) + 6 * xv[2]
                             + (-0.0761970646f * yv[0]) + (-0.4844033683f * yv[1])
                             + (-1.2756133250f * yv[2]) + (-1.5703988512f * yv[3]);
                output = yv[4];
            }


        }
    }
}
