using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SyslogMonitor.Webserver
{
    public class LocalfileHelper
    {
        private static string WebserverPath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "Webserver\\www";
                else
                    return "Webserver/www";
            }
        }

        public enum FileType
        {
            [FileType(".html")]
            Html,
            [FileType(".css")]
            Css,
            [FileType(".js")]
            Js
        }

        public static string GetFileContent(string fileName, FileType filetype)
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string combinedPath = Path.Combine(path, WebserverPath);
            if (Directory.Exists(combinedPath))
            {
                string file = Path.Combine(combinedPath, fileName + filetype.GetFileTypeValue());
                if (File.Exists(file))
                {
                    return File.ReadAllText(file);
                }
            }
            else
            {
                Directory.CreateDirectory(combinedPath);
                BConsole.WriteLine($"Did not find www folder, created here: {combinedPath}", ConsoleColor.Red);
            }
            throw new FileNotFoundException($"File {fileName} not found");
        }
    }
}
