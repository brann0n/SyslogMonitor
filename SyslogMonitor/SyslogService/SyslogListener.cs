using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor
{
    public delegate void SyslogConnectionMessageHandler(string connectionState);

    public class SyslogListener
    {
        UdpClient server;
        IPEndPoint endpointUDP;
        public DateTime lastReceiveTime;
        private bool CalledStop;
        private string PCIP;

        public event SyslogConnectionMessageHandler ConnectionChanged;

        public SyslogListener(string ipAdress, int port)
        {
            //init vars
            lastReceiveTime = new DateTime();
            CalledStop = false;
            PCIP = ipAdress;

            //init connection objects for later use.
            IPAddress localAddr = IPAddress.Parse(ipAdress);
            endpointUDP = new IPEndPoint(localAddr, port);
            server = new UdpClient(endpointUDP);
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
                Console.WriteLine($"Syslog Listener started...");
                while (!CalledStop)
                {
                    byte[] bits = server.Receive(ref endpointUDP);
                    string response = Encoding.ASCII.GetString(bits, 0, bits.Length);
                    DataReceivedObject receiv = new DataReceivedObject
                    {
                        Data = response.Replace("\n", ""),
                        IPAddress = endpointUDP.Address.ToString(),
                        ReceivedDateTime = DateTime.Now,
                        Timestamp = "<<Timestamp>>"
                    };

                    var validRule = SyslogFilters.CheckConnectionState(receiv.Data);

                    //this item is a connection log
                    if (validRule.Item1)
                    {
                        ConnectionChanged?.Invoke(validRule.Item2);
                    }

                    lastReceiveTime = DateTime.Now;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            Console.WriteLine("Service stopped!");
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
            server.Close();
            Console.WriteLine("Telling service to stop...");
        }
    }
}
