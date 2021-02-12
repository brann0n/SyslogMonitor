using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.SyslogService
{
    public class ConfigReader
    {
        private const string CONFIG_PATH = "Config/config.json";

        [JsonProperty("ip")]
        private string Ip { get; set; }
        
        [JsonProperty("host")]
        private string Host { get; set; }

        [JsonProperty("port")]
        private int Port { get; set; }

        [JsonProperty("display")]
        private bool Display { get; set; }

        public static ConfigReader GetConfig()
        {           
            if(File.Exists(CONFIG_PATH)){
                return JObject.Parse(File.ReadAllText(CONFIG_PATH)).ToObject<ConfigReader>();
            }
            else
            {
                var config = new ConfigReader { Ip = "127.0.0.1", Port = 514, Display = true, Host = "127.0.0.1" };
                Console.WriteLine("Config file not found, created a new one!");
                File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(config));
                return config;
            }            
        }

        public string GetIp() => Ip;

        public int GetPort() => Port;

        public bool GetDisplay() => Display;

        public string GetHost() {
            if (string.IsNullOrEmpty(Host))
            {
                return Ip;
            }
            else
            {
                return Host;
            }
        }
    }
}
