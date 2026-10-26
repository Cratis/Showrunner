// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// Events the specifications seed an issue with.
/// </summary>
public static class IssueEvents
{
    /// <summary>
    /// Where the specifications' issues are reported from.
    /// </summary>
    public static readonly SliceLocation Location = new("Invoicing", "Invoices", "SendInvoice");

    /// <summary>
    /// A problem described by hand.
    /// </summary>
    /// <param name="reporter">The subject of the reporter.</param>
    /// <returns>The event.</returns>
    public static ReporterDescribedIssue Described(IdentityProviderSubject reporter) => new(
        Location,
        [],
        "The invoice is not sent",
        "I pressed send and nothing happened.",
        "Ledger",
        "1.2.3",
        reporter);

    /// <summary>
    /// A reported error.
    /// </summary>
    /// <param name="reporter">The subject of the reporter.</param>
    /// <returns>The event.</returns>
    public static ReporterFlaggedError Flagged(IdentityProviderSubject reporter) => new(
        Location,
        [],
        [],
        "Object reference not set to an instance of an object.",
        "System.NullReferenceException",
        ErrorFingerprint.NotSet,
        "Ledger",
        "1.2.3",
        reporter);

    /// <summary>
    /// A person at the desk.
    /// </summary>
    public static readonly SupportAuthor Support = new("Grace", SupportAuthorKind.Person);

    /// <summary>
    /// A support agent.
    /// </summary>
    public static readonly SupportAuthor Agent = new("Usher Agent", SupportAuthorKind.Agent);
}
#endif
