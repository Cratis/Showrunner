// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// Defines a toleration that lets the pods of a Production run on tainted nodes.
/// </summary>
public sealed class ProductionTolerationArgs
{
    /// <summary>
    /// Gets the taint key it applies to. Empty with the <c>Exists</c> operator matches every taint.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Gets the operator, <c>Equal</c> (the default) or <c>Exists</c>.
    /// </summary>
    public string Operator { get; init; } = "Equal";

    /// <summary>
    /// Gets the taint value the <c>Equal</c> operator compares with.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Gets the taint effect it applies to, such as <c>NoSchedule</c>. Empty matches all effects.
    /// </summary>
    public string? Effect { get; init; }

    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("key", Key)
            .Put("operator", Operator)
            .Put("value", Value)
            .Put("effect", Effect);
}
