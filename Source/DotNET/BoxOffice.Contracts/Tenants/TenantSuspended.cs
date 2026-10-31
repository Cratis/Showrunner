// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by Direct when a tenant is suspended. The product becomes read-only for it until <see
/// cref="TenantReinstated"/>.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Cause">Why the tenant is suspended.</param>
[EventType]
[Public]
public record TenantSuspended(TenantId TenantId, SuspensionCause Cause);
