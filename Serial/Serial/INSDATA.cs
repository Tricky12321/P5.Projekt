using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    class INSDATA
    {
        XYZ XYZacc;
        XYZ XYZgyro;

        public INSDATA() { }
        public INSDATA(XYZ acc, XYZ gyro)
        {
            XYZacc = acc;
            XYZgyro = gyro;
        }
    }
}
