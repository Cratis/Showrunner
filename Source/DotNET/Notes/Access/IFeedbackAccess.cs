// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Answers whether a user may give feedback with Notes. In <see cref="FeedbackAccessMode.Everyone"/> every signed-in user may;
/// in <see cref="FeedbackAccessMode.GrantedUsers"/> only the users a desk granted the right to, and not since revoked.
/// </summary>
/// <remarks>
/// A host that gates something of its own on the same decision - a menu entry, a page - injects this.
/// </remarks>
public interface IFeedbackAccess
{
    /// <summary>
    /// Gets whether the signed-in user may give feedback. It is an advisory answer for queries and hosts, read from the
    /// tenant's current state; the commands decide with <see cref="Permit"/>. When everyone may give feedback it is always
    /// true, signed in or not. Otherwise nobody signed in may not.
    /// </summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>True when the user may give feedback.</returns>
    Task<bool> MayGiveFeedback(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether a user may give feedback, as the tenant's current state tells it. An advisory answer.
    /// </summary>
    /// <param name="giver">Who the user is.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>True when the user may give feedback.</returns>
    Task<bool> MayGiveFeedback(FeedbackGiverIdentity giver, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decides inside a command that gives feedback whether the signed-in user may. The decision reads the user's right to
    /// give feedback so that it guards the append: a revocation that lands before the note is appended refuses the note.
    /// </summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The permit, or a refusal that is the same whether the user was never granted or was revoked.</returns>
    Task<Result<FeedbackPermit, ValidationResult>> Permit(CancellationToken cancellationToken = default);
}
