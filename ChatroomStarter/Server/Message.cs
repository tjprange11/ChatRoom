using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Message
    {
        public Client sender;
        public string Body;
        public int UserId;
        public Message(Client Sender, string Body)
        {
            sender = Sender;
            StringBuilder wholeBody = new StringBuilder();
            wholeBody.Append(sender.displayName);
            wholeBody.Append(": ");
            wholeBody.Append(Body);
            this.Body = wholeBody.ToString();
            UserId = sender.UserId;
        }
    }
}
