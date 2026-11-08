// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The named sinks of a <see cref="Production"/>, and which of them read models and graphs use when they name none.
/// </summary>
public class ProductionSinks
{
    /// <summary>
    /// Gets or sets the configuration read models store to when they name no sink. Empty keeps the database of the Production.
    /// </summary>
    [Description("The configuration read models store to when they name no sink. Empty keeps the database of the Production.")]
    public string ReadModels { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration graphs store to when they name no sink. Empty keeps the graph of the Production.
    /// </summary>
    [Description("The configuration graphs store to when they name no sink. Empty keeps the graph of the Production.")]
    public string Graphs { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the named sink configurations.
    /// </summary>
    [Description("The named sink configurations.")]
    public IList<ProductionSinkConfiguration> Configurations { get; set; } = [];

    /// <summary>
    /// Gets whether a name is configured, without regard to case as Chronicle matches it.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>True when a configuration has the name.</returns>
    public bool Configures(string name) => Configurations.Any(configuration => string.Equals(configuration.Name, name, StringComparison.OrdinalIgnoreCase));
}
