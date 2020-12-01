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
using SyslogMonitor.Database;

namespace SyslogMonitor
{
    public partial class Monitor
    {
        private readonly ConfigReader Config;
        private readonly SyslogListener Listener;
        private readonly DeviceManager Manager;
        private readonly DatabaseContext DB;
        private bool StopCalled = false;
        private Server httpServer;
        public Monitor()
        {
            Config = ConfigReader.GetConfig();
            Manager = new DeviceManager();
            Listener = new SyslogListener(Config.GetIp(), Config.GetPort());
            Listener.ConnectionChanged += L_ConnectionChanged;
            DB = new DatabaseContext();

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
                {
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
                else
                {
                    return Page("Index");
                }

            }
            catch (FileNotFoundException fnf)
            {
                return new HttpResponseModel { StatusCode = 404, StatusDescription = fnf.Message };
            }
            catch (Exception ex)
            {
                return new HttpResponseModel { StatusCode = 500, StatusDescription = ex.Message };
            }
        }

        public async Task Start()
        {
            BConsole.WriteLine("NetworkMonitor v1.0");
            await Manager.Init();

            new Thread(delegate ()
            {
                int minutes = 10;
                int interval = 1000;
                int IntervalMax = minutes * 1000 * 60;
                int timer = IntervalMax;
                while (!StopCalled)
                {
                    if (timer % interval != 0 || timer > IntervalMax) break;
                    if (timer < IntervalMax)
                    {
                        Thread.Sleep(interval);
                        timer += interval;
                    }
                    else
                    {
                        timer = 0;
                        var devices = NetworkScanner.GetDevices();
                        Manager.AddDevices(devices);
                    }
                }
            }).Start();

            await Listener.Start();
        }

        public void Stop()
        {
            StopCalled = true;
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
