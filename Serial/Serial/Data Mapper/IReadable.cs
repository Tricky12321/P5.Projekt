using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    interface IReadable <T>
    {
        #region METHODS
        T Read();
        #endregion
    }
}
