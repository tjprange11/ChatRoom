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
        Queue<Message> messages;
        TcpListener server;
        ILogger logger;
        public Server()
        {
            users = new Dictionary<int, ISubscriber>();
            messages = new Queue<Message>();
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
            while (true)
            {
                Parallel.Invoke(
                    //This thread is always listening for new clients (users)
                    async () =>
                    {
                        await AcceptUser();
                    },
                    //This thread is always listening for new messages
                    async () =>
                    {
                        await GetAllMessages();
                    },
                    //This thread is always sending new messages
                    async () =>
                    {
                        await SendAllMessages();
                    },
                    //This thread is always checking for new connections and checking for disconnections
                    async () =>
                    {
                        await CheckIfConnected();
                    }


                );
            }
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
        Task SendAllMessages()
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    if (messages.Count > 0)
                    {
                        for (int i = 0; i < users.Count; i++)
                        {
                            for (int j = 0; j < messages.Count; j++)
                            {
                                users.ElementAt(i).Value.Send(messages.ElementAt(j));
                            }
                        }
                        messages.Clear();
                    }
                }
            });
        }
        Task SendUsers(Dictionary<int, ISubscriber> users, Client client)
        {
            return Task.Run(() =>
            {
                Object sendLock = new Object();
                lock (sendLock)
                {
                    try
                    {
                        if (users.Count > 0)
                        {
                            for (int i = 0; i < users.Count; i++)
                            {
                                byte[] userName = Encoding.ASCII.GetBytes(users.Values.ElementAt(i).ToString());
                                client.stream.Write(userName, 0, userName.Count());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: '{0}'", e);
                    }
                }
            });
        }
        Task SendUserMessage(Client user, Message message)
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    if (user.CheckIfConnected())
                    {
                        user.Send(message);
                    }
                }
            });
        }
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
        }
    }
}
