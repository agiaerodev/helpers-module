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
        public  Task SendMessageAsync(string data, string topic)
        {
            return Task.CompletedTask;
        }

        public  Task SendMessageAsync(string data, string messageChannel, MessageType messageType = MessageType.Topic)
        {
            return Task.CompletedTask;
        }

        public Task StartListeningAsync<TMessage, THandler>(string subscriptionName, string messageChannel, MessageType messageType = MessageType.Topic, CancellationToken cancellationToken = default) where THandler : IMessageHandler<TMessage>
        {
            return Task.CompletedTask;
        }
    }
}
