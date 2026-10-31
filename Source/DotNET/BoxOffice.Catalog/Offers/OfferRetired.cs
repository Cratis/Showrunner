// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// An offer was retired: it is no longer sold, and tenants that have it keep it.
/// </summary>
[EventType]
public record OfferRetired();
