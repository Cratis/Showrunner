// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Notes.Contracts.Access;

/// <summary>
/// Identifies one user's right to give feedback in one application tenant. In the application it is the event source id of
/// the facts the desk's grants and revocations are recorded as, and it never appears in an event payload. It is derived
/// from the user's identity, so the application and a desk compute the same one: a Guid taken from a SHA-256 of the
/// identity provider, in lowercase, and the subject.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record FeedbackGiverId(Guid Value) : EventSourceId<Guid>(Value)
{
    const string Prefix = "notes-feedback-giver";

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="FeedbackGiverId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator FeedbackGiverId(Guid value) => new(value);

    /// <summary>
    /// Gets the id of a user's right to give feedback.
    /// </summary>
    /// <param name="identityProvider">The identity provider the user signs in with; compared without regard to case.</param>
    /// <param name="subject">The subject the identity provider issued.</param>
    /// <returns>The <see cref="FeedbackGiverId"/>.</returns>
    public static FeedbackGiverId For(IdentityProviderName identityProvider, IdentityProviderSubject subject) =>
        new(new Guid(Hash(identityProvider, subject)[..16]));

    /// <summary>
    /// Gets the id of a user's right to give feedback.
    /// </summary>
    /// <param name="giver">Who the user is.</param>
    /// <returns>The <see cref="FeedbackGiverId"/>.</returns>
    public static FeedbackGiverId For(FeedbackGiverIdentity giver) => For(giver.IdentityProvider, giver.Subject);

    /// <summary>
    /// Gets the compliance subject of a user's grants: a one-way hash of the identity, because the subject travels
    /// unencrypted with every event. Erase the user by deleting its key in every tenant namespace.
    /// </summary>
    /// <param name="giver">Who the user is.</param>
    /// <returns>The <see cref="Subject"/>.</returns>
    public static Subject SubjectOf(FeedbackGiverIdentity giver) =>
        new($"{Prefix}-{Convert.ToHexStringLower(Hash(giver.IdentityProvider, giver.Subject))}");

    static byte[] Hash(IdentityProviderName identityProvider, IdentityProviderSubject subject) =>
        SHA256.HashData(Encoding.UTF8.GetBytes($"{Prefix}\n{identityProvider.Value.Trim().ToLowerInvariant()}\n{subject.Value}"));
}
