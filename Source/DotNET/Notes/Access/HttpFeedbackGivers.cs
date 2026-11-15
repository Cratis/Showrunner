// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Transport;

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Represents an implementation of <see cref="IFeedbackGivers"/> for the HTTP transport: the desk's decisions reach the
/// application as the list of granted users the collector serves, so what a user may do is read from the
/// <see cref="GrantedGiversCache"/> and not from facts recorded in the application's event store.
/// </summary>
/// <remarks>
/// A granted user is a <see cref="FeedbackGiver"/> that is granted, decided when the collector last confirmed the list. A user
/// who is not in the list, and every user while the list is unknown or too old, has no state, which refuses them exactly as
/// a user the desk never decided about is refused in the same-kernel transport. The cache is the guard of a decision: unlike
/// the facts of the same-kernel transport it does not take part in the command's append, so a revocation takes effect when the
/// next refresh reads it.
/// </remarks>
/// <param name="cache">The cache of granted users.</param>
public class HttpFeedbackGivers(GrantedGiversCache cache) : IFeedbackGivers
{
    /// <inheritdoc/>
    public Task<FeedbackGiver?> Current(FeedbackGiverId id) => Task.FromResult(Read(id));

    /// <inheritdoc/>
    public Task<FeedbackGiver?> ForDecision(FeedbackGiverId id, CancellationToken cancellationToken = default) => Task.FromResult(Read(id));

    FeedbackGiver? Read(FeedbackGiverId id) => cache.IsGranted(id) ? new FeedbackGiver(id, true, cache.ConfirmedAt) : null;
}
