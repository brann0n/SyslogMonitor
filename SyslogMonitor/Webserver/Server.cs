using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyslogMonitor.Webserver
{
    public sealed class Server
    {
        private readonly HttpListener listener;
        private readonly CancellationTokenSource CancelSource;
        public delegate HttpResponseModel HandleHttpRequest(HttpRequestModel request);
        private readonly string Host;
        private readonly string Protocol;
        private readonly int Port;

        public event HandleHttpRequest OnRequest;

        public Server(string host, string protocol, int port)
        {
            Host = host;
            Protocol = protocol;
            Port = port;

            listener = new HttpListener();
            listener.Prefixes.Add($"{Protocol}://{Host}:{Port}/");
            listener.Prefixes.Add($"http://localhost:{Port}/");
            CancelSource = new CancellationTokenSource();
        }

        public async Task Listen()
        {
            if (OnRequest == null)
                throw new Exception("Please assign an event listener to OnRequest before starting the listner.");
            listener.Start();
            BConsole.WriteLine($"HttpServer started on '{Protocol}://{Host}:{Port}/' listening for requests.", ConsoleColor.DarkMagenta);

            while (true)
            {
                try
                {
                    CancelSource.Token.ThrowIfCancellationRequested();
                    HttpListenerContext context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => HandleInternalHttpRequest(context));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    //server doesnt have a URI assigned or the server hasn't started. exit the while loop.
                    break;
                }
                catch (Exception ex)
                {
                    BConsole.WriteLine(ex.Message, ConsoleColor.Red);
                }
            }

            listener.Close();
            BConsole.WriteLine("HttpServer has stopped", ConsoleColor.DarkMagenta);
            await Task.Yield();
        }

        public void Stop()
        {
            CancelSource.Cancel();
            listener.Abort();
        }

        private void HandleInternalHttpRequest(object state)
        {
            if (OnRequest == null)
                throw new Exception("No event listeners subscribed to OnRequest for incoming requests.");
            try
            {
                HttpListenerContext c = (HttpListenerContext)state;
                HttpListenerRequest request = c.Request;
                HttpListenerResponse response = c.Response;

                BConsole.WriteLine($"Incoming http request from {request.UserHostAddress}", ConsoleColor.Magenta);

                //Send this request up the pipeline for processing.
                HttpResponseModel responseData = OnRequest(request);

                response.StatusCode = responseData.StatusCode;
                response.Headers = responseData.Headers;
                response.Cookies = responseData.Cookies;
                response.StatusDescription = responseData.StatusDescription ?? "No description";

                if (!string.IsNullOrEmpty(responseData.OutputStream))
                {
                    response.ContentType = responseData.ContentType;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseData.OutputStream);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }

                response.Close();
            }
            catch (Exception ex)
            {
                BConsole.WriteLine("HttpServer Error: " + ex.Message, ConsoleColor.Red);
            }
        }
    }
}
