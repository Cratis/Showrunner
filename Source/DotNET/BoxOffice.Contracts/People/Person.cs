// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.People;

/// <summary>
/// A person who acts in a tenant: the one who registered it, or a seat holder.
/// </summary>
/// <param name="IdentityProvider">The identity provider that knows the person.</param>
/// <param name="IdentityProviderSubject">The subject the person is known by at the identity provider.</param>
/// <param name="Name">The person's display name.</param>
/// <param name="Email">The person's email address.</param>
public record Person(
    IdentityProviderName IdentityProvider,
    IdentityProviderSubject IdentityProviderSubject,
    PersonName Name,
    Email Email);
