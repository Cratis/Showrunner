// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// A toleration that lets the pods of a <see cref="Production"/> run on tainted nodes.
/// </summary>
public class ProductionToleration
{
    /// <summary>
    /// Gets or sets the taint key it applies to. Empty with the Exists operator matches every taint.
    /// </summary>
    [Description("The taint key the toleration applies to. Empty with the Exists operator matches every taint.")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operator, Equal (the default) or Exists.
    /// </summary>
    [Description("Equal (the default) or Exists.")]
    public string Operator { get; set; } = "Equal";

    /// <summary>
    /// Gets or sets the taint value the Equal operator compares with.
    /// </summary>
    [Description("The taint value to match when the operator is Equal.")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the taint effect it applies to. Empty matches all effects.
    /// </summary>
    [Description("NoSchedule, PreferNoSchedule or NoExecute. Empty matches all effects.")]
    public string Effect { get; set; } = string.Empty;
}
