// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The observed state of a <see cref="Production"/>.
/// </summary>
public class ProductionStatus
{
    /// <summary>
    /// Gets or sets the phase the Production is in.
    /// </summary>
    [Description("Pending, Blocked, Progressing or Ready.")]
    public ProductionPhase Phase { get; set; } = ProductionPhase.Pending;

    /// <summary>
    /// Gets or sets the generation of the spec the status describes.
    /// </summary>
    [Description("The generation of the spec the status describes.")]
    public long ObservedGeneration { get; set; }

    /// <summary>
    /// Gets or sets the address applications reach Chronicle on.
    /// </summary>
    [Description("The address applications reach Chronicle on, a chronicle+srv:// address without credentials.")]
    public string ChronicleAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Secret holding the connection string and client credentials. A Performance consumes it by name and never copies it.
    /// </summary>
    [Description("The name of the Secret holding the connection string, the client credentials and the certificate authority of Chronicle.")]
    public string ConnectionSecretName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database kind and mode the Production was first applied with. A spec that differs is refused.
    /// </summary>
    [Description("The database kind and mode the Production runs with, as kind/mode. A spec asking for another is refused.")]
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the graph mode and edition the Production was first applied with. A spec that differs is refused.
    /// </summary>
    [Description("The graph mode and edition the Production runs with, as mode/edition. A spec asking for another is refused.")]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conditions that describe the Production.
    /// </summary>
    [Description("The conditions that describe the Production.")]
    public IList<ResourceCondition> Conditions { get; set; } = [];
}
