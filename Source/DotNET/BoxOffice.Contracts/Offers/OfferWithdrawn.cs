// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// Published by Direct when an offer is no longer sold. Appended under the offer's id; tenants that already
/// have it keep it.
/// </summary>
[EventType]
[Public]
public record OfferWithdrawn();
