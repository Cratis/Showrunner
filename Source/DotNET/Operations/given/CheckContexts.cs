// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// Builds the context a check is run with in a specification.
/// </summary>
public static class CheckContexts
{
    /// <summary>
    /// Creates a context at the current time of a manual clock.
    /// </summary>
    /// <param name="time">The clock.</param>
    /// <param name="window">How long ago the previous run started.</param>
    /// <param name="budget">How much collection time is left.</param>
    /// <returns>The context.</returns>
    public static CheckContext At(ManualTimeProvider time, TimeSpan? window = null, TimeSpan? budget = null) =>
        new(time.Now, time.Now - (window ?? TimeSpan.FromMinutes(5)), time.Now + (budget ?? TimeSpan.FromMinutes(2)), time);
}
#endif
