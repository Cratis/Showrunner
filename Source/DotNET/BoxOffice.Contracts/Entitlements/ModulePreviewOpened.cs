// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Entitlements;

/// <summary>
/// Published by Direct when a tenant may preview a module before it is granted.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Module">The module.</param>
/// <param name="Until">The last day of the preview.</param>
[EventType]
[Public]
public record ModulePreviewOpened(TenantId TenantId, ModuleId Module, EntitlementEnd Until);
