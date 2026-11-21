// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Text.Json;

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Reads the GitHub REST API with a read-only token. The <see cref="HttpClient"/> it is given must not follow redirects, so a
/// redirect answer is an error here and the credential never reaches another host.
/// </summary>
/// <param name="http">The HTTP client, with automatic redirects turned off.</param>
/// <param name="token">The token. It is never logged and never part of an error.</param>
public sealed class HttpGitHubReader(HttpClient http, string token) : IGitHubReader
{
    /// <summary>
    /// The most a single answer may be.
    /// </summary>
    public const int MaximumBodyBytes = 2 * 1024 * 1024;

    /// <inheritdoc/>
    public async Task<GitHubPage> Get(Uri uri, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.ParseAdd("Cratis-cluster-health-watchdog");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, limit.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new GitHubApiException(
                (int)response.StatusCode,
                Header(response, "X-RateLimit-Remaining"),
                Header(response, "Retry-After"),
                Header(response, "X-RateLimit-Reset"));
        }

        var buffer = new MemoryStream();
        await using (buffer)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(limit.Token);
            var chunk = new byte[16 * 1024];
            int read;
            while ((read = await stream.ReadAsync(chunk, limit.Token)) > 0)
            {
                await buffer.WriteAsync(chunk.AsMemory(0, read), limit.Token);
                if (buffer.Length > MaximumBodyBytes)
                {
                    throw new GitHubResponseTooLargeException();
                }
            }

            buffer.Position = 0;
            using var document = await JsonDocument.ParseAsync(buffer, cancellationToken: limit.Token);
            return new GitHubPage(document.RootElement.Clone(), NextOf(Header(response, "Link")));
        }
    }

    static string? NextOf(string? link)
    {
        // The Link header is a comma separated list of <address>; rel="name" entries.
        foreach (var entry in (link ?? string.Empty).Split(',', StringSplitOptions.TrimEntries))
        {
            var open = entry.IndexOf('<', StringComparison.Ordinal);
            var close = entry.IndexOf('>', StringComparison.Ordinal);
            if (open == 0 && close > open && entry[(close + 1)..].Replace(" ", string.Empty, StringComparison.Ordinal).Contains(";rel=\"next\"", StringComparison.Ordinal))
            {
                return entry[(open + 1)..close];
            }
        }

        return null;
    }

    static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
}
