// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access;

/// <summary>
/// Turns the public events a desk sends about who may give feedback into the private facts the collecting side records.
/// </summary>
public static class AccessPrivateEvents
{
    /// <summary>
    /// Gets the public events the collecting side receives from a desk, and subscribes to.
    /// </summary>
    public static IReadOnlyList<Type> FromDesk { get; } = [typeof(FeedbackAccessGranted), typeof(FeedbackAccessRevoked)];

    /// <summary>
    /// Gets the private facts the collecting side records about a desk's decisions.
    /// </summary>
    public static IReadOnlyList<Type> Facts { get; } = [typeof(FeedbackAccessGrantReceived), typeof(FeedbackAccessRevocationReceived)];

    /// <summary>
    /// Translates a public event a desk sent.
    /// </summary>
    /// <param name="deskEvent">The public event.</param>
    /// <returns>The tenant it names, the user it is about and the private fact, or <see langword="null"/> when it is not an event a desk sends about access.</returns>
    public static (TenantId TenantId, FeedbackGiverIdentity Giver, object Fact)? For(object deskEvent) => deskEvent switch
    {
        FeedbackAccessGranted granted => (granted.TenantId, granted.Giver, new FeedbackAccessGrantReceived(granted.GrantedBy)),
        FeedbackAccessRevoked revoked => (revoked.TenantId, revoked.Giver, new FeedbackAccessRevocationReceived(revoked.RevokedBy)),
        _ => null
    };
}
