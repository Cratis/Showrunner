// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// Published by the desk when support removes the link between an issue and a work item.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="WorkItemReference">The work item the issue is no longer linked to.</param>
[EventType]
[Public]
public record IssueUnlinkedFromWorkItem(TenantId TenantId, WorkItemReference WorkItemReference);
