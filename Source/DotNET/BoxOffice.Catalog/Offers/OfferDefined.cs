// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// An offer was defined: the kind of thing it sells, what it is called and what it is for. Appended under the offer's id.
/// </summary>
/// <param name="Kind">The kind of offer.</param>
/// <param name="Name">Its name.</param>
/// <param name="Description">What it is for.</param>
[EventType]
public record OfferDefined(OfferKind Kind, OfferName Name, OfferDescription Description);
