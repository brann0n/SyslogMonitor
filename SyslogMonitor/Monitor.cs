using SyslogMonitor.SyslogService;
using SyslogMonitor.ConnectionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using SyslogMonitor.Webserver;
using System.IO;

namespace SyslogMonitor
{
    public partial class Monitor
    {
        private readonly ConfigReader Config;
        private readonly SyslogListener Listener;
        private readonly DeviceManager Manager;
        private Server httpServer;
        public Monitor()
        {
            Config = ConfigReader.GetConfig();
            Manager = new DeviceManager();
            Listener = new SyslogListener(Config.GetIp(), Config.GetPort());
            Listener.ConnectionChanged += L_ConnectionChanged;
            
            new Thread(async delegate ()
            {
                httpServer = new Server(Config.GetIp(), "http", 7000);
                httpServer.OnRequest += HttpServer_OnRequest;
                await httpServer.Listen();
            }).Start();
            //todo: also add a http listener, to have live updates on connected devices.
        }

        private HttpResponseModel HttpServer_OnRequest(HttpRequestModel request)
        {
            try
            {
                string[] urlSegments = request.RawUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (urlSegments.Length != 0)
                    if (request.HttpMethod == "POST")
                    {
                        string bodyData;
                        //request.InputStream.Seek(0, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(request.InputStream))
                        {
                            bodyData = sr.ReadToEnd();
                        }
                        return Parse(urlSegments, request.HttpMethod, request.Headers["API_KEY"] ?? "", bodyData);
                    }
                    else
                    {
                        return Parse(urlSegments, request.HttpMethod, request.Headers["API_KEY"] ?? "", "");
                    }
            }
            catch (Exception ex)
            {
                return new HttpResponseModel { StatusCode = 500, StatusDescription = ex.Message };
            }
            return new HttpResponseModel
            {
                StatusCode = 200,
                ContentType = "text/html",
                OutputStream = "yuh"
            };
        }

        public async Task Start()
        {
            BConsole.WriteLine("NetworkMonitor v1.0");

            new Thread(delegate () 
            { 
                var devices = NetworkScanner.GetDevices();
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
