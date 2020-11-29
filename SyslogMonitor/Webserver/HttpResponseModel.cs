using System.Net;

namespace SyslogMonitor.Webserver
{
    public sealed class HttpResponseModel
    {
        public string ContentType { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public CookieCollection Cookies { get; set; }
        public int StatusCode { get; set; }
        public string OutputStream { get; set; }
        public string StatusDescription { get; set; }

        public HttpResponseModel()
        {
            Headers = new WebHeaderCollection();
            Cookies = new CookieCollection();
        }
    }
}
