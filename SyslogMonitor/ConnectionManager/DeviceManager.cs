using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.ConnectionManager
{
    public class DeviceManager
    {
        private List<DeviceInfo> Devices;
        public DeviceManager()
        {
            Devices = new List<DeviceInfo>();
        }

        public void AddDevices(List<DeviceInfo> deviceInfos)
        {
            foreach(DeviceInfo device in deviceInfos)
            {
                DeviceInfo fndDevice = GetDeviceInfo(device.DeviceMac);
                if(fndDevice == null)
                {
                    Devices.Add(new DeviceInfo { DeviceMac = device.DeviceMac.ToLower(), DeviceIp = device.DeviceIp, DeviceConnected = false });
                }
                else
                {
                    fndDevice.DeviceIp = device.DeviceIp;
                }
            }          
        }

        public void UpdateDeviceStatus(string mac, bool connected)
        {
            var device = GetDeviceInfo(mac);
            if(device == null)
            {
                Devices.Add(new DeviceInfo {DeviceMac = mac.ToLower(), DeviceConnected = connected });
            }
            else
            {
                device.DeviceConnected = connected;
                Console.WriteLine(device);
            }
        }

        public DeviceInfo GetDeviceInfo(string mac)
        {
            return Devices.FirstOrDefault(n => n.DeviceMac == mac.ToLower());
        }

        public void PrintAllDevices()
        {
            foreach (DeviceInfo d in Devices) Console.WriteLine(d);          
        }
    }
}
