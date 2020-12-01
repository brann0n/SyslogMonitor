using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.Webserver
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FileTypeAttribute : Attribute
    {
        private string type { get; set; }
        public FileTypeAttribute(string type)
        {
            this.type = type;
        }

        public string GetFileType()
        {
            if (string.IsNullOrEmpty(type)) throw new Exception("No file type defined.");
            return type;
        }
    }
}
