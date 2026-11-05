// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by the product when it refused to create the tenant, for a reason that retrying will not change, such
/// as a name it already uses. A failure that is worth another attempt is not published: the product retries it.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Reason">Why the product did not create the tenant.</param>
[EventType]
[Public]
public record TenantNotProvisioned(TenantId TenantId, NotProvisionedReason Reason);
