// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// What a check is told about the run it is part of.
/// </summary>
/// <param name="Now">When the pass started.</param>
/// <param name="WindowStarted">When the previous run started; events before it were covered by that run.</param>
/// <param name="CollectionDeadline">When collection must be finished so there is room left to alert.</param>
/// <param name="TimeProvider">The clock.</param>
public sealed record CheckContext(
    DateTimeOffset Now,
    DateTimeOffset WindowStarted,
    DateTimeOffset CollectionDeadline,
    TimeProvider TimeProvider)
{
    /// <summary>
    /// Gets how much collection time is left.
    /// </summary>
    public TimeSpan Remaining => CollectionDeadline - TimeProvider.GetUtcNow();
}
