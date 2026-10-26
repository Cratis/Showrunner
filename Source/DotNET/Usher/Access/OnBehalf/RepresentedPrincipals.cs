// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Represents the default <see cref="IRepresentedPrincipals"/>: the reporter as they shared who they are, with their
/// name and email as <c>name</c> and <c>email</c> claims and no roles.
/// </summary>
public class RepresentedPrincipals : IRepresentedPrincipals
{
    /// <inheritdoc/>
    public Task<OnBehalfClientPrincipal> For(Reporter reporter, TenantId tenant)
    {
        var name = (string)reporter.Name;
        var email = (string)reporter.Email;
        IEnumerable<OnBehalfClientPrincipalClaim> claims =
        [
            .. string.IsNullOrWhiteSpace(name) ? [] : new[] { new OnBehalfClientPrincipalClaim("name", name) },
            .. string.IsNullOrWhiteSpace(email) ? [] : new[] { new OnBehalfClientPrincipalClaim("email", email) }
        ];

        return Task.FromResult(new OnBehalfClientPrincipal(
            reporter.IdentityProvider,
            reporter.IdentityProviderSubject,
            string.IsNullOrWhiteSpace(name) ? email : name,
            [],
            claims));
    }
}
