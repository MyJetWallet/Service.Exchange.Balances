using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf.Grpc.Client;
using Service.Exchange.Balances.Client;
using Service.Exchange.Sdk.Messages;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();


            var factory = new ExchangeBalancesClientFactory("http://localhost:100");
            var client = factory.GetBalanceOperationService();

            var request1Id = Guid.NewGuid().ToString();

            var response = await client.ProcessBalanceUpdates(new List<ExBalanceUpdateInstruction>
            {
                new()
                {
                    OperationId = request1Id,
                    Updates = new List<ExBalanceUpdateInstruction.BalanceUpdate>
                    {
                        new()
                        {
                            WalletId = "TestWallet",
                            AssetId = "USD",
                            Number = 1,
                            Amount = 100,
                            ReserveAmount = 0
                        }
                    }
                }
            });

            Console.WriteLine("First response:");
            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            response = await client.ProcessBalanceUpdates(new List<ExBalanceUpdateInstruction>
            {
                new()
                {
                    OperationId = request1Id,
                    Updates = new List<ExBalanceUpdateInstruction.BalanceUpdate>
                    {
                        new()
                        {
                            WalletId = "TestWallet",
                            AssetId = "USD",
                            Number = 1,
                            Amount = 100,
                            ReserveAmount = 0
                        }
                    }
                }
            });
            Console.WriteLine("Duplicate response:");
            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));


            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}