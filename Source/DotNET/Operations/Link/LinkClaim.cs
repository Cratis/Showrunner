// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Claims the instance: the first claim exchanges the one-time enrollment token for the instance credential and pins the
/// signing key; every later claim presents the credential and the session this watchdog kept, and returns the current epoch.
/// </summary>
/// <param name="options">The link options.</param>
/// <param name="client">The Direct client.</param>
/// <param name="store">The state store.</param>
/// <param name="timeProvider">The clock.</param>
internal sealed class LinkClaim(DirectLinkOptions options, IDirectClient client, ILinkStore store, TimeProvider timeProvider)
{
    /// <summary>
    /// Claims the instance.
    /// </summary>
    /// <param name="context">The pass.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the claim was confirmed.</returns>
    internal async Task<bool> Claim(LinkContext context, CancellationToken cancellationToken)
    {
        var identity = context.Identity ?? new LinkIdentity(Guid.NewGuid().ToString("N"), string.Empty, string.Empty, 0, null);
        var bearer = identity.Credential.Length > 0 ? identity.Credential : options.EnrollmentToken;
        if (string.IsNullOrEmpty(bearer))
        {
            context.Connection = LinkConnection.NotEnrolled;
            context.Problems.Add("not enrolled: there is no instance credential and no enrollment token");
            return false;
        }

        var answer = await client.Claim(bearer, new WatchdogClaimRequest(options.RuntimeVersion, identity.Session, WatchdogVariant.Slim), cancellationToken);
        if (!context.Succeeded(answer, "claiming the instance"))
        {
            return false;
        }

        return await Accept(context, identity, answer.Value!, cancellationToken);
    }

    static bool Refuse(LinkContext context, string problem)
    {
        context.Connection = LinkConnection.Refused;
        context.Problems.Add(problem);
        return false;
    }

    async Task<bool> Accept(LinkContext context, LinkIdentity identity, WatchdogClaimResponse claim, CancellationToken cancellationToken)
    {
        var credential = claim.Credential.Length > 0 ? claim.Credential : identity.Credential;
        if (credential.Length == 0)
        {
            return Refuse(context, "Direct returned no instance credential for the enrollment token");
        }

        if (identity.SigningKey.Length > 0 && !string.Equals(identity.SigningKey, claim.SigningKey, StringComparison.Ordinal))
        {
            return Refuse(context, "Direct presented a signing key other than the pinned one; delete the link Secret and enroll again if the key was rotated on purpose");
        }

        var confirmed = identity with
        {
            Credential = credential,
            SigningKey = identity.SigningKey.Length > 0 ? identity.SigningKey : claim.SigningKey,
            Epoch = claim.FencingEpoch,
            ConfirmedAt = timeProvider.GetUtcNow()
        };

        // The credential is returned once: it must be stored before anything else is done with it.
        await store.SaveIdentity(confirmed, cancellationToken);
        context.Identity = confirmed;
        context.Connected = true;
        context.Connection = LinkConnection.Connected;
        return true;
    }
}
