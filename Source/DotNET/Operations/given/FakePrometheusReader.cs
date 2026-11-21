// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IPrometheusReader"/> the specifications fill in. A query is answered by the first entry of <see cref="Vectors"/>
/// whose key it contains; a query nothing answers returns an empty vector.
/// </summary>
public sealed class FakePrometheusReader : IPrometheusReader
{
    /// <summary>
    /// Gets the vectors, by a fragment of the expression that asks for them.
    /// </summary>
    public Dictionary<string, List<PrometheusSample>> Vectors { get; } = [];

    /// <summary>
    /// Gets the expressions that fail, by a fragment of the expression.
    /// </summary>
    public HashSet<string> FailingExpressions { get; } = [];

    /// <summary>
    /// Gets the active targets.
    /// </summary>
    public List<ScrapeTarget> Targets { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the active targets cannot be read.
    /// </summary>
    public bool TargetsFail { get; set; }

    /// <summary>
    /// Gets the queries that were run, with the timeout each was given.
    /// </summary>
    public List<(string Expression, TimeSpan Timeout)> Queries { get; } = [];

    /// <summary>
    /// Creates a sample.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="labels">The labels, as name and value pairs.</param>
    /// <returns>The sample.</returns>
    public static PrometheusSample Sample(double value, params (string Name, string Value)[] labels) =>
        new(labels.ToDictionary(label => label.Name, label => label.Value), value);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PrometheusSample>> Query(string expression, TimeSpan timeout, CancellationToken cancellationToken)
    {
        Queries.Add((expression, timeout));
        if (FailingExpressions.Any(expression.Contains))
        {
            throw new InvalidOperationException("query failed");
        }

        var match = Vectors.FirstOrDefault(entry => expression.Contains(entry.Key));
        return Task.FromResult<IReadOnlyList<PrometheusSample>>(match.Value ?? []);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ScrapeTarget>> ActiveTargets(TimeSpan timeout, CancellationToken cancellationToken) =>
        TargetsFail ? throw new InvalidOperationException("target query failed") : Task.FromResult<IReadOnlyList<ScrapeTarget>>(Targets);
}
#endif
