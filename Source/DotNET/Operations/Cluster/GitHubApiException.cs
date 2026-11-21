// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Thrown when GitHub answers a request with anything but a success, including a redirect that is never followed. The message never
/// carries the URL or a body: either may identify a private repository, and the request carried a credential.
/// </summary>
/// <param name="statusCode">The HTTP status.</param>
/// <param name="rateLimitRemaining">The <c>X-RateLimit-Remaining</c> header, when GitHub sent it.</param>
/// <param name="retryAfter">The <c>Retry-After</c> header, when GitHub sent it.</param>
/// <param name="rateLimitReset">The <c>X-RateLimit-Reset</c> header, when GitHub sent it.</param>
public sealed class GitHubApiException(int statusCode, string? rateLimitRemaining = null, string? retryAfter = null, string? rateLimitReset = null)
    : Exception($"GitHub answered HTTP {statusCode}")
{
    /// <summary>
    /// Gets the HTTP status.
    /// </summary>
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// Gets the <c>X-RateLimit-Remaining</c> header.
    /// </summary>
    public string? RateLimitRemaining { get; } = rateLimitRemaining;

    /// <summary>
    /// Gets the <c>Retry-After</c> header.
    /// </summary>
    public string? RetryAfter { get; } = retryAfter;

    /// <summary>
    /// Gets the <c>X-RateLimit-Reset</c> header.
    /// </summary>
    public string? RateLimitReset { get; } = rateLimitReset;

    /// <summary>
    /// Gets a value indicating whether GitHub is rate limiting the caller.
    /// </summary>
    public bool IsRateLimited => RateLimitRemaining == "0" || RetryAfter is not null;
}
