// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The watchdog's own re-check of an authorization Direct granted, made immediately before executing it: the same remedy,
/// revision and epoch it was authorized under, still unexpired, the target still failing, and budget and cooldown still allowing
/// it. Direct's authorization is necessary, never sufficient.
/// </summary>
public static class AuthorizationRecheck
{
    /// <summary>
    /// Finds why an authorization may not be executed.
    /// </summary>
    /// <param name="authorization">The authorization.</param>
    /// <param name="held">The configuration held.</param>
    /// <param name="epoch">The epoch held.</param>
    /// <param name="fencingCertain">Whether the claim was confirmed this pass.</param>
    /// <param name="target">What was last observed of the remedy's verification check.</param>
    /// <param name="attempts">When earlier attempts started.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The problem - empty when it may be executed.</returns>
    public static string Problem(
        WatchdogRemedyAuthorization authorization,
        WatchdogConfigurationSnapshot? held,
        long epoch,
        bool fencingCertain,
        ObservedCheckState target,
        IReadOnlyList<DateTimeOffset> attempts,
        DateTimeOffset now)
    {
        var remedy = held?.Remedies.FirstOrDefault(candidate => string.Equals(candidate.Name, authorization.Remedy, StringComparison.Ordinal));
        if (held is null || remedy is null)
        {
            return $"The configuration held has no remedy '{authorization.Remedy}'";
        }

        if (held.Revision != authorization.ConfigurationRevision)
        {
            return $"Authorized under revision {authorization.ConfigurationRevision}, but revision {held.Revision} is applied";
        }

        if (authorization.FencingEpoch != epoch || !fencingCertain)
        {
            return $"Authorized for fencing epoch {authorization.FencingEpoch}, which this watchdog cannot be sure it holds";
        }

        if (authorization.ExpiresAt <= now)
        {
            return $"The authorization lapsed at {authorization.ExpiresAt:O}";
        }

        if (target != ObservedCheckState.Failing)
        {
            return $"'{remedy.Verification}' is not failing any more";
        }

        if (attempts.Count(started => now - started < OfflineRemedyPolicy.AttemptWindow) >= Math.Max(remedy.MaxAttempts, 0))
        {
            return $"The attempt budget of {remedy.MaxAttempts} is spent";
        }

        return attempts.Count > 0 && now - attempts.Max() < remedy.Cooldown
            ? $"The previous attempt is inside the {remedy.Cooldown} cooldown"
            : string.Empty;
    }
}
