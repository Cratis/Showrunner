// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access;

/// <summary>
/// How long a reporter lets support sign in as them. A command carries the duration; the event records the fixed
/// <see cref="ValidUntil"/> it gives, so the fact does not change meaning with the time it is read.
/// </summary>
public enum AccessDuration
{
    /// <summary>
    /// One hour.
    /// </summary>
    OneHour = 0,

    /// <summary>
    /// One day.
    /// </summary>
    OneDay = 1,

    /// <summary>
    /// Seven days.
    /// </summary>
    SevenDays = 2
}

/// <summary>
/// Extension methods for <see cref="AccessDuration"/>.
/// </summary>
public static class AccessDurationExtensions
{
    /// <summary>
    /// Gets the end of a grant of the duration that starts now.
    /// </summary>
    /// <param name="duration">The <see cref="AccessDuration"/>.</param>
    /// <param name="now">The time the grant starts.</param>
    /// <returns>The <see cref="ValidUntil"/>.</returns>
    public static ValidUntil From(this AccessDuration duration, DateTimeOffset now) => duration switch
    {
        AccessDuration.OneDay => now.AddDays(1),
        AccessDuration.SevenDays => now.AddDays(7),
        _ => now.AddHours(1)
    };
}
