// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// What a <see cref="Production"/> runs: Chronicle, its database, its optional graph and the named sinks of its applications.
/// </summary>
public class ProductionSpec
{
    /// <summary>
    /// Gets or sets the Chronicle version, as an image tag.
    /// </summary>
    [Required]
    [Description("The Chronicle version, as a tag of the cratis/chronicle production image, such as 19.148.0. Changing it rolls Chronicle.")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of Chronicle replicas.
    /// </summary>
    [Description("The number of Chronicle replicas. Above one, they form one Orleans cluster through the database.")]
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets how Chronicle's database is provided.
    /// </summary>
    [Description("How Chronicle's database is provided.")]
    public ProductionDatabase Database { get; set; } = new();

    /// <summary>
    /// Gets or sets how Chronicle's keys are stored.
    /// </summary>
    [Description("How Chronicle's keys are stored. Only None is supported yet.")]
    public ProductionVault Vault { get; set; } = new();

    /// <summary>
    /// Gets or sets the Neo4j graph of the Production.
    /// </summary>
    [Description("The Neo4j graph graph projections land in: None, Managed or External.")]
    public ProductionGraph Graph { get; set; } = new();

    /// <summary>
    /// Gets or sets the named sinks applications store read models and graphs in.
    /// </summary>
    [Description("The named sinks applications store read models and graphs in, and the defaults for those that name none.")]
    public ProductionSinks Sinks { get; set; } = new();
}
