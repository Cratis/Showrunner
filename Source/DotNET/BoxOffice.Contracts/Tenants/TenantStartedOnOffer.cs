// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by the product when it has started a new tenant on the offer the product declared as its default,
/// without waiting for an operator decision. The operator sees which offer the tenant started on and what that gives
/// it, and may change it later. Repeating the start of a tenant publishes it once.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Offer">The name of the offer the tenant started on.</param>
/// <param name="Seats">The seats the offer includes, which the tenant starts with.</param>
/// <param name="Modules">The modules the offer includes, which the tenant starts with.</param>
[EventType]
[Public]
public record TenantStartedOnOffer(TenantId TenantId, OfferName Offer, SeatCount Seats, IEnumerable<ModuleId> Modules);
