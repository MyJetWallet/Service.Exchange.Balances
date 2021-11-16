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
    public class ExBalanceUpdateInstructionSubscriber : ISubscriber<ExBalanceUpdateInstructionMessage>
    {
        private readonly List<Func<ExBalanceUpdateInstructionMessage, ValueTask>> _list = new();

        public ExBalanceUpdateInstructionSubscriber(
            MyServiceBusTcpClient client,
            string queueName,
            TopicQueueType queryType)
        {
            client.Subscribe(ExBalanceUpdateInstructionMessage.ServiceBusTopicName, queueName, queryType, Handler);
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


        public void Subscribe(Func<ExBalanceUpdateInstructionMessage, ValueTask> callback)
        {
            this._list.Add(callback);
        }

        private ExBalanceUpdateInstructionMessage Deserializer(ReadOnlyMemory<byte> data) =>
            data.ByteArrayToServiceBusContract<ExBalanceUpdateInstructionMessage>();
    }
}