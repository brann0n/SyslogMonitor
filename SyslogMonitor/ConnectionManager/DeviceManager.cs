using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.ConnectionManager
{
    public class DeviceManager
    {
        private readonly List<DeviceInfo> Devices;

        public DeviceManager()
        {
            Devices = new List<DeviceInfo>();
        }
        public List<DeviceInfo> GetDevices() => Devices;

        public void AddDevices(List<DeviceInfo> deviceInfos)
        {
            foreach(DeviceInfo device in deviceInfos)
            {
                string mac = device.DeviceMac.Replace("-", ":").Replace(" ", "").ToLower();
                string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));
                Console.WriteLine("ByteData: "+ deviceId);

                DeviceInfo fndDevice = GetDeviceInfo(mac);
                if(fndDevice == null)
                {                   
                    Devices.Add(new DeviceInfo {DeviceId = deviceId, DeviceMac = mac, DeviceIp = device.DeviceIp, DeviceConnected = false });
                    BConsole.WriteLine($"Added device {mac} {device.DeviceIp}");
                }
                else
                {
                    fndDevice.DeviceIp = device.DeviceIp;
                }
            }          
        }

        public void UpdateDeviceStatus(string mac, bool connected)
        {
            mac = mac.Replace("-", ":").ToLower();
            string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));
            Console.WriteLine("ByteData: " + deviceId);
            DeviceInfo device = GetDeviceInfo(mac);
            if(device == null)
            {
                device = new DeviceInfo { DeviceId = deviceId, DeviceMac = mac, DeviceConnected = connected };
                Devices.Add(device);
                BConsole.WriteLine($"Added device {mac} {device.DeviceIp}");
            }
            else
            {
                device.DeviceConnected = connected;               
            }
            device.LastUpdate = DateTime.Now;
            BConsole.WriteLine(device.ToString());
        }

        public DeviceInfo GetDeviceInfo(string mac)
        {
            return Devices.FirstOrDefault(n => n.DeviceMac == mac.ToLower());
        }     

        public void PrintAllDevices()
        {
            foreach (DeviceInfo d in Devices) BConsole.WriteLine(d.ToString());          
        }
    }
}
