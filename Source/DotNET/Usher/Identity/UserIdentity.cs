// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Identity;

/// <summary>
/// Who a person is at the identity provider they signed in with: the provider and the subject it issued them. A
/// subject is unique only within its provider, so two people can have the same subject at two providers; Usher
/// compares the pair wherever it decides whose an issue is or who acts.
/// </summary>
/// <param name="IdentityProvider">The identity provider; <see cref="IdentityProviderName.NotSet"/> when the principal names none.</param>
/// <param name="Subject">The subject the identity provider issued.</param>
public record UserIdentity(IdentityProviderName IdentityProvider, IdentityProviderSubject Subject)
{
    /// <summary>
    /// Represents nobody: no provider and no subject.
    /// </summary>
    public static readonly UserIdentity NotSet = new(IdentityProviderName.NotSet, IdentityProviderSubject.NotSet);

    /// <summary>
    /// Gets whether the identity names somebody: it has a subject.
    /// </summary>
    public bool IsSet => !string.IsNullOrEmpty(Subject.Value);

    /// <summary>
    /// Gets the identity of a reporter as they shared who they are.
    /// </summary>
    /// <param name="reporter">The <see cref="Reporter"/>.</param>
    /// <returns>The <see cref="UserIdentity"/>.</returns>
    public static UserIdentity Of(Reporter reporter) => new(reporter.IdentityProvider, reporter.IdentityProviderSubject);
}
