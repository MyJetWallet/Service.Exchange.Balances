using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using Newtonsoft.Json;
using Service.Exchange.Balances.Domain;
using Service.Exchange.Balances.Postgres;
using Service.Exchange.Balances.Postgres.Models;
using Service.Exchange.Sdk.Messages;
using Service.Exchange.Sdk.Models;
// ReSharper disable InconsistentLogPropertyNaming

namespace Service.Exchange.Balances.Services
{
    public class BalancesService : IBalancesService
    {
        private readonly ILogger<BalancesService> _logger;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        private readonly MyLocker _myLocker = new MyLocker();

        public BalancesService(ILogger<BalancesService> logger,
            DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _logger = logger;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<ExBalanceUpdate> ProcessBalanceUpdates(ExBalanceUpdateInstruction request)
        {
            using (await _myLocker.GetLocker())
            {
                try
                {
                    await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
                    var processedRequest =
                        await context.ProcessedOperations.FirstOrDefaultAsync(e =>
                            e.OperationId == request.OperationId);
                    if (processedRequest != null)
                    {
                        _logger.LogInformation("Request already processed, return previous result");
                        return JsonConvert.DeserializeObject<ExBalanceUpdate>(processedRequest.ResponseJson);
                    }

                    var status = ExBalanceUpdate.BalanceUpdateResult.Ok;
                    var updates = new List<ExBalanceUpdate.Update>();
                    var balances = new Dictionary<string, ExBalance>();
                    var now = DateTime.UtcNow;
                    foreach (var updateRequest in request.Updates)
                    {
                        var balance = balances.GetValueOrDefault($"{updateRequest.WalletId}::{updateRequest.AssetId}");
                        if (balance == null)
                        {
                            balance = await context.Balances
                                .Where(e => e.WalletId == updateRequest.WalletId && e.AssetId == updateRequest.AssetId)
                                .SingleOrDefaultAsync();

                            if (balance == null)
                            {
                                balance = new ExBalance
                                {
                                    WalletId = updateRequest.WalletId,
                                    AssetId = updateRequest.AssetId,
                                    Balance = 0,
                                    ReserveBalance = 0,
                                    LastUpdate = now,
                                    Version = 0
                                };
                                context.Balances.Add(balance);
                            }

                            balances.TryAdd($"{updateRequest.WalletId}::{updateRequest.AssetId}", balance);
                        }

                        if (balance.Balance + updateRequest.Amount < 0)
                        {
                            // withdrawal/sell
                            updates.Add(new ExBalanceUpdate.Update
                            {
                                Number = updateRequest.Number,
                                WalletId = updateRequest.WalletId,
                                AssetId = updateRequest.AssetId,
                                Amount = updateRequest.Amount,
                                ReserveAmount = updateRequest.ReserveAmount,
                                Result = ExBalanceUpdate.BalanceUpdateResult.LowBalance,
                                ErrorMessage = "Unable to deduct funds, low balance"
                            });
                            status = ExBalanceUpdate.BalanceUpdateResult.LowBalance;
                            continue;
                        }

                        if (balance.Balance - updateRequest.ReserveAmount < 0)
                        {
                            // reserve funds
                            updates.Add(new ExBalanceUpdate.Update
                            {
                                Number = updateRequest.Number,
                                WalletId = updateRequest.WalletId,
                                AssetId = updateRequest.AssetId,
                                Amount = updateRequest.Amount,
                                ReserveAmount = updateRequest.ReserveAmount,
                                Result = ExBalanceUpdate.BalanceUpdateResult.LowBalance,
                                ErrorMessage = "Unable to reserve funds, low balance"
                            });
                            status = ExBalanceUpdate.BalanceUpdateResult.LowBalance;
                            continue;
                        }

                        if (balance.ReserveBalance + updateRequest.ReserveAmount < 0)
                        {
                            // unreserve funds
                            updates.Add(new ExBalanceUpdate.Update
                            {
                                Number = updateRequest.Number,
                                WalletId = updateRequest.WalletId,
                                AssetId = updateRequest.AssetId,
                                Amount = updateRequest.Amount,
                                ReserveAmount = updateRequest.ReserveAmount,
                                Result = ExBalanceUpdate.BalanceUpdateResult.LowBalance,
                                ErrorMessage = "Unable to unreserve funds, low balance"
                            });
                            status = ExBalanceUpdate.BalanceUpdateResult.LowReserveBalance;
                            continue;
                        }

                        var (oldBalance, oldReserveBalance) = (balance.Balance, balance.ReserveBalance);
                        balance.Balance += updateRequest.Amount - updateRequest.ReserveAmount;
                        balance.ReserveBalance += updateRequest.ReserveAmount;
                        balance.LastUpdate = now;
                        updates.Add(new ExBalanceUpdate.Update
                        {
                            Number = updateRequest.Number,
                            WalletId = updateRequest.WalletId,
                            AssetId = updateRequest.AssetId,
                            Amount = updateRequest.Amount,
                            ReserveAmount = updateRequest.ReserveAmount,
                            OldBalance = oldBalance,
                            NewBalance = balance.Balance,
                            ReserveOldBalance = oldReserveBalance,
                            ReserveNewBalance = balance.ReserveBalance,
                            Result = ExBalanceUpdate.BalanceUpdateResult.Ok,
                        });
                    }

                    var result = new ExBalanceUpdate
                    {
                        Instance = Program.Settings.InstanceName,
                        Timestamp = now,
                        OperationId = request.OperationId,
                        EventType = request.EventType,
                        Result = status,
                        Updates = updates,
                        Balances = balances.Values.ToList()
                    };

                    if (status == ExBalanceUpdate.BalanceUpdateResult.Ok)
                    {
                        foreach (var balancesValue in balances.Values)
                        {
                            balancesValue.Version++;
                        }

                        context.ProcessedOperations.Add(new ProcessedOperationSqlEntity
                        {
                            OperationId = request.OperationId,
                            ProcessedTime = now,
                            ResponseJson = JsonConvert.SerializeObject(result)
                        });
                        await context.SaveChangesAsync();
                    }

                    return result;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Cannot Process Balance Updates: {requestJson}",
                        JsonConvert.SerializeObject(request));
                    exception.FailActivity();
                    return new ExBalanceUpdate
                    {
                        Instance = Program.Settings.InstanceName,
                        Timestamp = DateTime.Now,
                        OperationId = request.OperationId,
                        Result = ExBalanceUpdate.BalanceUpdateResult.Failed,
                        Updates = new List<ExBalanceUpdate.Update>(),
                        Balances = new List<ExBalance>()
                    };
                }
            }
        }
    }
}