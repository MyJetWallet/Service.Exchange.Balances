using System.Runtime.Serialization;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain.Models.ServiceBus
{
    [DataContract]
    public class ExBalanceUpdateMessage : ExBalanceUpdate
    {
        public static readonly string ServiceBusTopicName = "jet-wallet-balance-update";

        public ExBalanceUpdateMessage()
        {
        }

        public ExBalanceUpdateMessage(ExBalanceUpdate update)
        {
            OperationId = update.OperationId;
            Instance = update.Instance;
            Timestamp = update.Timestamp;
            Result = update.Result;
            Updates = update.Updates;
            Balances = update.Balances;
            EventType = update.EventType;
        }
    }
}