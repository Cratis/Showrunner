// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// What a <see cref="Production"/> runs. Schema only: the reconciliation that acts on it is a separate piece of work.
/// </summary>
public class ProductionSpec
{
    /// <summary>
    /// Gets or sets the Chronicle version, as an image tag.
    /// </summary>
    [Description("The Chronicle version, as an image tag.")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of Chronicle replicas.
    /// </summary>
    [Description("The number of Chronicle replicas.")]
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets how Chronicle's database is provided.
    /// </summary>
    [Description("How Chronicle's database is provided.")]
    public ProductionDatabase Database { get; set; } = new();

    /// <summary>
    /// Gets or sets how Chronicle's keys are stored.
    /// </summary>
    [Description("How Chronicle's keys are stored.")]
    public ProductionVault Vault { get; set; } = new();

    /// <summary>
    /// Gets or sets the named sinks applications store read models and graphs in.
    /// </summary>
    [Description("The named sinks applications store read models and graphs in, and the defaults for those that name none.")]
    public ProductionSinks Sinks { get; set; } = new();
}
