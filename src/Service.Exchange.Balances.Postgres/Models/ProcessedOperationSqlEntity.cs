using System;

namespace Service.Exchange.Balances.Postgres.Models
{
    public class ProcessedOperationSqlEntity
    {
        public string OperationId { get; set; }
        public string ResponseJson { get; set; }
        public DateTime ProcessedTime { get; set; }
    }
}