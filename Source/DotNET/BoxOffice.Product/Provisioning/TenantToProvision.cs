// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// What the operator asked the product to create.
/// </summary>
/// <param name="Tenant">The tenant to create: the name of its Chronicle namespace.</param>
/// <param name="Name">The name of the tenant as a person reads it.</param>
/// <param name="Creator">The person who registered the tenant.</param>
/// <param name="CreatorRole">The role the creator holds in the tenant.</param>
public record TenantToProvision(TenantId Tenant, TenantName Name, Person Creator, TenantRole CreatorRole);
