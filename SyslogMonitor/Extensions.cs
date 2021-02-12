using SyslogMonitor.Webserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public static class Extensions
    {
        public static async void FireAndForget(this Task task, bool returnToCallingContext, Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
        }

        public static string GetFileTypeValue(this LocalfileHelper.FileType attribute)
        {
            Type currentType = attribute.GetType();

            var customAttributes = currentType.GetMember(attribute.ToString()).FirstOrDefault().GetCustomAttributes(false);

            return ((FileTypeAttribute)customAttributes
                .FirstOrDefault(n => n.GetType() == typeof(FileTypeAttribute)))
                .GetFileType();
        }
    }
}
