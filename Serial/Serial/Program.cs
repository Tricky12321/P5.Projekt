using System;
using Serial.Menu;
using Serial.Utility;
using Serial.DynamicCalibrationName;
using System.Linq;
using System.IO;
using System.Threading;
using System.Globalization;
using Serial.DataMapper;

namespace Serial
{
    class MainClass
    {
        public static void Main()
        {
            PozyxController pozyxController = new PozyxController();
            pozyxController.CalculateDistance();

                  Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
			      MainMenu.ShowMenu();
        }
    }
}
