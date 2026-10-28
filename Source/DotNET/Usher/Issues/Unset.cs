// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.WorkItems;

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// The values read models use for something that has not happened, for the contract types that have no
/// <c>NotSet</c> of their own. Read models never hold <see langword="null"/>.
/// </summary>
public static class Unset
{
    /// <summary>
    /// The end of a grant that does not exist.
    /// </summary>
    public static readonly ValidUntil ValidUntil = new(DateTimeOffset.MinValue);

    /// <summary>
    /// No question.
    /// </summary>
    public static readonly QuestionId QuestionId = new(Guid.Empty);

    /// <summary>
    /// No work item.
    /// </summary>
    public static readonly WorkItem WorkItem = new(Tracker.NotSet, WorkItemReference.NotSet, WorkItemTitle.NotSet, WorkItemUrl.NotSet);

    /// <summary>
    /// A point in time that has not been reached, such as the end of a session that is still running.
    /// </summary>
    public static readonly DateTimeOffset Time = DateTimeOffset.MinValue;
}
