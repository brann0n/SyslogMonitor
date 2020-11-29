using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace SyslogMonitor.Webserver
{
    public sealed class HttpRequestModel
    {
        public NameValueCollection QueryString { get; set; }
        public CookieCollection Cookies { get; set; }
        public string UserHostName { get; set; }
        public string UserHostAddress { get; set; }
        public string UserAgent { get; set; }
        public Uri UrlReferrer { get; set; }
        public Uri Url { get; set; }
        public string RawUrl { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public Stream InputStream { get; set; }
        public string HttpMethod { get; set; }
        public NameValueCollection Headers { get; set; }
        public string ContentType { get; set; }
        public long ContentLength64 { get; set; }
        public Encoding ContentEncoding { get; set; }
        public string[] AcceptTypes { get; set; }

        public static implicit operator HttpRequestModel(HttpListenerRequest r)
        {
            return new HttpRequestModel()
            {
                QueryString = r.QueryString,
                AcceptTypes = r.AcceptTypes,
                ContentEncoding = r.ContentEncoding,
                ContentLength64 = r.ContentLength64,
                ContentType = r.ContentType,
                Cookies = r.Cookies,
                Headers = r.Headers,
                HttpMethod = r.HttpMethod,
                InputStream = r.InputStream,
                LocalEndPoint = r.LocalEndPoint,
                RawUrl = r.RawUrl,
                RemoteEndPoint = r.RemoteEndPoint,
                Url = r.Url,
                UrlReferrer = r.UrlReferrer,
                UserAgent = r.UserAgent,
                UserHostAddress = r.UserHostAddress,
                UserHostName = r.UserHostName
            };
        }
    }
}
