using Ihelpers.Messages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Messages
{
    public class MessageProvider : IMessageProvider
    {
        public async Task SendMessageAsync(string data, string topic)
        {
            return;
        }
    }
}
