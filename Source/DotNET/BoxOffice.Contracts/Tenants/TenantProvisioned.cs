// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by the product when it has created the tenant and it is ready to use. Direct shows the tenant as
/// set up only after this, and starts a trial's clock from it. Replaces Direct's
/// CustomerOrganizationProvisioned.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
[EventType]
[Public]
public record TenantProvisioned(TenantId TenantId);
