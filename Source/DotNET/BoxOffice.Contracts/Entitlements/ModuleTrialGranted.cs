// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Entitlements;

/// <summary>
/// Published by Direct when a tenant may try a module until a date.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Module">The module.</param>
/// <param name="Ends">The last day of the module trial.</param>
[EventType]
[Public]
public record ModuleTrialGranted(TenantId TenantId, ModuleId Module, TrialEnd Ends);
