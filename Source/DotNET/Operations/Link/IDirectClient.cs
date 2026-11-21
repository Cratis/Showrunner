// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// A client of Direct's watchdog protocol. It never throws for an unreachable Direct: that is <see cref="DirectOutcome.Unavailable"/>.
/// </summary>
public interface IDirectClient
{
    /// <summary>
    /// Claims the instance.
    /// </summary>
    /// <param name="bearer">The enrollment token on the first claim, the instance credential after.</param>
    /// <param name="request">The claim.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer.</returns>
    Task<DirectAnswer<WatchdogClaimResponse>> Claim(string bearer, WatchdogClaimRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the signed configuration.
    /// </summary>
    /// <param name="credentials">The credential and epoch.</param>
    /// <param name="after">The revision already applied.</param>
    /// <param name="renew">Whether to ask for the applied revision again with a fresh lease.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer; <see cref="DirectOutcome.NoContent"/> when there is nothing newer.</returns>
    Task<DirectAnswer<SignedWatchdogConfiguration>> Configuration(DirectCredentials credentials, long after, bool renew, CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges an applied revision.
    /// </summary>
    /// <param name="credentials">The credential and epoch.</param>
    /// <param name="acknowledgement">The acknowledgement.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer.</returns>
    Task<DirectAnswer<WatchdogConfigurationAcknowledgement>> Applied(DirectCredentials credentials, WatchdogConfigurationAcknowledgement acknowledgement, CancellationToken cancellationToken);

    /// <summary>
    /// Delivers a batch of outbox events.
    /// </summary>
    /// <param name="credentials">The credential and epoch.</param>
    /// <param name="batch">The batch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer with Direct's receipt.</returns>
    Task<DirectAnswer<WatchdogEventBatchReceipt>> Events(DirectCredentials credentials, WatchdogEventBatch batch, CancellationToken cancellationToken);

    /// <summary>
    /// Asks Direct to govern a remedy.
    /// </summary>
    /// <param name="credentials">The credential and epoch.</param>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer with Direct's receipt.</returns>
    Task<DirectAnswer<WatchdogRemedyRequestReceipt>> RemedyRequest(DirectCredentials credentials, WatchdogRemedyRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the remedy requests Direct has authorized.
    /// </summary>
    /// <param name="credentials">The credential and epoch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer.</returns>
    Task<DirectAnswer<WatchdogRemedyAuthorizations>> RemedyAuthorizations(DirectCredentials credentials, CancellationToken cancellationToken);
}
