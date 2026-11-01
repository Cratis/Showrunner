// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Thrown when a tenant is started on a default offering the operator has not published, or has withdrawn.
/// </summary>
/// <param name="offer">The name of the default offering.</param>
/// <param name="withdrawn">Whether the operator published the offer and then withdrew it.</param>
public class DefaultOfferingNotPublished(OfferName offer, bool withdrawn)
    : InvalidOperationException(withdrawn
        ? $"The default offering '{offer}' is no longer sold: the operator withdrew it. Declare another default offering."
        : $"The default offering '{offer}' is not an offer the operator has published. Check the name against the operator's catalog, or wait for the catalog to arrive.")
{
    /// <summary>
    /// Gets the name of the default offering.
    /// </summary>
    public OfferName Offer { get; } = offer;
}
