// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// How long a Performance runs: permanently, or until a given time.
/// </summary>
public sealed class LifetimeArgs
{
    /// <summary>
    /// Gets a value indicating whether the Performance runs permanently. Give this or <see cref="Until"/>, not both.
    /// </summary>
    public bool Permanent { get; init; }

    /// <summary>
    /// Gets when the Performance stops: its workloads are scaled to zero and its public route is removed.
    /// </summary>
    public DateTimeOffset? Until { get; init; }

    /// <summary>
    /// Gets how many days after expiry the data should be kept, or null for the operator's default (7).
    /// </summary>
    public int? DataRetentionDays { get; init; }

    /// <summary>
    /// Builds the spec of the lifetime.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutWhenTrue("permanent", Permanent)
            .Put("until", Until?.ToString("o", CultureInfo.InvariantCulture))
            .Put("dataRetentionDays", DataRetentionDays);
}
