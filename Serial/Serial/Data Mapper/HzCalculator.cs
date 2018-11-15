using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace Serial
{
    public class HzCalculator
    {
		const int _hz_log_count = 100;
        readonly ConcurrentQueue<double> _hz_rate_log = new ConcurrentQueue<double>();
        public double HZ_rate
        {
            get
            {
				try
				{
					return Math.Round(_hz_rate_log.Average(), 0);
                }
                catch (InvalidOperationException)
                {
					return 0;
                }
            }
            set
            {
                _hz_rate_log.Enqueue(1000 / value);
				if (_hz_rate_log.Count() >= _hz_log_count) {
					_hz_rate_log.TryDequeue(out double none);
                }
            }
        }
        
    }
}
