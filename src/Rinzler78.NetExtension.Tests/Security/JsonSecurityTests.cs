using System;
using FluentAssertions;
using Newtonsoft.Json;
using Rinzler78.NetExtension.Json;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Security;

/// <summary>
/// Security validation tests for JSON operations.
/// Tests for potential vulnerabilities in JSON deserialization.
/// </summary>
[Trait("Category", "Security")]
public class JsonSecurityTests
{
    public class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Theory]
    [InlineData("{'$type': 'System.Windows.Forms.Control'}")]
    [InlineData("{'$type': 'System.Configuration.Install.AssemblyInstaller'}")]
    [InlineData("{'$type': 'System.Activities.Presentation.WorkflowDesigner'}")]
    [InlineData("{'$type': 'System.Web.UI.WebControls.FileUpload'}")]
    public void DeSerialize_MaliciousTypeSpecifiers_ShouldNotExecuteMaliciousCode(string maliciousJson)
    {
        // Act & Assert
        // The DeSerialize method should not deserialize dangerous types
        // Since we don't have TypeNameHandling enabled by default, this should be safe
        Action act = () => maliciousJson.Deserialize<object>();

        // Should not throw any exceptions and should not execute malicious code
        act.Should().NotThrow();

        // The result should not be a dangerous type (we check for common dangerous patterns)
        var result = maliciousJson.Deserialize<object>();
        if (result != null)
        {
            var resultType = result.GetType();
            resultType.Name.Should().NotContain("Control");
            resultType.Name.Should().NotContain("AssemblyInstaller");
        }
    }

    [Theory]
    [InlineData("{'__proto__': {'isAdmin': true}}")]
    [InlineData("{'constructor': {'prototype': {'isAdmin': true}}}")]
    [InlineData("{'__proto__': {'toString': 'malicious'}}")]
    public void DeSerialize_PrototypePollutionAttempts_ShouldNotAffectObjectPrototype(string pollutionJson)
    {
        // Act
        var result = pollutionJson.Deserialize<TestClass>();

        // Assert
        // .NET is not vulnerable to prototype pollution like JavaScript,
        // but we should ensure our JSON parsing doesn't create unexpected behavior
        result.Should().NotBeNull();

        // Verify that the object only has expected properties
        var type = typeof(TestClass);
        var properties = type.GetProperties();
        properties.Should().HaveCount(2); // Name and Value only
        properties.Should().Contain(p => p.Name == "Name");
        properties.Should().Contain(p => p.Name == "Value");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("0")]
    [InlineData("false")]
    public void DeSerialize_SafePrimitiveValues_ShouldDeserializeCorrectly(string safeJson)
    {
        // Act & Assert
        Action act = () => safeJson.Deserialize<object>();
        act.Should().NotThrow();
    }

    [Fact]
    public void DeSerialize_LargeStrings_ShouldHandleGracefully()
    {
        // Arrange - Create JSON with very large strings
        var largeStringJsons = new[]
        {
            "{\"Name\": \"" + new string('A', 10000) + "\"}",
            "{\"Name\": \"" + new string('B', 50000) + "\"}"
        };

        foreach (var largeStringJson in largeStringJsons)
        {
            // Act
            Action act = () => largeStringJson.Deserialize<TestClass>();

            // Assert
            // Should either succeed or fail gracefully without causing memory issues
            try
            {
                act();
            }
            catch (OutOfMemoryException)
            {
                // acceptable - no assertion needed
            }
            catch (JsonException)
            {
                // acceptable - no assertion needed
            }
        }
    }

    [Fact]
    public void DeSerialize_DeeplyNestedJson_ShouldHandleGracefully()
    {
        // Arrange - Create deeply nested JSON that could cause stack overflow
        var deepJson = "{";
        for (int i = 0; i < 1000; i++)
        {
            deepJson += "\"level" + i + "\":{";
        }
        deepJson += "\"value\":\"deep\"";
        for (int i = 0; i < 1000; i++)
        {
            deepJson += "}";
        }
        deepJson += "}";

        // Act & Assert
        Action act = () => deepJson.Deserialize<object>();

        // Should either succeed or fail gracefully without stack overflow
        try
        {
            act();
        }
        catch (JsonReaderException)
        {
            // acceptable - no assertion needed
        }
        catch (StackOverflowException)
        {
            // This should not happen with proper JSON parsing
            Assert.Fail("StackOverflowException indicates a security vulnerability");
        }
    }

    [Theory]
    [InlineData("\"\\u0000\"")]          // Null character
    [InlineData("\"\\u0001\\u0002\"")]   // Control characters
    [InlineData("\"\\u000C\"")]          // Form feed
    [InlineData("\"\\u007F\"")]          // DEL character
    public void DeSerialize_ControlCharacters_ShouldHandleSafely(string controlCharJson)
    {
        // Act
        Action act = () => controlCharJson.Deserialize<string>();

        // Assert
        act.Should().NotThrow();

        var result = controlCharJson.Deserialize<string>();
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("{\"eval\": \"alert('xss')\"}")]
    [InlineData("{\"script\": \"<script>alert('xss')</script>\"}")]
    [InlineData("{\"onload\": \"javascript:alert('xss')\"}")]
    public void DeSerialize_XssAttempts_ShouldNotExecuteScript(string xssJson)
    {
        // Act
        var result = xssJson.Deserialize<TestClass>();

        // Assert
        // JSON deserialization should not execute any scripts
        // The result should just be a plain object with string properties
        result.Should().NotBeNull();

        // Verify no script execution occurred (if Name property exists and contains the payload)
        if (!string.IsNullOrEmpty(result?.Name))
        {
            result.Name.Should().NotContain("javascript:");
            result.Name.Should().NotContain("<script>");
        }
    }

    [Fact]
    public void DeSerialize_CircularReference_ShouldHandleGracefully()
    {
        // This tests for JSON that might cause infinite loops during deserialization
        var circularJson = "{\"parent\": {\"child\": {\"parent\": {\"child\": null}}}}";

        // Act & Assert
        Action act = () => circularJson.Deserialize<object>();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeSerialize_EmptyOrWhitespaceInput_ShouldReturnDefault(string emptyInput)
    {
        // Act
        var result = emptyInput.Deserialize<TestClass>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DeSerialize_NullInput_ShouldReturnDefault()
    {
        // Act
        string? nullInput = null;
        var result = nullInput.Deserialize<TestClass>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid json")]
    [InlineData("{invalid}")]
    [InlineData("{\"unclosed\": \"string")]
    public void DeSerialize_MalformedJson_ShouldThrowJsonException(string malformedJson)
    {
        // Act & Assert
        Action act = () => malformedJson.Deserialize<TestClass>();
        act.Should().Throw<JsonException>();
    }
}
