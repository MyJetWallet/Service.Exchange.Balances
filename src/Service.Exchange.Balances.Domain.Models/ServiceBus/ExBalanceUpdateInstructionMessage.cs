using System.Runtime.Serialization;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain.Models.ServiceBus
{
    [DataContract]
    public class ExBalanceUpdateInstructionMessage : ExBalanceUpdateInstruction
    {
        public static readonly string ServiceBusTopicName = "jet-wallet-balance-update-instruction";
    }
}