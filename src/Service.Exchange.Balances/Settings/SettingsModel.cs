using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Exchange.Balances.Settings
{
    public class SettingsModel
    {
        [YamlProperty("ExchangeBalances.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("ExchangeBalances.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("ExchangeBalances.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("ExchangeBalances.InstanceName")]
        public string InstanceName { get; set; }

        [YamlProperty("ExchangeBalances.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }

        [YamlProperty("ExchangeBalances.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }
    }
}