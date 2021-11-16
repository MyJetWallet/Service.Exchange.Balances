using Autofac;
using Service.Exchange.Balances.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Exchange.Balances.Client
{
    public static class AutofacHelper
    {
        public static void RegisterExchangeBalancesClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ExchangeBalancesClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetBalanceOperationService())
                .As<IBalanceOperationService>().SingleInstance();
        }
    }
}