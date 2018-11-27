using System;
using Serial.Menu;
using System.Threading;
using System.Globalization;

namespace Serial
{
    class MainClass
    {
        public static void Main()
        {
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
			MainMenu.ShowMenu();
        }
    }
}
