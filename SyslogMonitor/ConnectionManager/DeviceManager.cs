using SyslogMonitor.Database;
using SyslogMonitor.SyslogService;
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
            AddDevicesToCache(await DB.GetDevicesAsync());
        }

        public List<DeviceInfo> GetDevices() => Devices;

        /// <summary>
        /// Adds devices found in the ARP scan.
        /// </summary>
        /// <param name="deviceInfos"></param>
        public void AddDevices(List<DeviceInfo> deviceInfos)
        {
            // get a list of all devices not present in this list, and update those to cable disconnected.
            foreach (DeviceInfo device in deviceInfos)
            {
                string mac = device.DeviceMac.Replace("-", ":").Replace(" ", "").ToLower();
                string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));

                DeviceInfo fndDevice = GetDeviceById(deviceId);
                if (fndDevice == null)
                {
                    fndDevice = new DeviceInfo { DeviceName = device.DeviceName, DeviceId = deviceId, DeviceMac = mac, DeviceIp = device.DeviceIp, DeviceConnected = device.DeviceConnected, LastUpdate = device.LastUpdate };
                    Devices.Add(fndDevice);
                    DB.AddDeviceAsync(fndDevice).Wait();
                    BConsole.WriteLine($"Added device {mac} {fndDevice.DeviceIp}");
                }
                else
                {
                    fndDevice.DeviceIp = device.DeviceIp ?? fndDevice.DeviceIp;
                    fndDevice.DeviceName = device.DeviceName ?? fndDevice.DeviceName;

                    if (string.IsNullOrEmpty(fndDevice.Host))
                    {
                        fndDevice.DeviceConnected = true;
                        fndDevice.Host = APMapping.CABLE;
                    }
                    else if (fndDevice.Host == APMapping.CABLE || fndDevice.Host == APMapping.CABLE_DISCONNECTED)
                    {
                        fndDevice.DeviceConnected = true;
                        fndDevice.Host = APMapping.CABLE;
                    }

                    DB.UpdateDeviceAsync(fndDevice).Wait();
                    BConsole.WriteLine($"Updated device {mac} {fndDevice.DeviceIp}");
                }
            }
            var CurrentConnectedDevices = new HashSet<string>(deviceInfos.Select(n => n.DeviceMac));
            var CurrentNotConnectedDevices = Devices.Where(n => (!CurrentConnectedDevices.Contains(n.DeviceMac) && (n.Host == APMapping.CABLE || n.Host == APMapping.CABLE_DISCONNECTED))).ToList();
            foreach(DeviceInfo device in CurrentNotConnectedDevices)
            {               
                string mac = device.DeviceMac.Replace("-", ":").Replace(" ", "").ToLower();
                string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));
                DeviceInfo fndDevice = GetDeviceById(deviceId);
                fndDevice.Host = APMapping.CABLE_DISCONNECTED;
                fndDevice.DeviceConnected = false;
                fndDevice.LastUpdate = DateTime.Now;
                DB.UpdateDeviceAsync(fndDevice).Wait();
            }
        }

        private void AddDevicesToCache(List<DeviceInfo> deviceInfos)
        {
            foreach (DeviceInfo device in deviceInfos)
            {
                Devices.Add(device);
                BConsole.WriteLine($"Added device {device.DeviceMac} {device.DeviceIp}");
            }
        }

        public void UpdateDeviceStatus(string mac, bool connected, bool display, string sourceHost)
        {
            mac = mac.Replace("-", ":").ToLower();
            string deviceId = Convert.ToBase64String(Encoding.Default.GetBytes(mac));
            string deviceHost;
            if (sourceHost == APMapping.WIFI1)
            {
                deviceHost = APMapping.WIFI_DOWNSTAIRS;
            }
            else if (sourceHost == APMapping.WIFI2)
            {
                deviceHost = APMapping.WIFI_UPSTAIRS;
            }
            else
            {
                deviceHost = APMapping.CABLE;
            }

            DeviceInfo device = GetDeviceInfo(mac);
            if (device == null)
            {
                device = new DeviceInfo { DeviceId = deviceId, DeviceMac = mac, DeviceConnected = connected, LastUpdate = DateTime.Now, Host = deviceHost };
                Devices.Add(device);
                DB.AddDeviceAsync(device).Wait();
                BConsole.WriteLine($"Added device {mac} {device.DeviceIp}");
            }
            else
            {
                device.DeviceConnected = connected;
                device.LastUpdate = DateTime.Now;
                device.Host = deviceHost;

                DB.UpdateDeviceAsync(device).Wait();
            }
            if (display)
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
