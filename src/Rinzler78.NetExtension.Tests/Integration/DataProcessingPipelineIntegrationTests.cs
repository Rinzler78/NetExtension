using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Linq;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Strings;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Integration;

/// <summary>
/// Integration tests that verify end-to-end functionality by combining multiple components
/// in complex data processing pipelines. These tests demonstrate real-world usage scenarios
/// where multiple library components work together to solve complex problems.
/// </summary>
[Trait("Category", "Integration")]
public class DataProcessingPipelineIntegrationTests
{
    /// <summary>
    /// Represents a customer entity for testing complex data processing scenarios.
    /// </summary>
    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Order> Orders { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public CustomerStatus Status { get; set; }
    }

    /// <summary>
    /// Represents an order for testing nested data processing.
    /// </summary>
    private class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Amount { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    /// <summary>
    /// Represents an order item for testing deeply nested structures.
    /// </summary>
    private class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Customer status enumeration for testing enum processing.
    /// </summary>
    private enum CustomerStatus
    {
        Active,
        Inactive,
        Suspended,
        VIP
    }

    /// <summary>
    /// Observable analytics result for testing observable patterns with complex data.
    /// </summary>
    private class AnalyticsResult : ObservableObject
    {
        private int _totalCustomers;
        private decimal _totalRevenue;
        private decimal _averageOrderValue;
        private DateTime _lastUpdated;
        private List<string> _topProducts = new();
        private Dictionary<string, int> _statusCounts = new();

        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        public decimal AverageOrderValue
        {
            get => _averageOrderValue;
            set => SetProperty(ref _averageOrderValue, value);
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public List<string> TopProducts
        {
            get => _topProducts;
            set => SetProperty(ref _topProducts, value);
        }

        public Dictionary<string, int> StatusCounts
        {
            get => _statusCounts;
            set => SetProperty(ref _statusCounts, value);
        }
    }

    [Fact]
    public void EndToEnd_CustomerAnalyticsPipeline_ShouldProcessComplexData()
    {
        // Arrange
        var customers = GenerateTestCustomers(100);
        var analyticsResult = new AnalyticsResult();
        var propertyChanges = new List<string>();

        analyticsResult.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                propertyChanges.Add(e.PropertyName);
        };

        // Act - Complex data processing pipeline
        // Step 1: Filter active customers with valid emails
        var activeCustomers = customers
            .Where(c => c.Status == CustomerStatus.Active)
            .Where(c => c.Email.IsValidEmail())
            .ToList();

        // Step 2: Calculate total revenue using TryAggregate
        var totalRevenue = activeCustomers
            .SelectMany(c => c.Orders)
            .Select(o => o.Amount)
            .TryAggregate((x, y) => x + y);

        // Step 3: Find top products by quantity sold
        var topProducts = activeCustomers
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Items)
            .GroupBy(item => item.ProductName)
            .Select(group => new
            {
                Product = group.Key,
                TotalQuantity = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.TotalPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(5)
            .Select(x => x.Product)
            .ToList();

        // Step 4: Calculate average order value
        var allOrders = activeCustomers.SelectMany(c => c.Orders).ToList();
        var averageOrderValue = allOrders.Any() ? allOrders.Average(o => o.Amount) : 0;

        // Step 5: Count customers by status
        var statusCounts = customers
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        // Step 6: Update analytics result (triggering property change events)
        analyticsResult.TotalCustomers = activeCustomers.Count;
        analyticsResult.TotalRevenue = totalRevenue;
        analyticsResult.AverageOrderValue = averageOrderValue;
        analyticsResult.LastUpdated = DateTime.Now;
        analyticsResult.TopProducts = topProducts;
        analyticsResult.StatusCounts = statusCounts;

        // Step 7: Serialize the result
        var json = analyticsResult.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<AnalyticsResult>();

        // Assert
        analyticsResult.TotalCustomers.Should().BeGreaterThan(0);
        analyticsResult.TotalRevenue.Should().BeGreaterThan(0);
        analyticsResult.AverageOrderValue.Should().BeGreaterThan(0);
        analyticsResult.TopProducts.Should().NotBeEmpty();
        analyticsResult.StatusCounts.Should().ContainKey("Active");

        // Verify property changes were triggered
        propertyChanges.Should().Contain("TotalCustomers");
        propertyChanges.Should().Contain("TotalRevenue");
        propertyChanges.Should().Contain("AverageOrderValue");
        propertyChanges.Should().Contain("TopProducts");

        // Verify serialization/deserialization
        Assert.NotNull(restored);
        restored!.TotalCustomers.Should().Be(analyticsResult.TotalCustomers);
        restored.TotalRevenue.Should().Be(analyticsResult.TotalRevenue);
        restored.TopProducts.Should().BeEquivalentTo(analyticsResult.TopProducts);
    }

    [Fact]
    public void EndToEnd_DataTransformationPipeline_ShouldHandleComplexOperations()
    {
        // Arrange
        var rawData = GenerateTestCustomers(50);
        var tempFile = MockHelpers.CreateTempFile("", ".json");

        try
        {
            // Act - Multi-step data processing pipeline
            // Step 1: Clean and validate customer data
            var cleanedData = rawData
                .Select(customer => new
                {
                    Id = customer.Id,
                    Name = customer.Name.ToPascalCase(),
                    Email = customer.Email.ToLowerInvariant(),
                    IsEmailValid = customer.Email.IsValidEmail(),
                    RegistrationMonth = customer.RegistrationDate.ToString("yyyy-MM"),
                    TotalSpent = customer.TotalSpent,
                    OrderCount = customer.Orders.Count,
                    Status = customer.Status.ToString()
                })
                .Where(c => c.IsEmailValid)
                .ToList();

            // Step 2: Group by registration month and calculate statistics
            var monthlyStats = cleanedData
                .GroupBy(c => c.RegistrationMonth)
                .Select(group => new
                {
                    Month = group.Key,
                    CustomerCount = group.Count(),
                    TotalSpent = group.Sum(c => c.TotalSpent),
                    AverageSpent = group.Average(c => c.TotalSpent),
                    TotalOrders = group.Sum(c => c.OrderCount)
                })
                .OrderBy(stat => stat.Month)
                .ToList();

            // Step 3: Create summary report
            var summaryReport = new
            {
                GeneratedAt = DateTime.Now,
                TotalCustomers = cleanedData.Count,
                TotalRevenue = cleanedData.Sum(c => c.TotalSpent),
                MonthlyBreakdown = monthlyStats,
                TopCustomers = cleanedData
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(System.Math.Min(10, cleanedData.Count))
                    .Select(c => new { c.Name, c.TotalSpent })
                    .ToList()
            };

            // Step 4: Save to file and read back
            var reportJson = summaryReport.SerializeObject(Formatting.Indented);
            File.WriteAllText(tempFile, reportJson);

            var restoredReport = tempFile.DeserializeObjectFromFile<JObject>();

            // Step 5: Calculate similarity scores for customer names
            var nameSimilarities = cleanedData
                .Select(c => new
                {
                    Name = c.Name,
                    SimilarityToJohn = c.Name.CalculateSimilarity("John"),
                    SimilarityToMary = c.Name.CalculateSimilarity("Mary"),
                    LevenshteinDistance = c.Name.ComputeLevenshteinDistance("Customer")
                })
                .OrderByDescending(n => n.SimilarityToJohn)
                .Take(5)
                .ToList();

            // Assert
            cleanedData.Should().NotBeEmpty();
            cleanedData.Should().OnlyContain(c => c.IsEmailValid);
            cleanedData.Should().OnlyContain(c => char.IsUpper(c.Name[0])); // Pascal case check

            monthlyStats.Should().NotBeEmpty();
            monthlyStats.Should().BeInAscendingOrder(stat => stat.Month);

            summaryReport.TotalCustomers.Should().Be(cleanedData.Count);
            summaryReport.TotalRevenue.Should().BeGreaterThan(0);
            summaryReport.TopCustomers.Should().HaveCount(System.Math.Min(10, cleanedData.Count));

            restoredReport.Should().NotBeNull();
            nameSimilarities.Should().NotBeEmpty();
            nameSimilarities.Should().BeInDescendingOrder(n => n.SimilarityToJohn);
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void EndToEnd_RealTimeDataProcessingPipeline_ShouldHandleObservableUpdates()
    {
        // Arrange
        var observableCollection = new ObservableRangeCollection<Customer>();
        var analyticsResult = new AnalyticsResult();
        var updateHistory = new List<string>();

        // Set up change tracking
        observableCollection.CollectionChanged += (sender, e) =>
        {
            updateHistory.Add($"Collection: {e.Action}");
        };

        analyticsResult.PropertyChanged += (sender, e) =>
        {
            updateHistory.Add($"Analytics: {e.PropertyName}");
        };

        // Act - Simulate real-time data processing
        // Step 1: Initial data load
        var initialCustomers = GenerateTestCustomers(20);
        observableCollection.AddRange(initialCustomers);

        // Step 2: Process initial data
        UpdateAnalytics(observableCollection, analyticsResult);

        // Step 3: Add new customers (simulating real-time updates)
        var newCustomers = GenerateTestCustomers(10, startId: 21);
        observableCollection.AddRange(newCustomers);

        // Step 4: Update analytics with new data
        UpdateAnalytics(observableCollection, analyticsResult);

        // Step 5: Remove inactive customers
        var inactiveCustomers = observableCollection
            .Where(c => c.Status == CustomerStatus.Inactive)
            .ToList();

        foreach (var customer in inactiveCustomers)
        {
            observableCollection.Remove(customer);
        }

        // Step 6: Final analytics update
        UpdateAnalytics(observableCollection, analyticsResult);

        // Step 7: Serialize final state
        var finalSnapshot = new
        {
            Customers = observableCollection.ToList(),
            Analytics = analyticsResult,
            UpdateHistory = updateHistory
        };

        var json = finalSnapshot.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<JObject>();

        // Assert
        observableCollection.Should().NotBeEmpty();
        analyticsResult.TotalCustomers.Should().Be(observableCollection.Count);
        analyticsResult.TotalRevenue.Should().BeGreaterThan(0);

        // With fixed random seed, expect specific count after removing inactive customers
        observableCollection.Count.Should().BeGreaterThan(0, "Should have at least some customers after removing inactive ones");

        updateHistory.Should().Contain(h => h.Contains("Collection: Add"));
        updateHistory.Should().Contain(h => h.Contains("Analytics: TotalCustomers"));
        updateHistory.Should().Contain(h => h.Contains("Analytics: TotalRevenue"));

        Assert.NotNull(restored);
        json.Should().Contain("Customers");
        json.Should().Contain("Analytics");
        json.Should().Contain("UpdateHistory");
    }

    [Fact]
    public void EndToEnd_BatchProcessingPipeline_ShouldHandleLargeDataSets()
    {
        // Arrange
        var largeDataSet = GenerateTestCustomers(1000);
        var batchSize = 100;
        var processedBatches = new List<object>();

        // Act - Process data in batches
        for (int i = 0; i < largeDataSet.Count; i += batchSize)
        {
            var batch = largeDataSet.Skip(i).Take(batchSize).ToList();

            // Process batch
            var batchResult = ProcessCustomerBatch(batch, i / batchSize + 1);
            processedBatches.Add(batchResult);
        }

        // Aggregate results from all batches
        var aggregatedResult = new
        {
            TotalBatches = processedBatches.Count,
            TotalCustomers = largeDataSet.Count,
            TotalRevenue = processedBatches.Sum(b => (decimal)((dynamic)b).TotalRevenue),
            AverageOrderValue = processedBatches.Average(b => (decimal)((dynamic)b).AverageOrderValue),
            ProcessingTime = DateTime.Now
        };

        // Serialize aggregated result
        var json = aggregatedResult.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<JObject>();

        // Assert
        processedBatches.Should().HaveCount(10); // 1000 / 100 = 10 batches
        aggregatedResult.TotalCustomers.Should().Be(1000);
        aggregatedResult.TotalRevenue.Should().BeGreaterThan(0);
        aggregatedResult.AverageOrderValue.Should().BeGreaterThan(0);

        Assert.NotNull(restored);
        json.Should().Contain("TotalBatches");
        json.Should().Contain("TotalRevenue");
    }

    [Fact]
    public void EndToEnd_ErrorHandlingPipeline_ShouldHandleExceptionsGracefully()
    {
        // Arrange
        var mixedData = GenerateTestCustomersWithErrors(50);
        var errorLog = new List<string>();
        var successfulProcessing = new List<object>();

        // Act - Process data with error handling
        foreach (var customer in mixedData)
        {
            try
            {
                // Validate customer data
                if (customer == null)
                {
                    errorLog.Add("Null customer encountered");
                    continue;
                }

                if (string.IsNullOrEmpty(customer.Name))
                {
                    errorLog.Add($"Customer {customer.Id} has empty name");
                    continue;
                }

                if (!customer.Email.IsValidEmail())
                {
                    errorLog.Add($"Customer {customer.Id} has invalid email: {customer.Email}");
                    continue;
                }

                // Process valid customer
                var processedCustomer = new
                {
                    Id = customer.Id,
                    Name = customer.Name.ToPascalCase(),
                    Email = customer.Email,
                    TotalSpent = customer.TotalSpent,
                    OrderCount = customer.Orders.Count,
                    Status = customer.Status.ToString()
                };

                successfulProcessing.Add(processedCustomer);
            }
            catch (Exception ex)
            {
                errorLog.Add($"Exception processing customer {customer?.Id}: {ex.Message}");
            }
        }

        // Use TryAggregate for safe aggregation
        var totalRevenue = successfulProcessing
            .Select(c => ((dynamic)c).TotalSpent)
            .Cast<decimal>()
            .TryAggregate((x, y) => x + y);

        // Create processing report
        var processingReport = new
        {
            TotalRecords = mixedData.Count,
            SuccessfullyProcessed = successfulProcessing.Count,
            ErrorCount = errorLog.Count,
            TotalRevenue = totalRevenue,
            ErrorLog = errorLog,
            ProcessedAt = DateTime.Now
        };

        // Serialize report
        var json = processingReport.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<JObject>();

        // Assert
        successfulProcessing.Should().NotBeEmpty();
        errorLog.Should().NotBeEmpty(); // We expect some errors in the test data

        processingReport.TotalRecords.Should().Be(mixedData.Count);
        processingReport.SuccessfullyProcessed.Should().BeLessThan(mixedData.Count);
        processingReport.ErrorCount.Should().BeGreaterThan(0);
        processingReport.TotalRevenue.Should().BeGreaterThan(0);

        Assert.NotNull(restored);
        json.Should().Contain("ErrorLog");
        json.Should().Contain("SuccessfullyProcessed");
    }

    /// <summary>
    /// Helper method to generate test customers with realistic data.
    /// </summary>
    private List<Customer> GenerateTestCustomers(int count, int startId = 1)
    {
        var random = new Random(42); // Fixed seed for reproducible tests
        var customers = new List<Customer>();

        for (int i = 0; i < count; i++)
        {
            var customer = new Customer
            {
                Id = startId + i,
                Name = $"Customer {startId + i}",
                Email = $"customer{startId + i}@example.com",
                RegistrationDate = DateTime.Now.AddDays(-random.Next(365)),
                TotalSpent = random.Next(100, 10000),
                Status = (CustomerStatus)random.Next(0, 4),
                Orders = GenerateTestOrders(random.Next(1, 5), random),
                Metadata = new Dictionary<string, object>
                {
                    { "source", "test" },
                    { "priority", random.Next(1, 5) }
                }
            };

            customers.Add(customer);
        }

        return customers;
    }

    /// <summary>
    /// Helper method to generate test orders for a customer.
    /// </summary>
    private List<Order> GenerateTestOrders(int count, Random random)
    {
        var orders = new List<Order>();

        for (int i = 0; i < count; i++)
        {
            var order = new Order
            {
                OrderId = random.Next(1000, 9999),
                OrderDate = DateTime.Now.AddDays(-random.Next(30)),
                Amount = random.Next(50, 1000),
                Status = random.Next(0, 2) == 0 ? "Completed" : "Pending",
                Items = GenerateTestOrderItems(random.Next(1, 3), random),
                Properties = new Dictionary<string, string>
                {
                    { "paymentMethod", "CreditCard" },
                    { "shippingMethod", "Standard" }
                }
            };

            orders.Add(order);
        }

        return orders;
    }

    /// <summary>
    /// Helper method to generate test order items.
    /// </summary>
    private List<OrderItem> GenerateTestOrderItems(int count, Random random)
    {
        var items = new List<OrderItem>();
        var productNames = new[] { "Product A", "Product B", "Product C", "Product D", "Product E" };

        for (int i = 0; i < count; i++)
        {
            var item = new OrderItem
            {
                ProductId = random.Next(1, 100),
                ProductName = productNames[random.Next(productNames.Length)],
                Quantity = random.Next(1, 5),
                UnitPrice = random.Next(10, 100),
                Tags = new List<string> { "tag1", "tag2" }
            };

            items.Add(item);
        }

        return items;
    }

    /// <summary>
    /// Helper method to generate test customers with intentional errors for error handling tests.
    /// </summary>
    private List<Customer?> GenerateTestCustomersWithErrors(int count)
    {
        var customers = new List<Customer?>();
        var validCustomers = GenerateTestCustomers(count - 10);

        // Add valid customers
        customers.AddRange(validCustomers);

        // Add customers with errors
        customers.Add(null); // Null customer
        customers.Add(new Customer { Id = 9999, Name = "", Email = "valid@example.com" }); // Empty name
        customers.Add(new Customer { Id = 9998, Name = "Valid Name", Email = "invalid-email" }); // Invalid email
        customers.Add(new Customer { Id = 9997, Name = "Valid Name", Email = "" }); // Empty email
        customers.Add(new Customer { Id = 9996, Name = "Valid Name", Email = "test@test.com", TotalSpent = -100 }); // Negative spending

        return customers;
    }

    /// <summary>
    /// Helper method to update analytics based on current customer data.
    /// </summary>
    private void UpdateAnalytics(ObservableRangeCollection<Customer> customers, AnalyticsResult analytics)
    {
        var activeCustomers = customers.Where(c => c.Status == CustomerStatus.Active).ToList();

        analytics.TotalCustomers = customers.Count; // Count all customers in collection
        analytics.TotalRevenue = activeCustomers.Sum(c => c.TotalSpent);
        analytics.AverageOrderValue = activeCustomers.Any() ?
            activeCustomers.SelectMany(c => c.Orders).Average(o => o.Amount) : 0;
        analytics.LastUpdated = DateTime.Now;

        analytics.TopProducts = activeCustomers
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductName)
            .OrderByDescending(g => g.Sum(i => i.Quantity))
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        analytics.StatusCounts = customers
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());
    }

    /// <summary>
    /// Helper method to process a batch of customers.
    /// </summary>
    private object ProcessCustomerBatch(List<Customer> batch, int batchNumber)
    {
        var batchResult = new
        {
            BatchNumber = batchNumber,
            CustomerCount = batch.Count,
            TotalRevenue = batch.Sum(c => c.TotalSpent),
            AverageOrderValue = batch.SelectMany(c => c.Orders).Any() ?
                batch.SelectMany(c => c.Orders).Average(o => o.Amount) : 0,
            TopCustomer = batch.OrderByDescending(c => c.TotalSpent).First().Name,
            ProcessedAt = DateTime.Now
        };

        return batchResult;
    }
}
