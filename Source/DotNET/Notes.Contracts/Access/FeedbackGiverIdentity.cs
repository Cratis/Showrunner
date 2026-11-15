// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Notes.Contracts.Access;

/// <summary>
/// Who may give feedback: a user of the application, as the application's identity provider tells it. A subject is
/// unique only within its provider, so the pair identifies the user. It is the same pair Usher uses for a reporter.
/// </summary>
/// <param name="IdentityProvider">The identity provider the user signs in to the application with, for example <c>aad</c> or <c>github</c>.</param>
/// <param name="Subject">The stable subject the identity provider issued for the user; personal data.</param>
public record FeedbackGiverIdentity(IdentityProviderName IdentityProvider, IdentityProviderSubject Subject);
