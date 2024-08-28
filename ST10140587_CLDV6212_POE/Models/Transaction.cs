using Azure;
using Azure.Data.Tables;
using System;

namespace ST10140587_CLDV6212_POE.Models
{
    public class Transaction : ITableEntity
    {
        public string Customer_Id { get; set; }
        public string Product_Id { get; set; }
        public DateTime Transaction_Date { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        // Additional properties for displaying names
        public string Customer_Name { get; set; }
        public string Product_Name { get; set; }
    }
}
