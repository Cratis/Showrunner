// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;

namespace Cratis.Showrunner.BoxOffice.Contracts.Offering;

/// <summary>
/// Published by the product, once per change, to say which offer of the operator's catalog a new tenant starts on
/// without an operator decision. Appended under the product's id, next to <see cref="OfferingDeclared"/>.
/// </summary>
/// <param name="Offer">The name of the offer a new tenant starts on.</param>
[EventType]
[Public]
public record DefaultOfferingDeclared(OfferName Offer);
