using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Grpc
{
    [ServiceContract]
    public interface IBalanceOperationService
    {
        [OperationContract]
        Task<List<ExBalanceUpdate>> ProcessBalanceUpdates(List<ExBalanceUpdateInstruction> request);
    }
}