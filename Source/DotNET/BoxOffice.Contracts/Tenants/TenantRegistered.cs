// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by Direct when an organization is registered as a tenant of the product. The product creates the
/// tenant and answers with <see cref="TenantProvisioned"/>. Replaces Direct's CustomerOrganizationRegistered
/// and the v1 OrganizationRegisteredForTenant.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Name">The name of the organization.</param>
/// <param name="Creator">Who registered the organization.</param>
/// <param name="CreatorRole">The role the creator holds in the tenant.</param>
[EventType]
[Public]
public record TenantRegistered(TenantId TenantId, TenantName Name, Person Creator, TenantRole CreatorRole);
