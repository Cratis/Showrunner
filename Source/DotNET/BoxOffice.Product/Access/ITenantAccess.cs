// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Access;

/// <summary>
/// Answers whether a tenant is read-only, for work that is not a command, such as a job, a reactor or an endpoint. Box
/// Office's command filter already refuses the commands of a read-only tenant; this is for the rest.
/// </summary>
public interface ITenantAccess
{
    /// <summary>
    /// Asks whether a tenant is read-only: the operator suspended it, or its trial ended and no seats were granted or
    /// bought. Reading is always allowed. A tenant the product does not have is read-only, because nothing in it can change.
    /// </summary>
    /// <param name="tenant">The tenant to ask about.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the question.</param>
    /// <returns><see langword="true"/> when the tenant may be read but not changed.</returns>
    Task<bool> IsReadOnly(TenantId tenant, CancellationToken cancellationToken = default);
}
