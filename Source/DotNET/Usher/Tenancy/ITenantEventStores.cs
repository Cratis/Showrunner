// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Usher.Tenancy;

/// <summary>
/// Defines access to the application's event store in the namespace of a tenant, for work that names its tenant
/// itself rather than taking it from the request.
/// </summary>
public interface ITenantEventStores
{
    /// <summary>
    /// Gets the event store of the namespace a tenant names, when the namespace exists. A tenant never creates a
    /// namespace here.
    /// </summary>
    /// <param name="tenant">The <see cref="TenantId"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The event store, or <see langword="null"/> when no namespace is named so.</returns>
    Task<IEventStore?> For(TenantId tenant, CancellationToken cancellationToken = default);
}
