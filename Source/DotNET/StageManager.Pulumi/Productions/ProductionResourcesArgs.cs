// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// Defines the compute resources of the Chronicle container of a Production.
/// </summary>
public sealed class ProductionResourcesArgs
{
    /// <summary>
    /// Gets what the scheduler reserves, by resource name, such as <c>memory: 1Gi</c>.
    /// </summary>
    public IDictionary<string, string> Requests { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the most the container may use, by resource name.
    /// </summary>
    public IDictionary<string, string> Limits { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Builds the resources as the operator's spec writes it.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutMap("requests", Requests.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .PutMap("limits", Limits.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)));
}
