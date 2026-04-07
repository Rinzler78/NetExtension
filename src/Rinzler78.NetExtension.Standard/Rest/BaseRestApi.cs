using System;

namespace Rinzler78.NetExtension.Rest;

/// <summary>
/// Abstract base class for REST API clients, providing a base URI for all requests.
/// </summary>
public abstract class BaseRestApi
{
    /// <summary>
    /// Gets the base URI used for all API requests.
    /// </summary>
    public Uri BaseUri { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="BaseRestApi"/> with the specified base URI string and optional path.
    /// </summary>
    /// <param name="baseUri">The base URI string.</param>
    /// <param name="path">An optional path to append to the base URI.</param>
    protected BaseRestApi(string baseUri, string? path = null)
        : this(new Uri(baseUri), path)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BaseRestApi"/> with the specified base URI and optional path.
    /// </summary>
    /// <param name="baseUri">The base URI.</param>
    /// <param name="path">An optional path to append to the base URI.</param>
    protected BaseRestApi(Uri baseUri, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        BaseUri = path is null
            ? baseUri
            : new Uri(baseUri, path.TrimStart('/'));
    }
}
