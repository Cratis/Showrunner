// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="ChronicleObserversCheck"/>.
/// </summary>
public sealed record ChronicleObserversOptions
{
    /// <summary>
    /// Gets how long a partition may keep failing before it is worth waking anyone. Retries back off from 2 seconds and
    /// stop after about half an hour, when Chronicle quarantines the partition.
    /// </summary>
    public TimeSpan FailingFor { get; init; } = TimeSpan.FromMinutes(15);
}
