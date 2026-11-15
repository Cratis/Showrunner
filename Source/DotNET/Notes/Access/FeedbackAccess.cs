// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Identity;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Represents an implementation of <see cref="IFeedbackAccess"/> that decides on <see cref="FeedbackGiver"/>.
/// </summary>
/// <param name="options">The <see cref="NotesOptions"/>.</param>
/// <param name="signedIn">The signed-in user.</param>
/// <param name="givers">Reads what the desk decided about a user.</param>
public class FeedbackAccess(
    IOptions<NotesOptions> options,
    ISignedInGiver signedIn,
    IFeedbackGivers givers) : IFeedbackAccess
{
    /// <summary>
    /// The refusal of a user who may not give feedback. It is one message for a user who was never granted and for one who was
    /// revoked, so it tells nothing about the desk's decisions.
    /// </summary>
    public const string Refusal = "Giving feedback is not available to you.";

    /// <inheritdoc/>
    public Task<bool> MayGiveFeedback(CancellationToken cancellationToken = default)
    {
        if (options.Value.AccessMode == FeedbackAccessMode.Everyone)
        {
            return Task.FromResult(true);
        }

        return signedIn.IsSignedIn ? MayGiveFeedback(signedIn.Identity, cancellationToken) : Task.FromResult(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MayGiveFeedback(FeedbackGiverIdentity giver, CancellationToken cancellationToken = default)
    {
        if (options.Value.AccessMode == FeedbackAccessMode.Everyone)
        {
            return true;
        }

        return await givers.Current(FeedbackGiverId.For(giver)) is { IsGranted: true };
    }

    /// <inheritdoc/>
    public async Task<Result<FeedbackPermit, ValidationResult>> Permit(CancellationToken cancellationToken = default)
    {
        if (!signedIn.IsSignedIn)
        {
            return ValidationResult.Error("Sign in to give a note.");
        }

        var permit = new FeedbackPermit(signedIn.Giver);
        if (options.Value.AccessMode == FeedbackAccessMode.Everyone)
        {
            return permit;
        }

        var decided = await givers.ForDecision(FeedbackGiverId.For(signedIn.Identity), cancellationToken);
        return decided is { IsGranted: true } ? permit : ValidationResult.Error(Refusal);
    }
}
