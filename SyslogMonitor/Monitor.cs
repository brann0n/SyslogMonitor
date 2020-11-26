using SyslogMonitor.SyslogService;
using SyslogMonitor.ConnectionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace SyslogMonitor
{
    public class Monitor
    {
        private readonly ConfigReader Config;
        private readonly SyslogListener Listener;
        private readonly DeviceManager Manager;
        private NetworkScanner Scanner;
        public Monitor()
        {
            Config = ConfigReader.GetConfig();
            Manager = new DeviceManager();
            Listener = new SyslogListener(Config.GetIp(), Config.GetPort());
            Listener.ConnectionChanged += L_ConnectionChanged;
            Scanner = new NetworkScanner();


            //todo: also add a http listener, to have live updates on connected devices.
        }

        public async Task Start()
        {
            Console.WriteLine("NetworkMonitor v1.0");

            new Thread(delegate () 
            { 
                var devices = Scanner.GetDevices();
                Manager.AddDevices(devices);
            }).Start();

            await Listener.Start();
        }

        public void Stop()
        {
            Listener.Stop();
        }


        private void L_ConnectionChanged(string connectionState)
        {
            string[] split = connectionState.Split(' ');
            string mac = split[0];
            switch (split.Last())
            {
                case "connected":
                    Manager.UpdateDeviceStatus(mac, true);
                    break;
                case "disconnected":
                    Manager.UpdateDeviceStatus(mac, false);
                    break;
                default:
                    throw new Exception("Did not understand connection state.");
            }

            //Manager.PrintAllDevices();
        }
    }
}
