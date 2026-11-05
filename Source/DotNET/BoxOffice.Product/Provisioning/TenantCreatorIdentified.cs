// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// The product created the tenant for a person: who the tenant's people can ask for a seat.
/// </summary>
/// <param name="Creator">The person who registered the tenant.</param>
/// <param name="Role">The role the creator holds in the tenant.</param>
[EventType]
public record TenantCreatorIdentified(Person Creator, TenantRole Role);
