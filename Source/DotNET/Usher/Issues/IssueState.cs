// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// The state an issue is in, as the reporter sees it.
/// </summary>
/// <remarks>
/// Reported is where every issue starts and where a reopened issue returns, because a reopened issue is new work the
/// desk triages again. Resolved and WontFix close the issue on the desk's side; Withdrawn closes it on the reporter's
/// side and is final.
/// </remarks>
public enum IssueState
{
    /// <summary>
    /// The reporter reported the issue, or reopened it, and the desk has not triaged it yet.
    /// </summary>
    Reported = 0,

    /// <summary>
    /// The desk has looked at the issue and will decide whether to accept it.
    /// </summary>
    Triaged = 1,

    /// <summary>
    /// The desk accepted the issue as something to fix.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// The desk started working on the issue.
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// The desk resolved the issue.
    /// </summary>
    Resolved = 4,

    /// <summary>
    /// The desk decided not to fix the issue.
    /// </summary>
    WontFix = 5,

    /// <summary>
    /// The reporter withdrew the issue. Nothing changes the issue after this.
    /// </summary>
    Withdrawn = 6
}
