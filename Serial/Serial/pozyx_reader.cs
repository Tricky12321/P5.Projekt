using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial 
{
    class PozyxReader : IReadable<XYZ>
    {
        private static SerialPort _serialPort;
        public PozyxReader()
        {

        }

        public XYZ Read()
        {
            return new XYZ();
        }
    }
}