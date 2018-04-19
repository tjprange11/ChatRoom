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
<<<<<<< HEAD





        private void Respond(string body)
        {
            client.Send(body);
=======
        Task GetAllMessages()
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        Parallel.Invoke(
                            async () =>
                            {
                                await GetUserMessage(users.ElementAt(i).Value);
                            }
                        );
                    }
                }
            });
        }
        Task GetUserMessage(ISubscriber user)
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    if (user.CheckIfConnected())
                    {
                        Message message = user.Recieve();
                        Console.WriteLine(message.Body);
                        logger.Save(message);
                        messages.Enqueue(message);
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
>>>>>>> ef068abd7986f8df136d8bb1d4f4e8b4f47bef8e
        }
    }
}

