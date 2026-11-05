// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.SignIns;

/// <summary>
/// The body of AuthProxy's sign-in callback.
/// </summary>
/// <param name="Kind">Why AuthProxy called: <c>SignedIn</c> after a sign-in, or <c>Returned</c> for a returning user's first request.</param>
/// <param name="Subject">The subject the person is known by at the identity provider.</param>
/// <param name="Name">The person's display name, empty when the provider gave none.</param>
/// <param name="Email">The person's email address, empty when the provider gave none.</param>
/// <param name="IdentityProvider">The identity provider that knows the person.</param>
/// <param name="Tenant">The tenant AuthProxy resolved, when it did.</param>
/// <param name="Occurred">When the person signed in or returned.</param>
public record SignInNotification(
    string Kind,
    string Subject,
    string? Name,
    string? Email,
    string IdentityProvider,
    string? Tenant,
    DateTimeOffset Occurred);
