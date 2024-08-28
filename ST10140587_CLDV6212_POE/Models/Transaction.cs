using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ST10140587_CLDV6212_POE.Models
{
    public class Transaction : ITableEntity
    {
        [Key]
        public int transaction_Id { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        //Introduce validation sample
        [Required(ErrorMessage = "Please select a customer")]
        public int Customer_Id { get; set; }

        [Required(ErrorMessage = "Please select a product")]
        public int Product_Id { get; set; }

        [Required(ErrorMessage ="Please select a date")]
        public DateTime Transaction_Date { get; set; }
    }
}
