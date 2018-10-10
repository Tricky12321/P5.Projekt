using System;
namespace Serial
{
    public class Utilities
    {
        public Utilities()
        {
        }

		public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 6) || (p == 128) || Environment.OSVersion.ToString().ToLower().Contains("linux");
            }
        }

        public static bool IsMacOS
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) && Environment.OSVersion.ToString().ToLower().Contains("unix");
            }
        }

        public static bool IsWindows
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 2) || Environment.OSVersion.ToString().ToLower().Contains("windows");
            }
        }
    }
}
