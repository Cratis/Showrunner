// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.Contracts.Seats;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// The product's own fact that it started a tenant on its default offering. It is recorded in the tenant's stream, so the
/// tenant has the offer's seats and modules at once, until an operator decision made later says otherwise. Private to the
/// product: the operator is told <c>TenantStartedOnOffer</c> instead.
/// </summary>
/// <param name="Offer">The name of the offer the tenant started on.</param>
/// <param name="Seats">The seats the offer includes.</param>
/// <param name="Modules">The modules the offer includes.</param>
[EventType]
public record TenantStartedOnDefaultOffering(OfferName Offer, SeatCount Seats, IEnumerable<ModuleId> Modules);
