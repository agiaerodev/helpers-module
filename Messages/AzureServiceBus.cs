using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Ihelpers.Helpers;
using Ihelpers.Messages.Interfaces;
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

        public AzureServiceBus()
        {
            _servicebusKey = ConfigurationHelper.GetConfig<string>("ConnectionStrings:AzureServiceBusKey");
            this.client = new ServiceBusClient(_servicebusKey);
            this.mensajes = new List<string>();
        }

        public async Task SendMessageAsync(string data, string topic)
        {


            //create topic if not exists

            var adminClient = new ServiceBusAdministrationClient(_servicebusKey);


            // Create a subscription
            if (!await adminClient.TopicExistsAsync(topic))
            {
                await adminClient.CreateTopicAsync(topic);
            }



            ServiceBusSender sender = this.client.CreateSender(topic);

            ServiceBusMessage message = new ServiceBusMessage(data);

            await sender.SendMessageAsync(message);
        }





        //public async Task<List<string>> ReceiveMessagesAsync(string topic)
        //{
        //    ServiceBusProcessor processor = this.client.CreateProcessor(topic);

        //    processor.ProcessMessageAsync += Processor_ProcessMessageAsync;

        //    processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

        //    await processor.StartProcessingAsync();

        //    Thread.Sleep(3000);

        //    await processor.StopProcessingAsync();

        //    return this.mensajes;
        //}

        //private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        //{
        //    string content = arg.Message.Body.ToString();

        //    this.mensajes.Add(content);

        //    await arg.CompleteMessageAsync(arg.Message);
        //}

        //private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        //{
        //    Debug.WriteLine(arg.Exception.ToString());
        //    return Task.CompletedTask;
        //}
    }
}
