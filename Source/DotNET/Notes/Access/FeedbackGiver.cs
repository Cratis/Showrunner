// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Access.Receiving;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Whether the desk lets one user give feedback in the tenant, one per user. It is the write side's own model: a command that
/// gives feedback decides on it.
/// </summary>
/// <remarks>
/// It is a self-reducing read model. The latest decision by the time the desk made it wins, so a grant and a revocation that
/// arrive out of order still end the way the desk meant; at the same time the revocation wins. Chronicle delivers an event
/// at least once, so deciding again with the same time changes nothing.
/// </remarks>
/// <param name="Id">The user's right to give feedback.</param>
/// <param name="IsGranted">Whether the user may give feedback.</param>
/// <param name="DecidedAt">When the desk made the latest decision.</param>
public record FeedbackGiver(FeedbackGiverId Id, bool IsGranted, DateTimeOffset DecidedAt)
{
    /// <summary>
    /// Records that the desk granted the right to give feedback.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static FeedbackGiver Reduce(DeskGrantedFeedbackAccess @event, FeedbackGiver? current, EventContext context) =>
        Decide(current, context, true);

    /// <summary>
    /// Records that the desk revoked the right to give feedback.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static FeedbackGiver Reduce(DeskRevokedFeedbackAccess @event, FeedbackGiver? current, EventContext context) =>
        Decide(current, context, false);

    static FeedbackGiver Decide(FeedbackGiver? current, EventContext context, bool granted)
    {
        var id = new FeedbackGiverId(Guid.Parse(context.EventSourceId.Value));
        if (current is null)
        {
            return new(id, granted, context.Occurred);
        }

        // A decision the desk made before the one already held does not change it, and at the same time the revocation wins.
        var isEarlier = context.Occurred < current.DecidedAt;
        var revocationWinsTheTie = context.Occurred == current.DecidedAt && granted && !current.IsGranted;
        if (isEarlier || revocationWinsTheTie)
        {
            return current;
        }

        return current with { IsGranted = granted, DecidedAt = context.Occurred };
    }
}
