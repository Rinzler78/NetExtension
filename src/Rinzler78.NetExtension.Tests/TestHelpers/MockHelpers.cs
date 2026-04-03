using System.Net;
using Moq;
using Moq.Protected;

namespace Rinzler78.NetExtension.Tests.TestHelpers;

public static class MockHelpers
{
    public static Mock<HttpMessageHandler> CreateHttpMessageHandlerMock(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            });

        return handlerMock;
    }

    public static HttpClient CreateHttpClientMock(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = CreateHttpMessageHandlerMock(responseContent, statusCode);
        return new HttpClient(handlerMock.Object);
    }

    public static Mock<HttpMessageHandler> CreateHttpMessageHandlerMockWithException<TException>()
        where TException : Exception, new()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TException());

        return handlerMock;
    }

    public static async Task<T> RunConcurrentlyAsync<T>(int threadCount, Func<Task<T>> action)
    {
        var tasks = new List<Task<T>>();

        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(action));
        }

        var results = await Task.WhenAll(tasks);
        return results.First();
    }

    public static async Task RunConcurrentlyAsync(int threadCount, Func<Task> action)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(action));
        }

        await Task.WhenAll(tasks);
    }

    public static void RunConcurrently(int threadCount, Action action)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(action));
        }

        Task.WaitAll(tasks.ToArray());
    }

    public static string CreateTempFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }

    /// <summary>Creates a temporary file with the given content and a specific extension.</summary>
    public static string CreateTempFile(string content, string extension)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        File.WriteAllText(tempFile, content);
        return tempFile;
    }

    public static string CreateTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    public static void CleanupTempFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void CleanupTempDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }
}
