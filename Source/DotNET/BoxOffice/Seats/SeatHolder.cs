// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// Who holds a seat: a person as the identity provider knows them.
/// </summary>
/// <param name="IdentityProvider">The identity provider that vouches for the person.</param>
/// <param name="Subject">The person's subject at the identity provider.</param>
public record SeatHolder(IdentityProviderName IdentityProvider, IdentityProviderSubject Subject)
{
    /// <summary>
    /// Gets who holds a seat from the person the seat is for.
    /// </summary>
    /// <param name="person">The person.</param>
    /// <returns>The holder.</returns>
    public static SeatHolder Of(Person person) => new(person.IdentityProvider, person.IdentityProviderSubject);
}
