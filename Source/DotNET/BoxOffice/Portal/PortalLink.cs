// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Portal;

/// <summary>
/// A link that opens the operator's customer portal for the signed-in user's tenant.
/// </summary>
/// <param name="Url">The link, which stops working after a while.</param>
[ReadModel]
public record PortalLink(string Url)
{
    /// <summary>
    /// Gets a link to the customer portal for what the signed-in user wants to do there.
    /// </summary>
    /// <param name="intent">What the user wants to do.</param>
    /// <param name="offer">The offer the user looked at, when they did.</param>
    /// <param name="portal">The customer portal.</param>
    /// <param name="holder">The signed-in user.</param>
    /// <returns>The <see cref="PortalLink"/>.</returns>
    /// <exception cref="CustomerPortalNotConfigured">Thrown when the product has no customer portal.</exception>
    [Authorize]
    public static PortalLink LinkToPortal(PortalIntent intent, Guid? offer, ICustomerPortal portal, ICurrentSeatHolder holder) =>
        new(portal.LinkFor(intent, offer, holder.Current));
}
