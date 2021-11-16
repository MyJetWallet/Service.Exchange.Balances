using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.Exchange.Balances.Domain;
using Service.Exchange.Balances.Grpc;
using Service.Exchange.Sdk.Messages;

namespace Service.Exchange.Balances.Services
{
    public class BalanceOperationService : IBalanceOperationService
    {
        private readonly ILogger<BalanceOperationService> _logger;
        private readonly IBalancesService _balancesService;

        public BalanceOperationService(ILogger<BalanceOperationService> logger, IBalancesService balancesService)
        {
            _logger = logger;
            _balancesService = balancesService;
        }

        public async Task<List<ExBalanceUpdate>> ProcessBalanceUpdates(List<ExBalanceUpdateInstruction> request)
        {
            request.AddToActivityAsJsonTag("request-data");
            _logger.LogInformation("Receive ProcessBalanceUpdates request: {JsonRequest}",
                JsonConvert.SerializeObject(request));

            var result = new List<ExBalanceUpdate>();

            foreach (var exBalanceUpdateInstruction in request)
            {
                var responseResult = await _balancesService.ProcessBalanceUpdates(exBalanceUpdateInstruction);
                result.Add(responseResult);
            }

            result.AddToActivityAsJsonTag("response-data");
            _logger.LogInformation("Processed ProcessBalanceUpdates request: {JsonResult}",
                JsonConvert.SerializeObject(result));

            return result;
        }
    }
}