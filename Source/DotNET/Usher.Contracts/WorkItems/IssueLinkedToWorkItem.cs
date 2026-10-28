// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// Published by the desk when support links an issue to the work item its fix is tracked by.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="WorkItem">The work item the issue is linked to.</param>
[EventType]
[Public]
public record IssueLinkedToWorkItem(TenantId TenantId, WorkItem WorkItem);
