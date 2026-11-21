// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A partition of a Chronicle observer that failed.
/// </summary>
/// <param name="ObserverId">The observer.</param>
/// <param name="IsResolved">Whether the failure has been resolved.</param>
/// <param name="IsQuarantined">Whether the partition ran out of retries and is no longer retried.</param>
/// <param name="Attempts">When each failed attempt happened.</param>
public sealed record ChronicleFailedPartition(string ObserverId, bool IsResolved, bool IsQuarantined, IReadOnlyList<DateTimeOffset> Attempts)
{
    /// <summary>
    /// Gets when the partition first failed, or <see langword="null"/> when no attempt is recorded.
    /// </summary>
    public DateTimeOffset? FirstFailure => Attempts.Count == 0 ? null : Attempts.Min();
}
