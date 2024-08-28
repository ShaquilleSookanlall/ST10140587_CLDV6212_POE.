using ST10140587_CLDV6212_POE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ST10140587_CLDV6212_POE.Services;
using System.Linq;
using System.Collections.Generic;

public class TransactionController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly QueueService _queueService;

    public TransactionController(TableStorageService tableStorageService, QueueService queueService)
    {
        _tableStorageService = tableStorageService;
        _queueService = queueService;
    }

    // Action to display all transactions with customer and product names
    public async Task<IActionResult> Index()
    {
        var transactions = await _tableStorageService.GetAllTransactionsAsync();
        var customers = await _tableStorageService.GetAllCustomerAsync();
        var products = await _tableStorageService.GetAllProductsAsync();

        // Create dictionaries for quick lookup
        var customerDictionary = customers.ToDictionary(c => c.RowKey, c => c.Customer_Name);
        var productDictionary = products.ToDictionary(p => p.RowKey, p => p.Product_Name);

        // Update transaction to include customer and product names
        foreach (var transaction in transactions)
        {
            if (!string.IsNullOrEmpty(transaction.Customer_Id) &&
                customerDictionary.TryGetValue(transaction.Customer_Id, out var customerName))
            {
                transaction.Customer_Name = customerName;
            }
            else
            {
                transaction.Customer_Name = "Unknown Customer"; // Fallback or default value
            }

            if (!string.IsNullOrEmpty(transaction.Product_Id) &&
                productDictionary.TryGetValue(transaction.Product_Id, out var productName))
            {
                transaction.Product_Name = productName;
            }
            else
            {
                transaction.Product_Name = "Unknown Product"; // Fallback or default value
            }
        }

        return View(transactions);
    }


    public async Task<IActionResult> Register()
    {
        var customers = await _tableStorageService.GetAllCustomerAsync();
        var products = await _tableStorageService.GetAllProductsAsync();

        // Check for null or empty lists
        if (customers == null || customers.Count == 0)
        {
            // Handle the case where no customers are found
            ModelState.AddModelError("", "No customers found. Please add customers first.");
            return View(); // Or redirect to another action
        }

        if (products == null || products.Count == 0)
        {
            // Handle the case where no products are found
            ModelState.AddModelError("", "No products found. Please add products first.");
            return View(); // Or redirect to another action
        }

        ViewData["CustomersList"] = customers;
        ViewData["ProductsList"] = products;

        return View();
    }

    // Action to handle the form submission and register the transaction
    [HttpPost]
    public async Task<IActionResult> Register(Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            // Set transaction details
            transaction.Transaction_Date = DateTime.SpecifyKind(transaction.Transaction_Date, DateTimeKind.Utc);
            transaction.PartitionKey = "TransactionsPartition";
            transaction.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddTransactionAsync(transaction);

            // Send message to queue
            string message = $"New Transaction by Customer {transaction.Customer_Id} of product {transaction.Product_Id} on {transaction.Transaction_Date}";
            await _queueService.SendMessageAsync(message);

            return RedirectToAction("Index");
        }
        else
        {
            // Log model state errors
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        // Reload customers and products lists if validation fails
        var customers = await _tableStorageService.GetAllCustomerAsync();
        var products = await _tableStorageService.GetAllProductsAsync();
        ViewData["CustomersList"] = customers;
        ViewData["ProductsList"] = products;

        return View(transaction);
    }
}
