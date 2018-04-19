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
        private bool isCommunicationOpen = true;
        public static Client client;
        TcpListener server;
        Dictionary<int, ISubscriber> users;
        Queue<Message> messages;

        public Server()
        {
            users = new Dictionary<int, ISubscriber>();
            messages = new Queue<Message>();
            string computerIP = GetIP();
            Console.WriteLine("Computer IP address is :" + computerIP);
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            server.Start();
        }
        public void Run()
        {
            Task[] tasks = new Task[5];
            for (int i = 0; i < 5; i++)
            {
                AcceptClient();
                while (isCommunicationOpen)

                {
                    string message = client.Recieve();
                    Respond(message);
                }
            }
        }








        TcpClient clientSocket = default(TcpClient);
        clientSocket = server.AcceptTcpClient();
                    Console.WriteLine("Connected");
                    NetworkStream stream = clientSocket.GetStream();
        client = new Client(stream, clientSocket);


        private string GetIP()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] clientIPAddress = Dns.GetHostAddresses(hostName);
            string computerIP = "127.0.0.1";
            foreach (IPAddress ip4 in clientIPAddress.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                computerIP = ip4.ToString();
            }
            return computerIP;
        }





        private void Respond(string body)
        {
            client.Send(body);
        }
    }
}

