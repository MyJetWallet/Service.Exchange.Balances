using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;
using Service.Exchange.Balances.Postgres.Models;
using Service.Exchange.Sdk.Models;

namespace Service.Exchange.Balances.Postgres
{
    public class DatabaseContext : MyDbContext
    {
        public const string Schema = "exchange_balances";

        private const string BalancesTableName = "balances";
        private const string ProcessedOperationsTableName = "processed_operations";

        public DbSet<ExBalance> Balances { get; set; }
        public DbSet<ProcessedOperationSqlEntity> ProcessedOperations { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetBalances(modelBuilder);
            SetProcessedOperations(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void SetBalances(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExBalance>().ToTable(BalancesTableName);
            modelBuilder.Entity<ExBalance>().Property(e => e.WalletIdAssetId).HasMaxLength(260);
            modelBuilder.Entity<ExBalance>().HasKey(e => e.WalletIdAssetId);
            modelBuilder.Entity<ExBalance>().Property(e => e.WalletId).HasMaxLength(128);
            modelBuilder.Entity<ExBalance>().Property(e => e.AssetId).HasMaxLength(128);
            modelBuilder.Entity<ExBalance>().Property(e => e.Balance).HasPrecision(40, 20);
            modelBuilder.Entity<ExBalance>().Property(e => e.ReserveBalance).HasPrecision(40, 20);
            modelBuilder.Entity<ExBalance>().Property(e => e.LastUpdate);
            modelBuilder.Entity<ExBalance>().Property(e => e.Version);
            modelBuilder.Entity<ExBalance>().Property(e => e.ReserveFuturesOrders).HasPrecision(40, 20);
            modelBuilder.Entity<ExBalance>().Property(e => e.ReserveFuturesPositions).HasPrecision(40, 20);

            modelBuilder.Entity<ExBalance>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<ExBalance>().HasIndex(e => new { e.WalletId, e.AssetId }).IsUnique();
        }

        private void SetProcessedOperations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedOperationSqlEntity>().ToTable(ProcessedOperationsTableName);
            modelBuilder.Entity<ProcessedOperationSqlEntity>().Property(e => e.OperationId).HasMaxLength(128);
            modelBuilder.Entity<ProcessedOperationSqlEntity>().HasKey(e => e.OperationId);
            modelBuilder.Entity<ProcessedOperationSqlEntity>().Property(e => e.ResponseJson);
            modelBuilder.Entity<ProcessedOperationSqlEntity>().Property(e => e.ProcessedTime);

            modelBuilder.Entity<ExBalance>().HasIndex(e => new { e.WalletId, e.AssetId }).IsUnique();
        }

        public async Task<int> UpsertRangeAsync(IEnumerable<ExBalance> entities)
        {
            var result = await Balances.UpsertRange(entities).On(e => e.WalletIdAssetId).RunAsync();
            return result;
        }

        public async Task<int> InsertAsync(ProcessedOperationSqlEntity entity)
        {
            var result = await ProcessedOperations.Upsert(entity).On(e => e.OperationId).NoUpdate().RunAsync();
            return result;
        }
    }
}