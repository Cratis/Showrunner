// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Defines where a product reads the offers of the operator's catalog from. <c>AddBoxOffice</c> provides it from the
/// offers the operator published; a product that does not use Box Office's receiving side provides its own.
/// </summary>
public interface IPublishedOffers
{
    /// <summary>
    /// Gets every offer the operator published, withdrawn or not.
    /// </summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The offers.</returns>
    Task<IEnumerable<PublishedOffer>> All(CancellationToken cancellationToken = default);
}
