using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public class SyslogFilters
    {
        public const string REGEX_MAC = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
        public SyslogFilters()
        {

        }

        public static (bool,string) CheckConnectionState(string data)
        {
            var success = Regex.Match(data, @"([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})\s\w{8}\s\w{6}\s(disconnected|connected)");
            if (success.Success)
            {
                return (true, success.Value);
            }
            else return (false, null);
        }

    }
}
