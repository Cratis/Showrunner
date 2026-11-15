// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Collecting.Access;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Notes.Collecting.Transport.given;

/// <summary>
/// Records what a desk decided, as the collecting side holds it, so specifications can ask the collector what it serves.
/// </summary>
public static class DeskDecisions
{
    /// <summary>
    /// When the desk made its first decision.
    /// </summary>
    public static readonly DateTimeOffset Start = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Gets a user at the identity provider the specifications use.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <returns>The identity.</returns>
    public static FeedbackGiverIdentity User(string subject) => new(new IdentityProviderName("github"), new IdentityProviderSubject(subject));

    /// <summary>
    /// Records that the desk granted a user the right.
    /// </summary>
    /// <param name="log">The event log of the tenant.</param>
    /// <param name="user">The user.</param>
    /// <param name="minutes">When the desk decided, in minutes after <see cref="Start"/>.</param>
    /// <returns>An awaitable task.</returns>
    public static Task Grant(IEventLog log, FeedbackGiverIdentity user, int minutes) =>
        log.Append(FeedbackGiverId.For(user), new FeedbackAccessGrantReceived("desk-operator-1"), occurred: Start.AddMinutes(minutes), subject: FeedbackGiverId.SubjectOf(user));

    /// <summary>
    /// Records that the desk revoked a user's right.
    /// </summary>
    /// <param name="log">The event log of the tenant.</param>
    /// <param name="user">The user.</param>
    /// <param name="minutes">When the desk decided, in minutes after <see cref="Start"/>.</param>
    /// <returns>An awaitable task.</returns>
    public static Task Revoke(IEventLog log, FeedbackGiverIdentity user, int minutes) =>
        log.Append(FeedbackGiverId.For(user), new FeedbackAccessRevocationReceived("desk-operator-1"), occurred: Start.AddMinutes(minutes), subject: FeedbackGiverId.SubjectOf(user));
}
#endif
