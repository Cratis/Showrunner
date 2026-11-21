// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// When an instance should ask for a fresh lease on the configuration it holds.
/// </summary>
public static class ConfigurationLease
{
    /// <summary>
    /// Whether a held snapshot is past half its lease and should be renewed.
    /// </summary>
    /// <param name="held">The snapshot held.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when it should be renewed.</returns>
    public static bool IsDue(WatchdogConfigurationSnapshot held, DateTimeOffset now) =>
        now >= held.IssuedAt + ((held.ExpiresAt - held.IssuedAt) / 2);
}
