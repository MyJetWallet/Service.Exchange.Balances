using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;

namespace Service.Exchange.Balances
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly ServiceBusLifeTime _myServiceBusTcpClient;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger,
            ServiceBusLifeTime myServiceBusTcpClient)
            : base(appLifetime)
        {
            _logger = logger;
            _myServiceBusTcpClient = myServiceBusTcpClient;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called");
            _myServiceBusTcpClient.Start();
            _logger.LogInformation("MyServiceBusTcpClient is started");
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called");
            _myServiceBusTcpClient.Stop();
            _logger.LogInformation("MyServiceBusTcpClient is stopped");
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called");
        }
    }
}