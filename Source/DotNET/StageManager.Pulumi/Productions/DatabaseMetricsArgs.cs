// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The MongoDB metrics of a managed database: one <c>mongodb_exporter</c> per member, with Prometheus scrape annotations on its pod.
/// </summary>
public sealed class DatabaseMetricsArgs
{
    /// <summary>
    /// Gets a value indicating whether the exporters run. Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the exporter image, or null for the operator's pinned version.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the node labels the exporters must match.
    /// </summary>
    public IDictionary<string, string> NodeSelector { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Builds the spec of the metrics.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("enabled", Enabled)
            .Put("image", Image)
            .PutMap("nodeSelector", NodeSelector.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)));
}
