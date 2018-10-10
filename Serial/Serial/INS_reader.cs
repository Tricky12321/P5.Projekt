using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Serial
{
    class INS_reader : IReadable<INSDATA>
    {
        private SerialPort _serialPort;

        public INS_reader()
        {

        }

        public INSDATA Read()
        {
            return new INSDATA();
        }
    }
}
