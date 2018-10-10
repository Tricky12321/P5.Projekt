using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Serial
{
    class DataMapper
    {
        private PozyxReader _pozyx;
        private INSReader _INS;

        private bool _isReading;

        private INSDATA INSData;
        private XYZ PozyxData;
        public DataMapper(PozyxReader Pozyx, INSReader INS)
        {
            _pozyx = Pozyx;
            _INS = INS;
        }

        public void StartReading()
        {
            _isReading = true;
            Console.WriteLine("Pozyx thread initialized.");
            Thread PozyxReaderThread = new Thread(new ThreadStart(ReadPozyx));
            Console.WriteLine("INS thread initialized.");
            Thread INSReaderThread = new Thread(new ThreadStart(ReadINS));
            Console.WriteLine("Starting Pozyx & INS threads.");
            //Start threads.
            PozyxReaderThread.Start();
            INSReaderThread.Start();
        }

        public void StopReading()
        {
            _isReading = false;
        }

        private void ReadPozyx()
        {
            while (_isReading)
            {
                PozyxData = _pozyx.Read();
            }
            Console.WriteLine("Pozyx thread has been terminated.");
        }

        private void ReadINS()
        {
            while (_isReading)
            {
                INSData = _INS.Read();
            }
            Console.WriteLine("INS has been terminated.");
        }
    }
}
