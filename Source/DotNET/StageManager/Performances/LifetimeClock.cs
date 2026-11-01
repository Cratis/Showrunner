// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Answers when a Performance expires. A Performance without a lifetime, or a permanent one, never does.
/// </summary>
public static class LifetimeClock
{
    /// <summary>
    /// Gets when the Performance expires.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The time, or null when it runs permanently.</returns>
    public static DateTimeOffset? ExpiresAt(PerformanceSpec spec) =>
        spec.Lifetime is { Permanent: false, Until: { } until } ? until : null;

    /// <summary>
    /// Determines whether the lifetime has ended.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when the Performance has expired.</returns>
    public static bool IsExpired(PerformanceSpec spec, DateTimeOffset now) => ExpiresAt(spec) is { } expiresAt && expiresAt <= now;
}
