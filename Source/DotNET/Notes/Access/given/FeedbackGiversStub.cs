// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.given;

/// <summary>
/// An <see cref="IFeedbackGivers"/> that holds what the desk decided, without an event store: the specifications state the
/// outcome the read model would have, which <c>for_FeedbackGiver</c> proves from the events.
/// </summary>
public class FeedbackGiversStub : IFeedbackGivers
{
    static readonly DateTimeOffset _decided = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    readonly Dictionary<FeedbackGiverId, FeedbackGiver> _states = [];

    /// <summary>
    /// Gets how many times a decision read was made.
    /// </summary>
    public int DecisionReads { get; private set; }

    /// <summary>
    /// Gets how many times a plain read was made.
    /// </summary>
    public int PlainReads { get; private set; }

    /// <summary>
    /// Lets the desk grant a user the right.
    /// </summary>
    /// <param name="giver">The user.</param>
    public void Grant(FeedbackGiverIdentity giver) => _states[FeedbackGiverId.For(giver)] = new(FeedbackGiverId.For(giver), true, _decided);

    /// <summary>
    /// Lets the desk revoke a user's right.
    /// </summary>
    /// <param name="giver">The user.</param>
    public void Revoke(FeedbackGiverIdentity giver) => _states[FeedbackGiverId.For(giver)] = new(FeedbackGiverId.For(giver), false, _decided);

    /// <inheritdoc/>
    public Task<FeedbackGiver?> Current(FeedbackGiverId id)
    {
        PlainReads++;
        return Task.FromResult(_states.GetValueOrDefault(id));
    }

    /// <inheritdoc/>
    public Task<FeedbackGiver?> ForDecision(FeedbackGiverId id, CancellationToken cancellationToken = default)
    {
        DecisionReads++;
        return Task.FromResult(_states.GetValueOrDefault(id));
    }
}
#endif
