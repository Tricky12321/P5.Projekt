using System;
namespace Serial.Menu
{
	public static class MenuController
    {
		public static bool Confirm(string Message, bool? Default = null)
        {
            string YN = "[y/n]";
            if (Default == true)
            {
                YN = "[Y/n]";
            }
            else if (Default == false)
            {
                YN = "[y/N]";
            }

            Console.Write($"{Message} {YN}:");
            bool ValidInput = false;
            do
            {
                string Output = Console.ReadLine();
                switch (Output.ToLower())
                {
                    case "y":
                        ValidInput = true;
                        return true;
                    case "n":
                        ValidInput = true;

                        return false;
                    case "":
                        if (Default != null)
                        {
                            ValidInput = true;
                            return Default.GetValueOrDefault();
                        }
                        break;
                }
            } while (!ValidInput);
            throw new Exception("Something went wrong in Confirm"); // It should never get here
        }

		public static void DefaultHelp() {
			Console.WriteLine("-----------------------------------");
            Console.WriteLine("help - shows this page");
            Console.WriteLine("clear - clears the console");
            Console.WriteLine("-----------------------------------");
		}


		public static bool DefaultCommands(string[] Input) {
			switch (Input[0])
			{

                case "exit":
					Environment.Exit(0);
                    return true;
                case "clear":
                    Console.Clear(); ;
					return false;
				default:
					Console.WriteLine("Unknown command!");
					return false;

			}
		}
    }
}
