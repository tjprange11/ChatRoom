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
        Dictionary<int, ISubscriber> users;
        TcpListener server;
        ILogger logger;
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
        Task CheckIfConnected()
        {
            return Task.Run(() =>
            {
                Object userListLock = new Object();
                lock (userListLock)
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        Client currentUser = (Client)users.ElementAt(i).Value;
                        if (!currentUser.CheckIfConnected())
                        {
                            int userKey = users.ElementAt(i).Key;
                            users.Remove(userKey);
                        }
                    }
                }
            });
        }
        Task AcceptUser()
        {
            return Task.Run(() =>
            {
                Object userListLock = new Object();
                lock (userListLock)
                {
                    TcpClient clientSocket = default(TcpClient);
                    clientSocket = server.AcceptTcpClient();
                    Console.WriteLine("Connected");
                    NetworkStream stream = clientSocket.GetStream();
                    Client user = new Client(stream, clientSocket);
                    user.displayName = user.ReceiveDisplayName();
                    users.Add(user.UserId, user);
                    NotifyUsersOfNewUser(user);
                }
            });
        }

        public void NotifyUsersOfNewUser(Client user)
        {
            Message notification = new Message(user, "I've joined the chat!");
            logger.Save(notification);
            for (int i = 0; i < users.Count; i++)
            {
                users.ElementAt(i).Value.Send(notification);
            }
        }
        private void Respond(string body)
        {
             client.Send(body);
        }
    }
}
