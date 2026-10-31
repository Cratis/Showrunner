// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Entitlements;

/// <summary>
/// Published by Direct when a tenant no longer has a module, whether it was granted, tried or previewed.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Module">The module.</param>
[EventType]
[Public]
public record ModuleRevoked(TenantId TenantId, ModuleId Module);
