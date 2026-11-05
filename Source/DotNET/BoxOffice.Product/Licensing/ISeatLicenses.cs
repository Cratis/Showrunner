// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.Licensing;

/// <summary>
/// Answers whether a person may take a seat in a tenant, before the product gives them one. It is answered from what the
/// operator has told the product, which Box Office keeps in the tenant's own namespace: nothing calls the operator.
/// </summary>
public interface ISeatLicenses
{
    /// <summary>
    /// Asks whether a person may take a seat in a tenant.
    /// </summary>
    /// <param name="tenant">The tenant the seat is in.</param>
    /// <param name="holder">The person who would hold the seat.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the question.</param>
    /// <returns>The <see cref="SeatLicense"/>.</returns>
    Task<SeatLicense> For(TenantId tenant, SeatHolder holder, CancellationToken cancellationToken = default);
}
