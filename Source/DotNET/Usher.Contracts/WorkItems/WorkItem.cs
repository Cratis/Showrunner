// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// A work item in the issue tracker configured for the application, such as a GitHub issue, that the fix for an
/// issue is tracked by.
/// </summary>
/// <param name="Tracker">The tracker the work item lives in.</param>
/// <param name="Reference">The tracker's own reference to the work item.</param>
/// <param name="Title">The work item's title.</param>
/// <param name="Url">Where the reporter can open the work item; <see cref="WorkItemUrl.NotSet"/> when they cannot.</param>
public record WorkItem(Tracker Tracker, WorkItemReference Reference, WorkItemTitle Title, WorkItemUrl Url);
