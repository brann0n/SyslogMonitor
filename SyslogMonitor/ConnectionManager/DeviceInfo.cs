using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.ConnectionManager
{
    public class DeviceInfo
    {
        public string DeviceMac { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIp { get; set; }
        public bool DeviceConnected { get; set; }

        public override string ToString()
        {
            return $"Device {DeviceMac}: ({DeviceIp}) Connected = {DeviceConnected}";
        }
    }
}
