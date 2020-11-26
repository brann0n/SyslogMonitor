using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyslogMonitor.ConnectionManager
{
    class NetworkScanner
    {
        public NetworkScanner()
        {

        }

        public List<DeviceInfo> GetDevices()
        {
            string devices = GetArpList();
            string[] lines = devices.Split("\r\n");
            List<DeviceInfo> returnList = new List<DeviceInfo>();

            foreach (string line in lines)
            {
                //check if the line has an ip & mac address
                bool found = Regex.IsMatch(line, @"(([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})|((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))");
                if (found)
                {
                    string mac = Regex.Match(line, @"([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})").Value;
                    string ip = Regex.Match(line, @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)").Value;

                    if (!string.IsNullOrEmpty(mac) && !string.IsNullOrEmpty(ip))
                        returnList.Add(new DeviceInfo { DeviceIp = ip, DeviceMac = mac });
                }
            }
            return returnList;
        }

        private string GetArpList()
        {
            var escapedArgs = "arp -a";

            ProcessStartInfo info;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                info = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                info = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {escapedArgs}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            }
            else throw new Exception("Unsupported platform!");

            var process = new Process()
            {
                StartInfo = info
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
