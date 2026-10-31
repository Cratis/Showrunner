// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Published by Direct when a suspended tenant may use the product normally again, for example because seats
/// were granted.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
[EventType]
[Public]
public record TenantReinstated(TenantId TenantId);
