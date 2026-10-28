// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// Published by the application when a reporter describes an issue by hand, without an error to attach. It opens
/// the issue for the desk; who the reporter is follows as <see cref="Identity.IssueAttributedToReporter"/> only if
/// they chose to share it.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Location">Where in the application the reporter was when they reported it.</param>
/// <param name="CorrelationIds">The correlation ids of the commands and queries recently run in the reporter's view; may be empty.</param>
/// <param name="Summary">The reporter's one-line summary.</param>
/// <param name="Description">The reporter's account of what happened.</param>
/// <param name="ApplicationName">The application the issue was reported in.</param>
/// <param name="ApplicationVersion">The version of the application the reporter was running.</param>
[EventType]
[Public]
public record IssueReported(
    TenantId TenantId,
    SliceLocation Location,
    IEnumerable<CorrelationId> CorrelationIds,
    Summary Summary,
    Description Description,
    ApplicationName ApplicationName,
    ApplicationVersion ApplicationVersion);
