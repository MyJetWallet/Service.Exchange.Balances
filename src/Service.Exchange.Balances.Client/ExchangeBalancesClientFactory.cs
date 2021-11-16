using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Exchange.Balances.Grpc;

namespace Service.Exchange.Balances.Client
{
    [UsedImplicitly]
    public class ExchangeBalancesClientFactory : MyGrpcClientFactory
    {
        public ExchangeBalancesClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IBalanceOperationService GetBalanceOperationService() => CreateGrpcService<IBalanceOperationService>();
    }
}