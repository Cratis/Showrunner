// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Decides whether a delivered snapshot may replace the one an instance holds. Pure: stale, out of order or expired
/// snapshots cannot take effect.
/// </summary>
public static class ConfigurationAcceptance
{
    /// <summary>
    /// Evaluates a delivered snapshot against the one an instance holds, accepting a renewed lease of it.
    /// </summary>
    /// <remarks>
    /// A renewal is the held revision again with the same digest, issued no earlier and expiring later. Anything else at
    /// the held revision is refused as <see cref="ConfigurationAcceptanceReason.NotNewer"/>, so a renewal can extend a
    /// grant but never change it.
    /// </remarks>
    /// <param name="delivered">The signed snapshot.</param>
    /// <param name="signingKey">The instance's base64 signing key.</param>
    /// <param name="instanceName">The instance's name.</param>
    /// <param name="held">The snapshot it holds - <see langword="null"/> for none.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The reason - <see cref="ConfigurationAcceptanceReason.Accepted"/> or
    /// <see cref="ConfigurationAcceptanceReason.Renewed"/> when it may be applied.</returns>
    public static ConfigurationAcceptanceReason Evaluate(
        SignedWatchdogConfiguration? delivered,
        string signingKey,
        string instanceName,
        WatchdogConfigurationSnapshot? held,
        DateTimeOffset now)
    {
        if (delivered is null || !ConfigurationSignature.Verify(delivered, signingKey))
        {
            return ConfigurationAcceptanceReason.InvalidSignature;
        }

        var snapshot = delivered.Snapshot;
        if (!string.Equals(snapshot.InstanceName, instanceName, StringComparison.Ordinal))
        {
            return ConfigurationAcceptanceReason.WrongInstance;
        }

        var renewal = held is not null && IsRenewalOf(snapshot, held);
        if (snapshot.Revision <= (held?.Revision ?? 0) && !renewal)
        {
            return ConfigurationAcceptanceReason.NotNewer;
        }

        if (snapshot.ExpiresAt <= now)
        {
            return ConfigurationAcceptanceReason.Expired;
        }

        if (!string.Equals(ConfigurationDigest.Of(snapshot), snapshot.Digest, StringComparison.Ordinal))
        {
            return ConfigurationAcceptanceReason.DigestMismatch;
        }

        return renewal ? ConfigurationAcceptanceReason.Renewed : ConfigurationAcceptanceReason.Accepted;
    }

    static bool IsRenewalOf(WatchdogConfigurationSnapshot snapshot, WatchdogConfigurationSnapshot held) =>
        snapshot.Revision == held.Revision &&
        string.Equals(snapshot.Digest, held.Digest, StringComparison.Ordinal) &&
        snapshot.IssuedAt >= held.IssuedAt &&
        snapshot.ExpiresAt > held.ExpiresAt;
}
