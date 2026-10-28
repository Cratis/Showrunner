// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.KnownIssues;

/// <summary>
/// Published by the desk when support links an issue to a known issue, because it is a duplicate of something the
/// desk already tracks.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="KnownIssueId">The known issue it is linked to.</param>
/// <param name="Title">The title of the known issue, as the reporter is meant to see it.</param>
[EventType]
[Public]
public record IssueLinkedToKnownIssue(TenantId TenantId, KnownIssueId KnownIssueId, KnownIssueTitle Title);
