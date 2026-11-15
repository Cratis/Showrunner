// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.Receiving;

/// <summary>
/// Turns the public events a desk sends about who may give feedback into the private facts the application records. Each
/// method is one row of the mapping table in the contracts documentation.
/// </summary>
public static class AccessPrivateEvents
{
    /// <summary>
    /// Gets the public events the application receives from a desk, and subscribes to.
    /// </summary>
    public static IReadOnlyList<Type> FromDesk { get; } = [typeof(FeedbackAccessGranted), typeof(FeedbackAccessRevoked)];

    /// <summary>
    /// Translates a public event a desk sent.
    /// </summary>
    /// <param name="deskEvent">The public event.</param>
    /// <returns>The tenant it names, the user it is about and the private fact, or <see langword="null"/> when it is not an event a desk sends about access.</returns>
    public static (TenantId TenantId, FeedbackGiverIdentity Giver, object Fact)? For(object deskEvent) => deskEvent switch
    {
        FeedbackAccessGranted granted => (granted.TenantId, granted.Giver, new DeskGrantedFeedbackAccess(granted.GrantedBy)),
        FeedbackAccessRevoked revoked => (revoked.TenantId, revoked.Giver, new DeskRevokedFeedbackAccess(revoked.RevokedBy)),
        _ => null
    };

    /// <summary>
    /// Gets the event source id the private fact about a user is recorded under, as Chronicle spells it.
    /// </summary>
    /// <param name="giver">The user.</param>
    /// <returns>The event source id.</returns>
    public static EventSourceId EventSourceIdOf(FeedbackGiverIdentity giver) => FeedbackGiverId.For(giver);
}
