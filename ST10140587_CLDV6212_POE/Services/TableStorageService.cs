using Azure;
using Azure.Data.Tables;
using ST10140587_CLDV6212_POE.Models;
using System.Threading.Tasks;

public class TableStorageService
{
    private readonly TableClient _tableClient;
    private readonly TableClient _customerTableClient;
    private readonly TableClient _transactionTableClient;

    public TableStorageService(string connectionString)
    {
        _tableClient = new TableClient(connectionString, "Products");
        _customerTableClient = new TableClient(connectionString, "Customers");
        _transactionTableClient = new TableClient(connectionString, "Transactions");
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();

        await foreach (var product in _tableClient.QueryAsync<Product>())
        {
            products.Add(product);
        }

        return products;
    }

    public async Task AddProductAsync(Product product)
    {
        // Ensure PartitionKey and RowKey are set
        if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _tableClient.AddEntityAsync(product);
        }
        catch (RequestFailedException ex)
        {
            // Handle exception as necessary, for example log it or rethrow
            throw new InvalidOperationException("Error adding entity to Table Storage", ex);
        }
    }

    public async Task DeleteProductAsync(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Handle not found
            return null;
        }
    }
    public async Task<List<Customer>> GetAllCustomerAsync()
    {
        var customers = new List<Customer>();

        await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
        {
            customers.Add(customer);
        }

        return customers;
    }
    public async Task AddCustomerAsync(Customer customer)
    {
        if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _customerTableClient.AddEntityAsync(customer);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding entity to Table Storage", ex);
        }
    }

    public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
    {
        await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        if (string.IsNullOrEmpty(transaction.PartitionKey) || string.IsNullOrEmpty(transaction.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _transactionTableClient.AddEntityAsync(transaction);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding sighting to Table Storage", ex);
        }
    }

    
    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        var transactions = new List<Transaction>();

        await foreach (var transaction in _transactionTableClient.QueryAsync<Transaction>())
        {
            transactions.Add(transaction);
        }

        return transactions;
    }
}


