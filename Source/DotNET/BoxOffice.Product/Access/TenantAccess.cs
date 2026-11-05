// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;

namespace Cratis.Showrunner.BoxOffice.Product.Access;

/// <summary>
/// Answers whether a tenant is read-only from what the operator decided, as Box Office's own command filter does.
/// </summary>
/// <param name="stores">The <see cref="ITenantStores"/> that finds the tenant.</param>
/// <param name="time">The <see cref="TimeProvider"/> the days are counted by.</param>
public class TenantAccess(ITenantStores stores, TimeProvider time) : ITenantAccess
{
    /// <inheritdoc/>
    public async Task<bool> IsReadOnly(TenantId tenant, CancellationToken cancellationToken = default) =>
        await stores.For(tenant, cancellationToken) is not { } store ||
        await new TenantEntitlements(store.ReadModels, time).WhyReadOnly() != ReadOnlyReason.None;
}
