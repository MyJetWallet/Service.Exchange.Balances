using System.Runtime.Serialization;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain.Models.ServiceBus
{
    [DataContract]
    public class ExBalanceUpdateInstructionMessage
    {
        public static readonly string ServiceBusTopicName = "jet-wallet-balance-update-instruction";
        
        [DataMember(Order = 1)]
        public ExBalanceUpdateInstruction Instruction { get; set; }

        public ExBalanceUpdateInstructionMessage()
        {
        }

        public ExBalanceUpdateInstructionMessage(ExBalanceUpdateInstruction instruction)
        {
            Instruction = instruction;
        }
    }
}