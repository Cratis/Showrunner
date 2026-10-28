// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Published by the desk when support decides not to fix an issue, and closes it with the reason.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Reason">Why support will not fix it.</param>
[EventType]
[Public]
public record IssueClosedAsWontFix(TenantId TenantId, WontFixReason Reason);
