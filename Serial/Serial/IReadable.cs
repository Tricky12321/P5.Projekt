using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    interface IReadable <T>
    {
        T Read();
    }
}
