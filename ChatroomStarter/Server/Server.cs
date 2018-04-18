using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        public static Client client;
        TcpListener server;
        public Server()
        {
            string computerIP = GetIPAddress();
            Console.WriteLine("Local Computer IP Address: " + computerIP);
            Console.WriteLine();
            server = new TcpListener(IPAddress.Parse(computerIP), 9999);
            server.Start();
        }
        private string GetIPAddress()
        {
            string hostName = Dns.GetHostName();

            IPAddress[] ipAddress = Dns.GetHostAddresses(hostName);
            string computerIP = "127.0.0.1";
            foreach (IPAddress ip4 in ipAddress.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                computerIP = ip4.ToString();
            }
            return computerIP;
        }
        public void Run()
        {
            AcceptClient();
            string message = client.Recieve();
            Respond(message);
        }
        Task AcceptClient()
        {
            return Task.Run(() =>
            {
                Object clientLock = new object();
                lock (clientLock)
                {
                    TcpClient clientSocket = default(TcpClient);
                    clientSocket = server.AcceptTcpClient();
                    Console.WriteLine("Connected");
                    NetworkStream stream = clientSocket.GetStream();
                    client = new Client(stream, clientSocket);
                }
            });
           
        }
        private void Respond(string body)
        {
             client.Send(body);
        }
    }
}
