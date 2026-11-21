// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The only way the watchdog reads GitHub: read-only GET requests to <c>api.github.com</c>. An implementation never follows a
/// redirect and never sends the credential anywhere else.
/// </summary>
public interface IGitHubReader
{
    /// <summary>
    /// Reads one page.
    /// </summary>
    /// <param name="uri">The absolute address; the caller has already checked that it is on <c>api.github.com</c>.</param>
    /// <param name="timeout">How long the request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page.</returns>
    /// <exception cref="GitHubApiException">GitHub did not answer with a success.</exception>
    /// <exception cref="GitHubResponseTooLargeException">The answer is larger than the limit.</exception>
    Task<GitHubPage> Get(Uri uri, TimeSpan timeout, CancellationToken cancellationToken);
}
