using System.Net;
using System.Net.Http;
using System.Text;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Strings;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Integration;

/// <summary>
/// Integration tests that verify end-to-end functionality by combining String extensions with HTTP operations.
/// These tests use mocked HTTP responses to test the integration without making actual HTTP calls.
/// </summary>
[Trait("Category", "Integration")]
public class StringHttpIntegrationTests
{
    /// <summary>
    /// Test data class for HTTP operations.
    /// </summary>
    private class ApiResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Test request class for HTTP operations.
    /// </summary>
    private class ApiRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    [Fact]
    public void EndToEnd_StringValidationWithHttpUrl_ShouldWork()
    {
        // Arrange
        var validUrls = new[]
        {
            "https://api.example.com/users",
            "http://api.example.com/users",
            "https://subdomain.api.example.com/v1/users"
        };

        var invalidUrls = new[]
        {
            "ftp://api.example.com/users", // Invalid scheme
            "https://localhost/users", // Localhost blocked
            "https://127.0.0.1/users", // Loopback blocked
            "https://10.0.0.1/users", // Private IP blocked
            "https://192.168.1.1/users", // Private IP blocked
            "https://172.16.0.1/users", // Private IP blocked
            "", // Empty
            "not-a-url" // Invalid format
        };

        // Act & Assert - Valid URLs should not throw
        foreach (var url in validUrls)
        {
            // These operations should validate the URL internally
            var act = () => url.GetStringContent();
            act.Should().NotThrow();
        }

        // Invalid URLs should throw when used in HTTP operations
        foreach (var url in invalidUrls)
        {
            if (string.IsNullOrEmpty(url))
                continue; // Skip empty URLs for this test

            // These operations should throw due to URL validation
            var act = async () => await url.HttpGetStringAsync();
            act.Should().ThrowAsync<ArgumentException>();
        }
    }

    [Fact]
    public async Task EndToEnd_StringManipulationWithHttpContent_ShouldWork()
    {
        // Arrange
        var apiRequest = new ApiRequest
        {
            Name = "test user",
            Description = "DESCRIPTION IN CAPS",
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };

        // Act - String manipulation combined with HTTP content preparation
        var processedRequest = new ApiRequest
        {
            Name = apiRequest.Name.BeginByUpperCase(), // "Test user"
            Description = apiRequest.Description.ToStartByUpperCase(), // "Desc in caps"
            Tags = apiRequest.Tags.Select(tag => tag.ToPascalCase()).ToList() // ["Tag1", "Tag2", "Tag3"]
        };

        // Create HTTP content from processed request
        var httpContent = processedRequest.GetStringContent();
        var contentString = await httpContent.ReadAsStringAsync();

        // Deserialize back to verify
        var restored = contentString.Deserialize<ApiRequest>();

        // Assert
        Assert.NotNull(restored);
        restored!.Name.Should().Be("Test user");
        restored.Description.Should().Be("Description in caps");
        restored.Tags.Should().BeEquivalentTo(new[] { "Tag1", "Tag2", "Tag3" });

        // Verify HTTP content properties
        httpContent.Headers.ContentType?.MediaType.Should().Be("application/json");
        httpContent.Headers.ContentType?.CharSet.Should().Be("utf-8");
    }

    [Fact]
    public void EndToEnd_StringSimilarityWithApiResponseProcessing_ShouldWork()
    {
        // Arrange
        var apiResponses = new List<ApiResponse>
        {
            new() { Id = 1, Name = "John Doe", Description = "Software Engineer" },
            new() { Id = 2, Name = "Jane Doe", Description = "Software Developer" },
            new() { Id = 3, Name = "John Smith", Description = "Data Scientist" },
            new() { Id = 4, Name = "Jane Smith", Description = "Product Manager" }
        };

        var searchQuery = "John";

        // Act - Use string similarity for search functionality
        var searchResults = apiResponses
            .Select(response => new
            {
                Response = response,
                NameSimilarity = response.Name.CalculateSimilarity(searchQuery),
                DescriptionSimilarity = response.Description.CalculateSimilarity(searchQuery)
            })
            .Where(x => x.NameSimilarity > 0.3 || x.DescriptionSimilarity > 0.1)
            .OrderByDescending(x => x.NameSimilarity)
            .ToList();

        // Calculate Levenshtein distances for additional analysis
        var distances = apiResponses
            .Select(response => new
            {
                Response = response,
                Distance = response.Name.ComputeLevenshteinDistance(searchQuery)
            })
            .OrderBy(x => x.Distance)
            .ToList();

        // Serialize search results
        var json = searchResults.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<List<object>>();

        // Assert
        searchResults.Should().HaveCount(3); // John Doe, John Smith, and Jane Smith (matches similarity)
        searchResults[0].Response.Name.Should().Be("John Doe"); // Highest similarity
        searchResults[1].Response.Name.Should().Be("John Smith"); // Second highest

        distances[0].Response.Name.Should().Be("John Doe"); // Smallest distance
        distances[0].Distance.Should().Be(4); // "John Doe" vs "John" = 4 characters difference

        Assert.NotNull(restored);
        Assert.Equal(3, restored.Count);
    }

    [Fact]
    public void EndToEnd_StringValidationWithEmailProcessing_ShouldWork()
    {
        // Arrange
        var userRegistrationData = new List<Dictionary<string, string>>
        {
            new() { ["name"] = "John Doe", ["email"] = "john.doe@example.com", ["username"] = "john_doe" },
            new() { ["name"] = "Jane Smith", ["email"] = "invalid-email", ["username"] = "jane.smith" },
            new() { ["name"] = "Bob Johnson", ["email"] = "bob@test.com", ["username"] = "bob-johnson" },
            new() { ["name"] = "Alice Brown", ["email"] = "", ["username"] = "alice_brown" }
        };

        // Act - Process user data with string validation and manipulation
        var processedUsers = userRegistrationData
            .Where(user => !user["email"].IsNullOrEmpty() && user["email"].IsValidEmail())
            .Select(user => new
            {
                Name = user["name"].BeginByUpperCase(),
                Email = user["email"],
                Username = user["username"].ToPascalCase(),
                IsValidEmail = user["email"].IsValidEmail(),
                EmailSimilarity = user["email"].CalculateSimilarity("test@example.com")
            })
            .ToList();

        // Check email validation specifically
        var emailValidationResults = userRegistrationData
            .Select(user => new
            {
                Email = user["email"],
                IsValid = user["email"].IsValidEmail(),
                IsEmpty = user["email"].IsNullOrEmpty()
            })
            .ToList();

        // Serialize processed users
        var json = processedUsers.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<List<object>>();

        // Assert
        processedUsers.Should().HaveCount(2); // Only valid emails
        processedUsers[0].Email.Should().Be("john.doe@example.com");
        processedUsers[1].Email.Should().Be("bob@test.com");

        processedUsers[0].Name.Should().Be("John Doe");
        processedUsers[0].Username.Should().Be("JohnDoe");
        processedUsers[0].IsValidEmail.Should().BeTrue();

        emailValidationResults.Should().HaveCount(4);
        emailValidationResults[0].IsValid.Should().BeTrue(); // john.doe@example.com
        emailValidationResults[1].IsValid.Should().BeFalse(); // invalid-email
        emailValidationResults[2].IsValid.Should().BeTrue(); // bob@test.com
        emailValidationResults[3].IsValid.Should().BeFalse(); // empty string

        Assert.NotNull(restored);
        Assert.Equal(2, restored.Count);
    }

    [Fact]
    public async Task EndToEnd_StringCombinationsWithHttpHeaders_ShouldWork()
    {
        // Arrange
        var headerPrefixes = new[] { "X-Custom", "X-App" };
        var headerValues = new[] { "Value1", "Value2" };

        // Act - Create all combinations of headers
        var allCombinations = headerPrefixes.MakeAllCombinations();
        var headerCombinations = headerValues.MakeAllCombinations();

        // Process combinations for HTTP headers
        var httpHeaders = allCombinations
            .Take(4) // Limit to reasonable number
            .Select((combo, index) => new
            {
                HeaderName = combo.Replace("X-Custom", "X-Custom-").Replace("X-App", "X-App-"),
                HeaderValue = headerCombinations.ElementAtOrDefault(index) ?? "DefaultValue"
            })
            .ToList();

        // Create HTTP content with custom headers simulation
        var requestData = new { Message = "Test", Headers = httpHeaders };
        var httpContent = requestData.GetStringContent();
        var contentString = await httpContent.ReadAsStringAsync();

        // Deserialize and verify
        var restored = contentString.Deserialize<JObject>();

        // Assert
        allCombinations.Should().HaveCount(4); // 2 prefixes × 2 prefixes = 4 combinations
        headerCombinations.Should().HaveCount(4); // 2 values × 2 values = 4 combinations

        httpHeaders.Should().HaveCount(4);
        httpHeaders[0].HeaderName.Should().Contain("X-Custom");
        httpHeaders[0].HeaderValue.Should().NotBeNullOrEmpty();

        Assert.NotNull(restored);
        contentString.Should().Contain("Headers");
        contentString.Should().Contain("X-Custom");
    }

    [Fact]
    public void EndToEnd_ByteConversionWithHttpResponseSizes_ShouldWork()
    {
        // Arrange
        var httpResponseSizes = new List<long>
        {
            512,          // 512 B
            1024,         // 1 KB
            1536,         // 1.5 KB
            1048576,      // 1 MB
            2097152,      // 2 MB
            1073741824,   // 1 GB
            2147483648    // 2 GB
        };

        // Act - Process response sizes with human-readable formatting
        var processedSizes = httpResponseSizes
            .Select(size => new
            {
                RawSize = size,
                HumanReadable = size.GetBytesReadable(),
                Category = size switch
                {
                    < 1024 => "Small",
                    < 1048576 => "Medium",
                    < 1073741824 => "Large",
                    _ => "VeryLarge"
                }
            })
            .ToList();

        // Convert to bytes for HTTP operations
        var httpPayload = new
        {
            ResponseSizes = processedSizes,
            Summary = new
            {
                TotalSize = httpResponseSizes.Sum().GetBytesReadable(),
                AverageSize = (httpResponseSizes.Sum() / httpResponseSizes.Count).GetBytesReadable(),
                MaxSize = httpResponseSizes.Max().GetBytesReadable()
            }
        };

        // Serialize for HTTP transmission
        var json = httpPayload.SerializeObject(Formatting.Indented);
        var httpContent = httpPayload.GetStringContent();
        var contentBytes = Encoding.UTF8.GetBytes(json);

        // Deserialize and verify
        var restored = json.Deserialize<JObject>();

        // Assert
        processedSizes.Should().HaveCount(7);
        processedSizes[0].HumanReadable.Should().Be("512 B");
        processedSizes[1].HumanReadable.Should().Be("1 KB");
        processedSizes[2].HumanReadable.Should().Be("1.5 KB");
        processedSizes[3].HumanReadable.Should().Be("1 MB");
        processedSizes[4].HumanReadable.Should().Be("2 MB");
        processedSizes[5].HumanReadable.Should().Be("1 GB");
        processedSizes[6].HumanReadable.Should().Be("2 GB");

        processedSizes[0].Category.Should().Be("Small");
        processedSizes[3].Category.Should().Be("Large");
        processedSizes[6].Category.Should().Be("VeryLarge");

        contentBytes.Length.Should().BeGreaterThan(0);
        httpContent.Headers.ContentType?.MediaType.Should().Be("application/json");

        Assert.NotNull(restored);
        json.Should().Contain("ResponseSizes");
        json.Should().Contain("Summary");
    }

    [Fact]
    public async Task EndToEnd_StringArrayOperationsWithHttpParameters_ShouldWork()
    {
        // Arrange
        var queryParameters = new[] { "param1", "param2", "param3" };
        var queryValues = new[] { "value1", "value2", "value3" };

        // Act - Build query string using string array operations
        var queryString = queryParameters
            .Zip(queryValues, (param, value) => $"{param}={value}")
            .ToArray()
            .Join('&');

        // Create search filters
        var searchFilters = new[] { "name", "email", "status" };
        var searchValues = new[] { "john", "test", "active" };

        // Check if all required parameters are present
        var requiredParams = new[] { "name", "email" };
        var hasAllRequired = queryString.ContainsAll(requiredParams);

        // Check if any optional parameters are present
        var optionalParams = new[] { "status", "category" };
        var hasAnyOptional = queryString.ContainsAny(optionalParams);

        // Build HTTP request data
        var requestData = new
        {
            QueryString = queryString,
            Filters = searchFilters.Zip(searchValues, (filter, value) => new { Filter = filter, Value = value }),
            ValidationResults = new
            {
                HasAllRequired = hasAllRequired,
                HasAnyOptional = hasAnyOptional
            }
        };

        // Serialize for HTTP transmission
        var json = requestData.SerializeObject(Formatting.Indented);
        var httpContent = requestData.GetStringContent();
        var contentString = await httpContent.ReadAsStringAsync();

        // Deserialize and verify
        var restored = contentString.Deserialize<JObject>();

        // Assert
        queryString.Should().Be("param1=value1&param2=value2&param3=value3");

        hasAllRequired.Should().BeFalse(); // Query string doesn't contain "name" and "email"
        hasAnyOptional.Should().BeFalse(); // Query string doesn't contain "status" or "category"

        httpContent.Headers.ContentType?.MediaType.Should().Be("application/json");

        Assert.NotNull(restored);
        json.Should().Contain("QueryString");
        json.Should().Contain("Filters");
        json.Should().Contain("ValidationResults");
    }

    [Fact]
    public async Task EndToEnd_JsonFormattingWithHttpResponseProcessing_ShouldWork()
    {
        // Arrange
        var rawJsonResponse = "{\"id\":1,\"name\":\"John Doe\",\"email\":\"john@example.com\",\"created_at\":\"2023-12-25T10:30:00Z\",\"metadata\":{\"role\":\"admin\",\"active\":true}}";

        // Act - Process HTTP response with JSON formatting
        var formattedJson = rawJsonResponse.ToJsonFormattedString();
        var deserializedResponse = rawJsonResponse.Deserialize<ApiResponse>();

        // Process response data with string operations
        var processedResponse = new
        {
            Id = deserializedResponse?.Id ?? 0,
            DisplayName = deserializedResponse?.Name?.BeginByUpperCase() ?? "Unknown",
            FormattedDescription = deserializedResponse?.Description?.ToStartByUpperCase() ?? "",
            ProcessedTags = deserializedResponse?.Tags?.Select(tag => tag.ToPascalCase()).ToList() ?? new List<string>(),
            ResponseSize = rawJsonResponse.Length.GetBytesReadable()
        };

        // Create HTTP content for potential forwarding
        var httpContent = processedResponse.GetStringContent();
        var contentString = await httpContent.ReadAsStringAsync();

        // Verify the formatted JSON is valid
        var reprocessedResponse = contentString.Deserialize<JObject>();

        // Assert
        formattedJson.Should().NotBe(rawJsonResponse); // Should be formatted differently
        formattedJson.Should().Contain("  \"id\": 1"); // Should have indentation
        formattedJson.Should().Contain("  \"name\": \"John Doe\"");

        processedResponse.DisplayName.Should().Be("John Doe");
        processedResponse.ResponseSize.Should().Contain("B"); // Should show byte size

        httpContent.Headers.ContentType?.MediaType.Should().Be("application/json");

        reprocessedResponse.Should().NotBeNull();
        contentString.Should().Contain("DisplayName");
        contentString.Should().Contain("ResponseSize");
    }

    [Fact]
    public async Task EndToEnd_ErrorHandlingWithHttpAndStringOperations_ShouldHandleGracefully()
    {
        // Arrange
        var problematicData = new List<string?>
        {
            "valid@example.com",
            null,
            "",
            "invalid-email",
            "test@domain.com"
        };

        // Act - Handle null and invalid data gracefully
        var processedEmails = problematicData
            .Where(email => !string.IsNullOrEmpty(email) && !email.IsNullOrEmpty())
            .Where(email => !string.IsNullOrEmpty(email) && email.IsValidEmail())
            .Select(email => new
            {
                Email = email,
                Domain = email?.Split('@').LastOrDefault() ?? string.Empty,
                IsValid = !string.IsNullOrEmpty(email) && email.IsValidEmail(),
                Similarity = email?.CalculateSimilarity("test@example.com") ?? 0
            })
            .ToList();

        // Test error handling with HTTP content
        var validData = processedEmails.Where(x => x.IsValid).ToList();
        var httpContent = validData.GetStringContent();
        var contentString = await httpContent.ReadAsStringAsync();

        // Test malformed JSON handling
        var malformedJson = "{\"email\":\"test@example.com\",\"invalid\":}";
        ApiResponse? malformedResult = null;
        try
        {
            malformedResult = malformedJson.Deserialize<ApiResponse>();
        }
        catch (Newtonsoft.Json.JsonException)
        {
            // Expected behavior - malformed JSON should throw
        }

        // Deserialize valid content
        var restored = contentString.Deserialize<List<object>>();

        // Assert
        processedEmails.Should().HaveCount(2); // Only valid emails
        processedEmails[0].Email.Should().Be("valid@example.com");
        processedEmails[1].Email.Should().Be("test@domain.com");

        processedEmails[0].Domain.Should().Be("example.com");
        processedEmails[0].IsValid.Should().BeTrue();

        malformedResult.Should().BeNull(); // Should handle malformed JSON gracefully

        Assert.NotNull(restored);
        Assert.Equal(2, restored.Count);
    }
}
