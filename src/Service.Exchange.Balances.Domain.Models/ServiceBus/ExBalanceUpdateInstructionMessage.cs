using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain.Models.ServiceBus
{
    public class ExBalanceUpdateInstructionMessage : ExBalanceUpdateInstruction
    {
        public static readonly string ServiceBusTopicName = "jet-wallet-balance-update-instruction";
    }
}