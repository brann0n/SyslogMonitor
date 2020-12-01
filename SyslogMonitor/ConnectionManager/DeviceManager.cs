using SyslogMonitor.Database;
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
        private readonly DatabaseContext DB;

        public DeviceManager()
        {
            Devices = new List<DeviceInfo>();
            DB = new DatabaseContext();
        }

        public async Task Init()
        {
            AddDevices(await DB.GetDevicesAsync());
        }

        public List<DeviceInfo> GetDevices() => Devices;

        public void AddDevices(List<DeviceInfo> deviceInfos)
        {
            foreach(DeviceInfo device in deviceInfos)
            {
                string mac = device.DeviceMac.Replace("-", ":").Replace(" ", "").ToLower();
                string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));

                DeviceInfo fndDevice = GetDeviceById(deviceId);
                if(fndDevice == null)
                {
                    fndDevice = new DeviceInfo { DeviceName = device.DeviceName, DeviceId = deviceId, DeviceMac = mac, DeviceIp = device.DeviceIp, DeviceConnected = device.DeviceConnected, LastUpdate = DateTime.Now};
                    Devices.Add(fndDevice);
                    DB.AddDeviceAsync(fndDevice).Wait();
                    BConsole.WriteLine($"Added device {mac} {fndDevice.DeviceIp}");
                }
                else
                {
                    fndDevice.DeviceIp = device.DeviceIp ?? fndDevice.DeviceIp;
                    fndDevice.DeviceName = device.DeviceName ?? fndDevice.DeviceName;
                    DB.UpdateDeviceAsync(fndDevice).Wait();
                    BConsole.WriteLine($"Updated device {mac} {fndDevice.DeviceIp}");
                }
            }          
        }

        public void UpdateDeviceStatus(string mac, bool connected, bool display)
        {
            mac = mac.Replace("-", ":").ToLower();
            string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));
            DeviceInfo device = GetDeviceInfo(mac);
            if(device == null)
            {
                device = new DeviceInfo { DeviceId = deviceId, DeviceMac = mac, DeviceConnected = connected, LastUpdate = DateTime.Now};
                Devices.Add(device);
                DB.AddDeviceAsync(device).Wait();
                BConsole.WriteLine($"Added device {mac} {device.DeviceIp}");
            }
            else
            {
                device.DeviceConnected = connected;
                device.LastUpdate = DateTime.Now;
                DB.UpdateDeviceAsync(device).Wait();
            }        
            if(display)
            BConsole.WriteLine(device.ToString());
        }

        public DeviceInfo GetDeviceInfo(string mac)
        {
            return Devices.FirstOrDefault(n => n.DeviceMac == mac.ToLower());
        }     

        public DeviceInfo GetDeviceById(string id)
        {
            return Devices.FirstOrDefault(n => n.DeviceId == id);
        }

        public void PrintAllDevices()
        {
            foreach (DeviceInfo d in Devices) BConsole.WriteLine(d.ToString());          
        }
    }
}
