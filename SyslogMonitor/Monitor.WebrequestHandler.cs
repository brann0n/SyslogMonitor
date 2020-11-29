﻿using Newtonsoft.Json;
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
                _ => new HttpResponseModel
                {
                    StatusCode = 404
                },
            };
        }

        private HttpResponseModel Devices()
        {
            var deviceList = Manager.GetDevices();
            StringBuilder builder = new StringBuilder();
            builder.Append("<ul>");
            foreach(var device in deviceList)
            {
                builder.Append($"<li>{device}</li>");
            }
            builder.Append("</ul>");
            return new HttpResponseModel
            {
                StatusCode = 200,
                ContentType = "text/html",
                OutputStream = builder.ToString()
            };
        }

        public HttpResponseModel ParseApiSegments(string[] urlSegments, string method, string api_key, string data)
        {
            //id the current request
            if (api_key != "dit-is-een-api-key") //cors allows this to only be used from the same domain :))))
                return new HttpResponseModel { StatusCode = 401, StatusDescription = "Invalid apikey" };

            //id the target for this request
            string channelId = (urlSegments.Length >= 3) ? urlSegments[2] : "";
            if (string.IsNullOrEmpty(channelId) && urlSegments[1].ToLower() != "clients")
                return new HttpResponseModel { StatusCode = 500, StatusDescription = "No channelid provided" };

            return (urlSegments[1].ToLower()) switch
            {
                "clients" => new HttpResponseModel
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    OutputStream = JsonConvert.SerializeObject(Manager.GetDevices())
                },
                _ => new HttpResponseModel
                {
                    StatusCode = 404
                },
            };
        }
    }
}
