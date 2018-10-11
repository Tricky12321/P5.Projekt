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
        private bool _dataReady;

        private INSDATA INSData;
        private XYZ PozyxData;
        public DataMapper(PozyxReader Pozyx, INSReader INS)
        {
            _pozyx = Pozyx;
            _INS = INS;
            _dataReady = false;
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
            Console.WriteLine("Reading Data.");
        }

        public void StopReading()
        {
            _isReading = false;
            Console.WriteLine("Stopped reading data.");
        }

        public List<Tuple<XYZ, INSDATA>> ReadToList(int amount)
        {
            List<Tuple<XYZ, INSDATA>> DataList = new List<Tuple<XYZ, INSDATA>>();
            int i = 0;
            while (i < amount)
            {
                if(_dataReady && PozyxData != null && INSData != null)
                {
                    DataList.Add(Tuple.Create<XYZ, INSDATA>(PozyxData, INSData));
                    i++;
                    _dataReady = false;
                }
            }
            return DataList;
        }

        private void ReadPozyx()
        {
            while (_isReading)
            {
                PozyxData = _pozyx.Read();
                _dataReady = true;
                
            }
            Console.WriteLine("Pozyx thread has been terminated.");
        }

        private void ReadINS()
        {
            while (_isReading)
            {
                INSData = _INS.Read();
                _dataReady = true;
            }
            Console.WriteLine("INS has been terminated.");
        }
    }
}
