// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by Direct when an organization is being deleted. The product tears the tenant down. Replaces the
/// v1 TenantDeleted.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
[EventType]
[Public]
public record TenantClosing(TenantId TenantId);
