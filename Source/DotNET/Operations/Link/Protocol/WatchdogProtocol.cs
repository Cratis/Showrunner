// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The HTTP protocol a watchdog instance speaks to Direct: every route, header and credential rule in one place.
/// </summary>
/// <remarks>
/// Every route is under <see cref="Base"/> with the instance's operator-facing name in place of <c>{name}</c>. Every body is
/// JSON serialized with <see cref="WatchdogJson.Options"/>. Authentication is a bearer token: <see cref="Claim"/> accepts the
/// one-time enrollment token (first claim) or the instance credential (later claims); every other route accepts only the
/// instance credential with the current fencing epoch in <see cref="EpochHeader"/>. A stale epoch is answered
/// <c>409 Conflict</c> - the caller has been fenced and must stop acting.
/// </remarks>
public static class WatchdogProtocol
{
    /// <summary>
    /// The route every watchdog route is under.
    /// </summary>
    public const string Base = "/webhooks/watchdogs/{name}";

    /// <summary>
    /// <c>POST</c> - claims the instance: <see cref="WatchdogClaimRequest"/> in, <see cref="WatchdogClaimResponse"/> out.
    /// </summary>
    public const string Claim = Base + "/claim";

    /// <summary>
    /// <c>GET ?after={revision}&amp;renew={true|false}</c> - the signed configuration, or <c>204 No Content</c>.
    /// </summary>
    public const string Configuration = Base + "/configuration";

    /// <summary>
    /// <c>POST</c> - acknowledges that a revision was applied: <see cref="WatchdogConfigurationAcknowledgement"/> in.
    /// </summary>
    public const string ConfigurationApplied = Base + "/configuration/applied";

    /// <summary>
    /// <c>POST</c> - delivers outbox events: <see cref="WatchdogEventBatch"/> in, <see cref="WatchdogEventBatchReceipt"/> out.
    /// </summary>
    public const string Events = Base + "/events";

    /// <summary>
    /// <c>POST</c> - asks Direct to govern a remedy: <see cref="WatchdogRemedyRequest"/> in, <see cref="WatchdogRemedyRequestReceipt"/> out.
    /// </summary>
    public const string RemedyRequests = Base + "/remedies/requests";

    /// <summary>
    /// <c>GET</c> - the remedy requests Direct has authorized: <see cref="WatchdogRemedyAuthorizations"/> out.
    /// </summary>
    public const string RemedyAuthorizations = Base + "/remedies/authorizations";

    /// <summary>
    /// The header carrying the fencing epoch the caller holds.
    /// </summary>
    public const string EpochHeader = "X-Direct-Watchdog-Epoch";

    /// <summary>
    /// The query parameter naming the revision the instance already applied.
    /// </summary>
    public const string AfterParameter = "after";

    /// <summary>
    /// The query parameter asking for the applied revision again with a fresh lease.
    /// </summary>
    public const string RenewParameter = "renew";

    /// <summary>
    /// The largest number of envelopes one <see cref="WatchdogEventBatch"/> may carry.
    /// </summary>
    public const int MaximumBatchSize = 500;

    /// <summary>
    /// How long a claim holds without an authenticated request.
    /// </summary>
    public static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Resolves a route template for one instance.
    /// </summary>
    /// <param name="route">The route template, one of the constants on this class.</param>
    /// <param name="instanceName">The instance's operator-facing name.</param>
    /// <returns>The concrete path.</returns>
    public static string For(string route, string instanceName)
    {
        ArgumentNullException.ThrowIfNull(route);
        return route.Replace("{name}", Uri.EscapeDataString(instanceName), StringComparison.Ordinal);
    }
}
