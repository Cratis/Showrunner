// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Published by the desk when support has looked at a newly reported issue. A triaged issue is decided next:
/// <see cref="IssueAccepted"/> or <see cref="IssueClosedAsWontFix"/>.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
[EventType]
[Public]
public record IssueTriaged(TenantId TenantId);
