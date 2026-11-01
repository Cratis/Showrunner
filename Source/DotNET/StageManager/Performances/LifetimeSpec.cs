// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How long a <see cref="Performance"/> runs: permanently, or until a given time.
/// </summary>
public class LifetimeSpec
{
    /// <summary>
    /// Gets or sets whether the Performance runs permanently.
    /// </summary>
    [Description("Runs permanently. Give this or until, not both.")]
    public bool Permanent { get; set; }

    /// <summary>
    /// Gets or sets when the Performance stops.
    /// </summary>
    [Description("When the Performance stops: its workloads are scaled to zero and its public route is removed. The phase is then Expired.")]
    public DateTimeOffset? Until { get; set; }

    /// <summary>
    /// Gets or sets how many days the data of an expired Performance is kept.
    /// </summary>
    [Description("How many days after expiry the data should be kept. The operator owns no storage; status.dataRetainedUntil tells the publisher when it may remove the data it provided.")]
    public int DataRetentionDays { get; set; } = 7;
}
