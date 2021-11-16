using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Exchange.Balances.Grpc;

namespace Service.Exchange.Balances.Client
{
    [UsedImplicitly]
    public class ExchangeBalancesClientFactory
    {
        private readonly CallInvoker _channel;

        public ExchangeBalancesClientFactory(string grpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(grpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IBalanceOperationService GetBalanceOperationService() =>
            _channel.CreateGrpcService<IBalanceOperationService>();
    }
}