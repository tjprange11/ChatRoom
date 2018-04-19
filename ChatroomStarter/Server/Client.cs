using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        NetworkStream stream;
        TcpClient client;
        public int UserId;
        public string displayName;
        public Client(NetworkStream Stream, TcpClient Client)
        {
            stream = Stream;
            client = Client;
            UserId = stream.GetHashCode();
            this.displayName = UserId.ToString();
        }
        public void CloseStream()
        {
            stream.Close();
            client.Close();
        }
        public void Send(Message message)
        {
            Object sendLock = new Object();
            lock (sendLock)
            {
                try
                {
                    byte[] messageBody = Encoding.ASCII.GetBytes(message.Body);
                    stream.Write(messageBody, 0, messageBody.Count());
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: '{0}'", e);
                }
            }
        }
        public string Recieve()
        {
            byte[] recievedMessage = new byte[256];
            stream.Read(recievedMessage, 0, recievedMessage.Length);
            string recievedMessageString = Encoding.ASCII.GetString(recievedMessage);
            Console.WriteLine(recievedMessageString);
            return recievedMessageString;
        }

    }
}
