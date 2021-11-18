using System.Runtime.Serialization;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain.Models.ServiceBus
{
    [DataContract]
    public class ExBalanceUpdateMessage
    {
        public static readonly string ServiceBusTopicName = "jet-wallet-balance-update";
        
        [DataMember(Order = 1)]
        public ExBalanceUpdate Update { get; set; }

        public ExBalanceUpdateMessage()
        {
        }

        public ExBalanceUpdateMessage(ExBalanceUpdate update)
        {
            Update = update;
        }
    }
}