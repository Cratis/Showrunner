// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Reads what the desk decided about one user's right to give feedback in the current tenant.
/// </summary>
public interface IFeedbackGivers
{
    /// <summary>
    /// Reads the state as it is now. A plain read for queries and hosts: it guards nothing.
    /// </summary>
    /// <param name="id">The user's right to give feedback.</param>
    /// <returns>The state, <see langword="null"/> when the desk never decided anything about the user.</returns>
    Task<FeedbackGiver?> Current(FeedbackGiverId id);

    /// <summary>
    /// Reads the state for a decision a command makes, so the read guards the command's append: a decision of the desk that
    /// lands before the note is appended refuses the note. Only valid inside a command.
    /// </summary>
    /// <param name="id">The user's right to give feedback.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The state, <see langword="null"/> when the desk never decided anything about the user.</returns>
    Task<FeedbackGiver?> ForDecision(FeedbackGiverId id, CancellationToken cancellationToken = default);
}
