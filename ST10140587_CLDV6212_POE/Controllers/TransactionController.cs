using ST10140587_CLDV6212_POE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ST10140587_CLDV6212_POE.Services;

public class TransactionController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly QueueService _queueService;

    public TransactionController(TableStorageService tableStorageService, QueueService queueService)
    {
        _tableStorageService = tableStorageService;
        _queueService = queueService;
    }

    public async Task<IActionResult> Index()
    {
        var transactions = await _tableStorageService.GetAllTransactionsAsync();
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

        ViewData["Customer"] = customers;
        ViewData["Products"] = products;

        return View();
    }



    // Action to handle the form submission and register the sighting
    [HttpPost]
    public async Task<IActionResult> Register(Transaction transaction)
    {
        if (ModelState.IsValid)
        {//TableService
            transaction.Transaction_Date = DateTime.SpecifyKind(transaction.Transaction_Date, DateTimeKind.Utc);
            transaction.PartitionKey = "TransactionsPartition";
            transaction.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddTransactionAsync(transaction);
            //MessageQueue
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
        ViewData["Customers"] = customers;
        ViewData["Products"] = products;

        return View(transaction);
    }

}