using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Serial.CSV;
using Serial.DynamicCalibrationName;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Concurrent;
using Serial.Menu;
namespace Serial
{
    class MainClass
    {
        public static void Main()
        {
			MainMenu.ShowMenu();
        }
    }
}
