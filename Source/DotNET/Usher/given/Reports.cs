// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// Valid reports the specifications vary one value of.
/// </summary>
public static class Reports
{
    /// <summary>
    /// A valid problem described by hand, sharing nothing.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <returns>The command.</returns>
    public static ReportIssue Issue(IssueId issueId) => new(
        issueId,
        IssueEvents.Location,
        [],
        "The invoice is not sent",
        "I pressed send and nothing happened.",
        "1.2.3",
        false,
        false,
        AccessDuration.OneHour);

    /// <summary>
    /// A valid reported error without details, sharing nothing.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <returns>The command.</returns>
    public static ReportError Error(IssueId issueId) => new(
        issueId,
        IssueEvents.Location,
        [],
        [new("Ledger.Invoicing.Send", "SendInvoice.cs", 42)],
        "Object reference not set to an instance of an object.",
        "System.NullReferenceException",
        Details.NotSet,
        "1.2.3",
        false,
        false,
        AccessDuration.OneHour);
}
#endif
