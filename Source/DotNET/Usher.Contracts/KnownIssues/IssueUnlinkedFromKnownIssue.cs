// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.KnownIssues;

/// <summary>
/// Published by the desk when support removes the link between an issue and a known issue, because it turned out
/// not to be a duplicate.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="KnownIssueId">The known issue it is no longer linked to.</param>
[EventType]
[Public]
public record IssueUnlinkedFromKnownIssue(TenantId TenantId, KnownIssueId KnownIssueId);
