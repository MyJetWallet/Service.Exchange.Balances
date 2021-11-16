using System.ServiceModel;
using System.Threading.Tasks;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Domain
{
    [ServiceContract]
    public interface IBalancesService
    {
        [OperationContract]
        Task<ExBalanceUpdate> ProcessBalanceUpdates(ExBalanceUpdateInstruction request);
    }
}