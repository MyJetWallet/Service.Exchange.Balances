using Autofac;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Exchange.Balances.Client.ServiceBus;
using Service.Exchange.Balances.Domain;
using Service.Exchange.Balances.Domain.Models.ServiceBus;
using Service.Exchange.Balances.Jobs;
using Service.Exchange.Balances.Services;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);

            builder.RegisterInstance(new ExBalanceUpdateInstructionSubscriber(serviceBusClient,
                "Service.Exchange.Balance",
                TopicQueueType.Permanent)).As<ISubscriber<ExBalanceUpdateInstructionMessage>>().SingleInstance();

            builder.RegisterMyServiceBusPublisher<ExBalanceUpdateMessage>(serviceBusClient,
                ExBalanceUpdateMessage.ServiceBusTopicName, false);

            builder
                .RegisterType<BalanceUpdateInstructionJob>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<BalancesService>()
                .As<IBalancesService>()
                .SingleInstance();
        }
    }
}