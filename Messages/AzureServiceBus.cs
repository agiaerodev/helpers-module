using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Ihelpers.Helpers;
using Ihelpers.Messages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Messages
{
    public class AzureServiceBus : IMessageProvider
    {
        private ServiceBusClient client;
        private List<string> mensajes;
        private string _servicebusKey = string.Empty;
        private static readonly HashSet<string> _verifiedTopics = new HashSet<string>();
        private static readonly HashSet<string> _verifiedQueues = new HashSet<string>();
        private readonly IServiceProvider _serviceProvider;
        private ServiceBusProcessor? _processor;
        public AzureServiceBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _servicebusKey = ConfigurationHelper.GetConfig<string>("ConnectionStrings:AzureServiceBusKey");
            this.client = new ServiceBusClient(_servicebusKey);
        }

        public AzureServiceBus()
        {
            _servicebusKey = ConfigurationHelper.GetConfig<string>("ConnectionStrings:AzureServiceBusKey");
            this.client = new ServiceBusClient(_servicebusKey);
            this.mensajes = new List<string>();
        }

        public async Task SendMessageAsync(string data, string topic)
        {

            //create topic if not exists

            if (!_verifiedTopics.Contains(topic))
            {
                var adminClient = new ServiceBusAdministrationClient(_servicebusKey);
                if (!await adminClient.TopicExistsAsync(topic))
                {
                    await adminClient.CreateTopicAsync(topic);
                }
                _verifiedTopics.Add(topic);
            }


            ServiceBusSender sender = this.client.CreateSender(topic);

            ServiceBusMessage message = new ServiceBusMessage(data);

            await sender.SendMessageAsync(message);
        }

        public async Task SendMessageAsync(string data, string messageChannel, MessageType messageType = MessageType.Topic)
        {


            if (messageType == MessageType.Topic)
            {
                await SendMessageAsync(data, messageChannel);

                return;
            }

            if (!_verifiedQueues.Contains(messageChannel))
            {
                var adminClient = new ServiceBusAdministrationClient(_servicebusKey);
                if (!await adminClient.TopicExistsAsync(messageChannel))
                {
                    await adminClient.CreateTopicAsync(messageChannel);
                }
                _verifiedQueues.Add(messageChannel);
            }



            ServiceBusSender sender = this.client.CreateSender(messageChannel);

            ServiceBusMessage message = new ServiceBusMessage(data);

            await sender.SendMessageAsync(message);
        }

        public async Task StartListeningAsync<TMessage, THandler>(
     string subscriptionName,
     string messageChannel,
     MessageType messageType = MessageType.Topic,
     CancellationToken cancellationToken = default)
     where THandler : IMessageHandler<TMessage>
        {
            var adminClient = new ServiceBusAdministrationClient(_servicebusKey);

            if (messageType == MessageType.Topic)
            {
                if (!await adminClient.TopicExistsAsync(messageChannel))
                {
                    await adminClient.CreateTopicAsync(messageChannel);
                }

                if (!await adminClient.SubscriptionExistsAsync(messageChannel, subscriptionName))
                {
                    await adminClient.CreateSubscriptionAsync(messageChannel, subscriptionName);
                }

                _processor = client.CreateProcessor(messageChannel, subscriptionName);
            }
            else
            {
                if (!await adminClient.QueueExistsAsync(messageChannel))
                {
                    await adminClient.CreateQueueAsync(messageChannel);
                }
                _processor = client.CreateProcessor(messageChannel);
            }

            _processor.ProcessMessageAsync += async (args) =>
            {
                Console.WriteLine($"Mensaje recibido: {args.Message.Body}");

                string body = args.Message.Body.ToString();
                var message = JsonConvert.DeserializeObject<TMessage>(body);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                    await handler.HandleAsync(message);
                }
                await args.CompleteMessageAsync(args.Message);
            };

            _processor.ProcessErrorAsync += (args) => {
                Console.WriteLine($"Error en Service Bus: {args.Exception.Message}");
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopListeningAsync(CancellationToken cancellationToken = default)
        {
            if (_processor != null) await _processor.StopProcessingAsync(cancellationToken);
        }
    }

    public class ServiceBusWorker<TMessage, THandler> : BackgroundService
        where THandler : IMessageHandler<TMessage>
    {
        private readonly IMessageProvider _messageProvider;
        private readonly string _topic;
        private readonly string _subscription;

        public ServiceBusWorker(IMessageProvider messageProvider, string topic, string subscription)
        {
            _messageProvider = messageProvider;
            _topic = topic;
            _subscription = subscription;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageProvider.StartListeningAsync<TMessage, THandler>(
                _subscription,
                _topic,
                MessageType.Topic,
                stoppingToken
            );
        }
    }
}
