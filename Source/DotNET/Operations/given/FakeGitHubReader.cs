// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IGitHubReader"/> that answers from routes the specification adds, by path and query. A route that is not
/// added answers 404 the way GitHub does for what a token cannot see.
/// </summary>
public sealed class FakeGitHubReader : IGitHubReader
{
    readonly Dictionary<string, Func<GitHubPage>> _routes = [];

    /// <summary>
    /// Gets the addresses that were requested.
    /// </summary>
    public List<string> Requests { get; } = [];

    /// <summary>
    /// Gets the full addresses that were requested, to see where a credential would have gone.
    /// </summary>
    public List<Uri> Uris { get; } = [];

    /// <summary>
    /// Gets the timeouts that were given.
    /// </summary>
    public List<TimeSpan> Timeouts { get; } = [];

    /// <summary>
    /// Answers a route with a JSON body.
    /// </summary>
    /// <param name="pathAndQuery">The path and query, for example <c>/orgs/Cratis/repos?type=all&amp;per_page=100</c>.</param>
    /// <param name="body">The body.</param>
    /// <param name="next">The next page address, if any.</param>
    public void Respond(string pathAndQuery, object body, string? next = null) =>
        _routes[pathAndQuery] = () => new GitHubPage(JsonSerializer.SerializeToElement(body), next);

    /// <summary>
    /// Answers a route with a failure.
    /// </summary>
    /// <param name="pathAndQuery">The path and query.</param>
    /// <param name="error">The failure.</param>
    public void Fail(string pathAndQuery, Exception error) => _routes[pathAndQuery] = () => throw error;

    /// <inheritdoc/>
    public Task<GitHubPage> Get(Uri uri, TimeSpan timeout, CancellationToken cancellationToken)
    {
        Requests.Add(uri.PathAndQuery);
        Uris.Add(uri);
        Timeouts.Add(timeout);
        return Task.FromResult(_routes.TryGetValue(uri.PathAndQuery, out var route) ? route() : throw new GitHubApiException(404));
    }
}
#endif
