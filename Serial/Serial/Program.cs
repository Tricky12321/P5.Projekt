using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
namespace Serial
{
    class MainClass
    {
        public static void Main()
		{
            PozyxReader Pozyx = new PozyxReader();
            INSReader INS = new INSReader();

            DataMapper Mapper = new DataMapper(Pozyx, INS);

            Mapper.StartReading();
            List<Tuple<XYZ, INSDATA>> MappedData = Mapper.ReadToList(100);
            Mapper.StopReading();

            foreach (var item in MappedData)
            {
                Console.WriteLine("POZYX" + "\n" + item.Item1.ToString());
                Console.WriteLine("INS" + "\n" + item.Item2.ToString());
            }
            Console.WriteLine(MappedData.Count);
            Console.Read();
		}
    }
}