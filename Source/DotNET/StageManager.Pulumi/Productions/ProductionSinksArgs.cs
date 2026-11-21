// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The named sinks of a Production, and which of them read models and graphs use when they name none.
/// </summary>
public sealed class ProductionSinksArgs
{
    /// <summary>
    /// Gets the configuration read models store to when they name no sink, or null to keep the database of the Production.
    /// </summary>
    public string? ReadModels { get; init; }

    /// <summary>
    /// Gets the configuration graphs store to when they name no sink, or null to keep the graph of the Production.
    /// </summary>
    public string? Graphs { get; init; }

    /// <summary>
    /// Gets the named sink configurations.
    /// </summary>
    public IList<SinkArgs> Configurations { get; init; } = [];

    /// <summary>
    /// Builds the spec of the sinks.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("readModels", ReadModels)
            .Put("graphs", Graphs)
            .PutList("configurations", Configurations.Select(sink => sink.ToSpec()));
}
