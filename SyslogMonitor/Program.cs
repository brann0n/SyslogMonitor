using SyslogMonitor.SyslogService;
using System;

namespace SyslogMonitor
{
    class Program
    {
        static void Main() => new Monitor().Start().GetAwaiter().GetResult();
    }
}
