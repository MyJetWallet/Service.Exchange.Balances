using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Exchange.Balances.Domain.Models.ServiceBus;

namespace Service.Exchange.Balances.Client.ServiceBus
{
    public class ExBalanceUpdateSubscriber : ISubscriber<ExBalanceUpdateMessage>
    {
        private readonly List<Func<ExBalanceUpdateMessage, ValueTask>> _list = new();

        public ExBalanceUpdateSubscriber(
            MyServiceBusTcpClient client,
            string queueName,
            TopicQueueType queryType)
        {
            client.Subscribe(ExBalanceUpdateMessage.ServiceBusTopicName, queueName, queryType, Handler);
        }

        private async ValueTask Handler(IMyServiceBusMessage data)
        {
            var item = Deserializer(data.Data);

            if (!_list.Any())
            {
                throw new Exception("Cannot handle event. No subscribers");
            }

            foreach (var callback in _list)
            {
                await callback.Invoke(item);
            }
        }


        public void Subscribe(Func<ExBalanceUpdateMessage, ValueTask> callback)
        {
            this._list.Add(callback);
        }

        private ExBalanceUpdateMessage Deserializer(ReadOnlyMemory<byte> data) =>
            data.ByteArrayToServiceBusContract<ExBalanceUpdateMessage>();
    }
}