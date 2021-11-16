using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain.Models.ServiceBus
{
    public class ExBalanceUpdateMessage : ExBalanceUpdate
    {
        public static readonly string ServiceBusTopicName = "jet-wallet-balance-update";

        public ExBalanceUpdateMessage(ExBalanceUpdate update)
        {
            OperationId = update.OperationId;
            Instance = update.Instance;
            Timestamp = update.Timestamp;
            Result = update.Result;
            Updates = update.Updates;
            Balances = update.Balances;
        }
    }
}