// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// The tenant, people and days the specifications share.
/// </summary>
public static class Tenants
{
    /// <summary>
    /// The tenant: the name of its namespace.
    /// </summary>
    public static readonly TenantId Tenant = new("fjord-logistics");

    /// <summary>
    /// The operator's organization behind the tenant.
    /// </summary>
    public static readonly OrganizationId Organization = new(new Guid(0x4f2a1c9e, 0x7b34, 0x4d0e, 0x9a, 0x51, 0x0c, 0x6d, 0x2e, 0x8b, 0x7f, 0x13));

    /// <summary>
    /// The identity provider the people sign in with.
    /// </summary>
    public static readonly IdentityProviderName Provider = new("github");

    /// <summary>
    /// The first person.
    /// </summary>
    public static readonly Person Ada = Person("1001", "Ada Lovelace", "ada@fjord.example.com");

    /// <summary>
    /// The second person.
    /// </summary>
    public static readonly Person Bo = Person("1002", "Bo Nilsen", "bo@fjord.example.com");

    /// <summary>
    /// The third person.
    /// </summary>
    public static readonly Person Cy = Person("1003", "Cy Hansen", "cy@fjord.example.com");

    /// <summary>
    /// The day the specifications run on.
    /// </summary>
    public static readonly DateOnly Today = new(2026, 11, 2);

    /// <summary>
    /// The moment the specifications run at.
    /// </summary>
    public static readonly DateTimeOffset Now = new(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// When the first event happens.
    /// </summary>
    public static readonly DateTimeOffset Start = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Creates a person.
    /// </summary>
    /// <param name="subject">The subject at the identity provider.</param>
    /// <param name="name">The name.</param>
    /// <param name="email">The email address.</param>
    /// <returns>The person.</returns>
    public static Person Person(string subject, string name, string email) => new(Provider, subject, name, email);

    /// <summary>
    /// Gets a day relative to <see cref="Today"/>.
    /// </summary>
    /// <param name="days">The number of days from today; negative for the past.</param>
    /// <returns>The day.</returns>
    public static DateOnly Day(int days) => Today.AddDays(days);

    /// <summary>
    /// Gets when the event with a sequence number happened, one minute apart from <see cref="Start"/>.
    /// </summary>
    /// <param name="minutes">The number of minutes after the start.</param>
    /// <returns>When.</returns>
    public static DateTimeOffset At(int minutes) => Start.AddMinutes(minutes);

    /// <summary>
    /// Creates the principal of a signed-in person.
    /// </summary>
    /// <param name="person">Who is signed in.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(Person person) => new(new ClaimsIdentity(
        [
            new Claim("sub", (string)person.IdentityProviderSubject),
            new Claim("name", (string)person.Name),
            new Claim("email", (string)person.Email),
            new Claim(MicrosoftIdentityPlatformClaims.IdentityProvider, (string)person.IdentityProvider)
        ],
        "Specifications"));
}

/// <summary>
/// A <see cref="ICurrentPrincipalAccessor"/> that always answers the same principal.
/// </summary>
/// <param name="principal">The principal.</param>
public class FixedPrincipal(ClaimsPrincipal principal) : ICurrentPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Current => principal;
}
#endif
