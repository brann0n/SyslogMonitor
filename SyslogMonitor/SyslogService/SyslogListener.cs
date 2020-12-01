using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public delegate void SyslogConnectionMessageHandler(string connectionState, DataReceivedObject rObject);

    public class SyslogListener
    {
        private readonly UdpClient Server;
        IPEndPoint UdpEndpoint;
        public DateTime LastReceiveTime;
        private bool CalledStop;

        public event SyslogConnectionMessageHandler ConnectionChanged;

        public SyslogListener(string ipAdress, int port)
        {
            //init vars
            LastReceiveTime = new DateTime();
            CalledStop = false;

            //init connection objects for later use.
            IPAddress localAddr = IPAddress.Parse(ipAdress);
            UdpEndpoint = new IPEndPoint(localAddr, port);
            Server = new UdpClient(UdpEndpoint);
        }

        /// <summary>
        /// Listens to the specified ip address for incoming UDP messages, and pushes those to a buffer
        /// Also Raises the SSHConsoleSyslog.SyslogListener.DataReceived Event
        /// </summary>
        /// <returns>Task completion information</returns>
        public Task Start()
        {
            byte[] bytes = new byte[256];
            CalledStop = false;
            try
            {
                BConsole.WriteLine($"Syslog Listener started...");
                while (!CalledStop)
                {
                    byte[] bits = Server.Receive(ref UdpEndpoint);
                    string response = Encoding.ASCII.GetString(bits, 0, bits.Length);
                    DataReceivedObject receiv = new DataReceivedObject
                    {
                        Data = response.Replace("\n", ""),
                        IPAddress = UdpEndpoint.Address.ToString(),
                        ReceivedDateTime = DateTime.Now
                    };

                    var validRule = SyslogFilters.CheckConnectionState(receiv.Data);

                    //this item is a connection log
                    if (validRule.Item1)
                    {
                        ConnectionChanged?.Invoke(validRule.Item2, receiv);
                    }

                    LastReceiveTime = DateTime.Now;
                }
            }
            catch (SocketException e)
            {
                BConsole.WriteLine($"Exception: {e.Message}");
            }
            catch (Exception e)
            {
                BConsole.WriteLine($"Exception: {e.Message}");
            }
            BConsole.WriteLine("Service stopped!");
            return Task.CompletedTask;
        }

        /// <summary>
        /// gets the currents service status
        /// </summary>
        /// <returns>true if the service is running, false if not</returns>
        public bool Status()
        {
            return this.CalledStop;
        }

        /// <summary>
        /// Function that stops the listner from listining on the port
        /// </summary>
        public void Stop()
        {
            CalledStop = true;
            Server.Close();
            BConsole.WriteLine("Telling service to stop...");
        }
    }
}
