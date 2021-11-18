using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Newtonsoft.Json;
using Service.Exchange.Balances.Domain;
using Service.Exchange.Balances.Domain.Models.ServiceBus;

namespace Service.Exchange.Balances.Jobs
{
    public class BalanceUpdateInstructionJob
    {
        private readonly ILogger<BalanceUpdateInstructionJob> _logger;
        private readonly IBalancesService _balancesService;
        private readonly IServiceBusPublisher<ExBalanceUpdateMessage> _depositPublisher;

        public BalanceUpdateInstructionJob(
            ISubscriber<ExBalanceUpdateInstructionMessage> subscriber,
            ILogger<BalanceUpdateInstructionJob> logger,
            IBalancesService balancesService,
            IServiceBusPublisher<ExBalanceUpdateMessage> depositPublisher)
        {
            _logger = logger;
            _balancesService = balancesService;
            _depositPublisher = depositPublisher;
            subscriber.Subscribe(HandleSignal);
        }

        private async ValueTask HandleSignal(ExBalanceUpdateInstructionMessage exBalanceUpdateInstruction)
        {
            using var activity = MyTelemetry.StartActivity("Handle Event ExBalanceUpdateInstructionMessage");
            try
            {
                exBalanceUpdateInstruction.AddToActivityAsJsonTag("request-data");
                _logger.LogInformation("Receive ExBalanceUpdateInstructionMessage: {JsonRequest}",
                    JsonConvert.SerializeObject(exBalanceUpdateInstruction));
                var result = await _balancesService.ProcessBalanceUpdates(exBalanceUpdateInstruction.Instruction);

                result.AddToActivityAsJsonTag("response-data");
                _logger.LogInformation("Processed ProcessBalanceUpdates request: {JsonResult}",
                    JsonConvert.SerializeObject(result));

                await _depositPublisher.PublishAsync(new ExBalanceUpdateMessage(result));
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }
    }
}