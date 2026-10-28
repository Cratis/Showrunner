// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// Who reported an issue, as the application's signed-in identity tells it. Only ever published after the
/// reporter chose to share it.
/// </summary>
/// <param name="IdentityProvider">The identity provider the reporter signed in with.</param>
/// <param name="IdentityProviderSubject">The stable subject the identity provider issued for the reporter.</param>
/// <param name="Name">The reporter's display name; personal data.</param>
/// <param name="Email">The reporter's email address; personal data.</param>
public record Reporter(
    IdentityProviderName IdentityProvider,
    IdentityProviderSubject IdentityProviderSubject,
    ReporterName Name,
    Email Email);
