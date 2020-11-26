using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public class DataReceivedObject
    {
        public string IPAddress { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }

    }
}
