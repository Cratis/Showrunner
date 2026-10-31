// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Portal;

/// <summary>
/// The operator's customer portal, where a tenant buys, tries and changes what it has.
/// </summary>
public interface ICustomerPortal
{
    /// <summary>
    /// Gets whether the product has a customer portal to send people to.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets a link that opens the customer portal for the tenant, for what the person wants to do there.
    /// </summary>
    /// <param name="intent">What the person wants to do.</param>
    /// <param name="offer">The offer the person looked at, when they did.</param>
    /// <param name="person">The person, when they are known.</param>
    /// <returns>The link.</returns>
    /// <exception cref="CustomerPortalNotConfigured">Thrown when the product has no customer portal.</exception>
    string LinkFor(PortalIntent intent, Guid? offer, SeatHolder? person);
}
