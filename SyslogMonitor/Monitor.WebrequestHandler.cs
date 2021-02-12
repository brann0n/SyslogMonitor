using Newtonsoft.Json;
using SyslogMonitor.ConnectionManager;
using SyslogMonitor.Webserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public partial class Monitor
    {
        public HttpResponseModel Parse(string[] urlSegments, string method, string api_key, string data)
        {
            return (urlSegments[0].ToLower()) switch
            {
                "devices" => Devices(),
                "api" => ParseApiSegments(urlSegments, method, api_key, data),
                "styles" => Css(urlSegments[1].ToLower()),
                "scripts" => JS(urlSegments[1].ToLower()),
                "ip" => GetIpAddress(data),
                "pages" => Page(urlSegments[1].ToLower()),
                _ => new HttpResponseModel
                {
                    StatusCode = 404,
                    StatusDescription = "Undefined url path"
                },
            };
        }

        private HttpResponseModel Devices()
        {
            return Page("devices");
        }

        public HttpResponseModel ParseApiSegments(string[] urlSegments, string method, string api_key, string data)
        {
            //id the current request
            if (api_key != "dit-is-een-api-key") //cors allows this to only be used from the same domain :))))
                return new HttpResponseModel { StatusCode = 401, StatusDescription = "Invalid apikey" };


            return (urlSegments[1].ToLower()) switch
            {
                "devices" => new HttpResponseModel
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    OutputStream = JsonConvert.SerializeObject(Manager.GetDevices())
                },
                "device" => ApiDevice(urlSegments, method, data),
                _ => new HttpResponseModel
                {
                    StatusCode = 404
                },
            };
        }
        internal HttpResponseModel GetIpAddress(string ip)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<!DOCTYPE html><html><head><title>Get IpAddress</title><link href=\"/styles/index\" rel=\"stylesheet\"/></head><body>");
            builder.Append("<div>");
            builder.Append($"<h3>This is your ip address: {ip}</h3>");
            builder.Append("</div></body></html>");
            return new HttpResponseModel
            {
                StatusCode = 200,
                ContentType = "text/html",
                OutputStream = builder.ToString()
            };
        }
        internal HttpResponseModel ApiDevice(string[] urlSegments, string method, string data)
        {
            string deviceId = (urlSegments.Length >= 3) ? urlSegments[2] : "";
            if (string.IsNullOrEmpty(deviceId) && urlSegments[1].ToLower() != "device")
                return new HttpResponseModel { StatusCode = 500, StatusDescription = "Device id not provided" };

            switch (method)
            {
                case "GET":
                    return new HttpResponseModel
                    {
                        StatusCode = 200,
                        ContentType = "application/json",
                        OutputStream = JsonConvert.SerializeObject(Manager.GetDeviceById(deviceId))
                    };
                case "POST":
                    //do an update
                    DeviceInfo device = JsonConvert.DeserializeObject<DeviceInfo>(data);
                    if(device != null)
                    {
                        if(device.DeviceId == deviceId)
                        {
                            DeviceInfo currentDevice = Manager.GetDeviceById(deviceId);
                            currentDevice.DeviceName = device.DeviceName;
                            DB.UpdateDeviceAsync(currentDevice).Wait();
                            BConsole.WriteLine($"Updated device {deviceId} in DB!");
                            return new HttpResponseModel { StatusCode = 200, StatusDescription = "Updated device in DB" };
                        }
                    }
                    return new HttpResponseModel { StatusCode = 500, StatusDescription = "Data object incorrect" };
                default:
                    return new HttpResponseModel()
                    {
                        StatusCode = 405
                    };
            }
        }

        internal HttpResponseModel Page(string page)
        {
            return new HttpResponseModel
            {
                StatusCode = 200,
                ContentType = "text/html",
                OutputStream = LocalfileHelper.GetFileContent(page, LocalfileHelper.FileType.Html)
            };
        }

        internal HttpResponseModel Css(string style)
        {
            return new HttpResponseModel
            {
                StatusCode = 200,
                ContentType = "text/css",
                OutputStream = LocalfileHelper.GetFileContent(style, LocalfileHelper.FileType.Css)
            };
        }

        internal HttpResponseModel JS(string script)
        {
            return new HttpResponseModel
            {
                StatusCode = 200,
                ContentType = "text/javascript",
                OutputStream = LocalfileHelper.GetFileContent(script, LocalfileHelper.FileType.Js)
            };
        }
    }
}
