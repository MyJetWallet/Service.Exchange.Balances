using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Newtonsoft.Json;
using Service.Exchange.Balances.Domain;
using Service.Exchange.Balances.Postgres;
using Service.Exchange.Balances.Postgres.Models;
using Service.Exchange.Sdk.Messages;
using Service.Exchange.Sdk.Models;

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
                    var now = DateTime.Now;
                    foreach (var updateRequest in request.Updates)
                    {
                        var balance =
                            balances.GetValueOrDefault($"{updateRequest.WalletId}::{updateRequest.AssetId}") ??
                            await context.Balances
                                .Where(e => e.WalletId == updateRequest.WalletId && e.AssetId == updateRequest.AssetId)
                                .SingleOrDefaultAsync() ??
                            new ExBalance
                            {
                                WalletId = updateRequest.WalletId,
                                AssetId = updateRequest.AssetId,
                                Balance = 0,
                                ReserveBalance = 0,
                                LastUpdate = now,
                                Version = 0
                            };

                        if (updateRequest.Amount < 0 && balance.Balance < -updateRequest.Amount)
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

                        if (updateRequest.ReserveAmount > 0 && balance.Balance < updateRequest.ReserveAmount)
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

                        if (updateRequest.ReserveAmount < 0 && balance.ReserveBalance < -updateRequest.ReserveAmount)
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
                            status = ExBalanceUpdate.BalanceUpdateResult.LowBalance;
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

                        balances.TryAdd($"{updateRequest.WalletId}::{updateRequest.AssetId}", balance);
                    }

                    var result = new ExBalanceUpdate
                    {
                        Instance = Program.Settings.InstanceName,
                        Timestamp = now,
                        OperationId = request.OperationId,
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
                        await context.UpsertRangeAsync(balances.Values);
                        await context.InsertAsync(new ProcessedOperationSqlEntity
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
                    _logger.LogError(exception, "Cannot Process Balance Updates");
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