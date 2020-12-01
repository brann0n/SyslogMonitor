using SyslogMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.ConnectionManager
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string DeviceMac { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIp { get; set; }
        public bool DeviceConnected { get; set; }
        public DateTime LastUpdate { get; set; }
        public override string ToString()
        {
            return $"Device {DeviceMac}: ({DeviceIp}) Connected = {DeviceConnected}, Since {LastUpdate:dd-MM-yyyy HH:mm:ss}";
        }

        public static implicit operator DeviceInfo(DbDevice device)
        {
            return new DeviceInfo
            {
                DeviceConnected = device.Connected,
                DeviceId = $"{device.Id}",
                DeviceIp = device.DeviceIp,
                DeviceMac = device.DeviceMac,
                DeviceName = device.Name,
                LastUpdate = device.LastUpdated
            };
        }
    }
}
