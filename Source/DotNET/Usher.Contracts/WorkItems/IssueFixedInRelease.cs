// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// Published by the desk when a released version of the application closes a work item an issue is linked to, so
/// the reporter learns which version has the fix. It is separate from <see cref="Lifecycle.IssueResolved"/> because a
/// fix is often resolved when merged and released later.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="ReleaseVersion">The version the fix shipped in.</param>
/// <param name="WorkItemReference">The work item the release closed.</param>
[EventType]
[Public]
public record IssueFixedInRelease(TenantId TenantId, ReleaseVersion ReleaseVersion, WorkItemReference WorkItemReference);
