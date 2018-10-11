using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial 
{
    class PozyxReader : IReadable<XYZ>
    {
        private string _description = "POZYX";
        private SerialPort _serialPort;
        
        public PozyxReader()
        {
            Console.WriteLine($"Getting {_description} Serial Port");
            _serialPort = SerialReader.GetSerialPort(ArduinoTypes.POZYX);
            Console.WriteLine($"Opening {_description} Serial Port");
            Console.WriteLine($"{_description} Serial Port Opened");
        }

        public string Reader()
        {
            return _serialPort.ReadLine();
        }

        public XYZ Read()
        {
            string XYZstring = _serialPort.ReadLine();
            string[] XYZstringSplit = XYZstring.Split(',');

            //Seriøst din taber lær at programmere noob
            string XString = XYZstringSplit[1].Split(' ')[1];
            string YString = XYZstringSplit[2].Split(' ')[1]; 
            string ZString = XYZstringSplit[3].Split(' ')[1];

            int.Parse(XString);

            return new XYZ(int.Parse(XString), int.Parse(YString), int.Parse(ZString));
        }
    }
}