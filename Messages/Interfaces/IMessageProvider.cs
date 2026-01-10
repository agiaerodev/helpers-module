using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Messages.Interfaces
{
    public interface IMessageProvider
    {
        Task SendMessageAsync(string data, string topic);
        Task SendMessageAsync(string data, string messageChannel, MessageType messageType = MessageType.Topic);
        Task StartListeningAsync<TMessage, THandler>(string subscriptionName, string messageChannel, MessageType messageType = MessageType.Topic, CancellationToken cancellationToken = default) where THandler : IMessageHandler<TMessage>;
    }
    public enum MessageType
    {
        Topic,
        Queue
    }
}
