using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client : ISubscriber
    {
        public NetworkStream stream;
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
                catch { }
            }
        }
        public Message Recieve()
        {
            Object recieveLock = new Object();
            lock (recieveLock)
            {
                byte[] recievedMessage = new byte[256];
                try
                {
                    stream.Read(recievedMessage, 0, recievedMessage.Length);
                }
                catch { }
                string recievedMessageString;
                if (recievedMessage[0] == 0)
                {
                    recievedMessageString = "I've left the chat!";
                }
                else
                {
                    recievedMessageString = Encoding.ASCII.GetString(recievedMessage);
                }
                Message message = new Message(this, recievedMessageString);
                return message;
            }
        }
        public string ReceiveDisplayName()
        {
            byte[] receivedDisplayNameArray = new byte[256];
            stream.Read(receivedDisplayNameArray, 0, receivedDisplayNameArray.Length);
            int displayNameLength = 0;
            for (int i = 0; i < receivedDisplayNameArray.Length; i++)
            {
                if (receivedDisplayNameArray[i] != 0)
                {
                    displayNameLength++;
                }
            }
            byte[] displayNameArray = new byte[displayNameLength];
            Array.Copy(receivedDisplayNameArray, displayNameArray, displayNameLength);
            string receivedDisplayName = Encoding.ASCII.GetString(displayNameArray);
            return receivedDisplayName;
        }

        public bool CheckIfConnected()
        {
            return client.Connected;
        }
    }
}
