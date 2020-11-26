using SyslogMonitor.SyslogService;
using System;

namespace SyslogMonitor
{
    class Program
    {
        static void Main(string[] args) => new Monitor().Start().GetAwaiter().GetResult();
    }
}
