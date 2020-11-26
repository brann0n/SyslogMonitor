using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public class FilterObject
    {
        public int ID { get; set; }
        public string FilterName { get; set; }
        public string FilterExpression { get; set; }
        public bool Enabled { get; set; }
    }
}
