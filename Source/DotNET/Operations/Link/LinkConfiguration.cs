// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Pulls Direct's signed configuration and applies it atomically: only a snapshot that verifies under the pinned key, is for
/// this instance, is newer (or a renewed lease of the one held) and has not expired takes effect; the applied revision is then
/// acknowledged, retried on later passes until Direct has it. The lease is renewed once it is past half.
/// </summary>
/// <param name="options">The link options.</param>
/// <param name="client">The Direct client.</param>
/// <param name="store">The state store.</param>
/// <param name="timeProvider">The clock.</param>
/// <param name="logger">The logger.</param>
internal sealed class LinkConfiguration(DirectLinkOptions options, IDirectClient client, ILinkStore store, TimeProvider timeProvider, ILogger<DirectLink> logger)
{
    /// <summary>
    /// Syncs the configuration and acknowledges what was applied.
    /// </summary>
    /// <param name="context">The pass.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the configuration is synced.</returns>
    internal async Task Synchronize(LinkContext context, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var held = context.Identity!.Held?.Snapshot;
        var renew = held is not null && ConfigurationLease.IsDue(held, now);
        var answer = await client.Configuration(context.Credentials, held?.Revision ?? 0, renew, cancellationToken);
        if (context.Succeeded(answer, "fetching the configuration") && answer.Outcome == DirectOutcome.Ok)
        {
            await Take(context, answer.Value!, held, now, cancellationToken);
        }

        if (context.Connected)
        {
            await Acknowledge(context, cancellationToken);
        }
    }

    async Task Take(LinkContext context, SignedWatchdogConfiguration delivered, WatchdogConfigurationSnapshot? held, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var reason = ConfigurationAcceptance.Evaluate(delivered, context.Identity!.SigningKey, options.InstanceName, held, now);
        if (reason is not (ConfigurationAcceptanceReason.Accepted or ConfigurationAcceptanceReason.Renewed))
        {
            context.Problems.Add($"configuration revision {delivered.Snapshot?.Revision} was not applied: {reason}");
            return;
        }

        var accepted = reason == ConfigurationAcceptanceReason.Accepted;
        var identity = context.Identity with
        {
            Held = delivered,
            AcknowledgeRevision = accepted ? delivered.Snapshot.Revision : context.Identity.AcknowledgeRevision,
            AcknowledgeDigest = accepted ? delivered.Snapshot.Digest : context.Identity.AcknowledgeDigest
        };
        await store.SaveIdentity(identity, cancellationToken);
        context.Identity = identity;
        if (accepted)
        {
            logger.Applied(delivered.Snapshot.Revision);
        }
    }

    async Task Acknowledge(LinkContext context, CancellationToken cancellationToken)
    {
        var identity = context.Identity!;
        if (identity.AcknowledgeRevision == 0)
        {
            return;
        }

        var answer = await client.Applied(context.Credentials, new WatchdogConfigurationAcknowledgement(identity.AcknowledgeRevision, identity.AcknowledgeDigest), cancellationToken);
        if (context.Succeeded(answer, "acknowledging the configuration"))
        {
            context.Identity = identity with { AcknowledgeRevision = 0, AcknowledgeDigest = string.Empty };
            await store.SaveIdentity(context.Identity, cancellationToken);
        }
    }
}
