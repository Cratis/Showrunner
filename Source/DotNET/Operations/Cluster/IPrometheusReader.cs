// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The only way the watchdog reads Prometheus: small instant queries and the active targets. An implementation throws when the
/// answer cannot be trusted (no success status, warnings, a wrong result type, an unreadable sample); a check turns that into a
/// finding instead of granting health.
/// </summary>
public interface IPrometheusReader
{
    /// <summary>
    /// Runs an instant query that must return a vector.
    /// </summary>
    /// <param name="expression">The PromQL expression.</param>
    /// <param name="timeout">How long the request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The samples.</returns>
    Task<IReadOnlyList<PrometheusSample>> Query(string expression, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the active scrape targets.
    /// </summary>
    /// <param name="timeout">How long the request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The active targets.</returns>
    Task<IReadOnlyList<ScrapeTarget>> ActiveTargets(TimeSpan timeout, CancellationToken cancellationToken);
}
