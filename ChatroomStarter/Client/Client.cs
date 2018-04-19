using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        TcpClient clientSocket;
        NetworkStream stream;
        public string displayName;
        public Client(string IP, int port)
        {
            SetDisplayName();
            clientSocket = new TcpClient();
            clientSocket.Connect(IPAddress.Parse(IP), port);
            stream = clientSocket.GetStream();
        }
        private void SetDisplayName()
        {
            UI.DisplayMessage("What would you like your display name to be?");
            displayName = UI.GetInput();
        }
        Task Send()
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    if (clientSocket.Connected)
                    {
                        string messageString = UI.GetInput();
                        byte[] message = Encoding.ASCII.GetBytes(messageString);
                        stream.Write(message, 0, message.Count());
                    }
                }
            });
        }
        Task Recieve()
        {
            return Task.Run(() =>
            {
                Object messageLock = new object();
                lock (messageLock)
                {
                    if (clientSocket.Connected)
                    {
                        byte[] recievedMessage = new byte[256];
                        stream.Read(recievedMessage, 0, recievedMessage.Length);
                        UI.DisplayMessage(Encoding.ASCII.GetString(recievedMessage));
                    }
                }
            });
        }
        public void Run()
        {
            while (true)
            {
                Parallel.Invoke(

                    async () =>
                    {
                        await Send();
                    },

                    async () =>
                    {
                        await Recieve();
                    }
                );
            }
        }
    }
}
