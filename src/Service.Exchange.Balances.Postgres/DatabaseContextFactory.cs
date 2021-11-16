using Microsoft.EntityFrameworkCore;

namespace Service.Exchange.Balances.Postgres
{
    public class DatabaseContextFactory
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public DatabaseContextFactory(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public DatabaseContext Create()
        {
            return new DatabaseContext(_dbContextOptionsBuilder.Options);
        }
    }
}