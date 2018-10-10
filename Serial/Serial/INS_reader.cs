using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    class INS_reader : IReadable<INSDATA>
    {
        public INSDATA Read()
        {
            return new INSDATA();
        }
    }
}
