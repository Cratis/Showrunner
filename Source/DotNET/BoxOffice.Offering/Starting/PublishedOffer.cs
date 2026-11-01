// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.Contracts.Seats;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// An offer of the operator's catalog as the product knows it from <c>OfferPublished</c>.
/// </summary>
/// <param name="Id">The id of the offer.</param>
/// <param name="Name">The name of the offer.</param>
/// <param name="Modules">The modules the offer includes.</param>
/// <param name="IncludedSeats">The seats the offer includes.</param>
/// <param name="Withdrawn">Whether the operator stopped selling the offer.</param>
public record PublishedOffer(OfferId Id, OfferName Name, IEnumerable<ModuleId> Modules, SeatCount IncludedSeats, bool Withdrawn);
