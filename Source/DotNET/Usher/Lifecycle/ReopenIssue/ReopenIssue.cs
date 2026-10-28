// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Reporting;

namespace Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;

/// <summary>
/// The reporter reopens one of their resolved issues because the problem is not gone.
/// </summary>
/// <param name="IssueId">The issue to reopen.</param>
/// <param name="Reason">Why the reporter reopens it.</param>
[Command]
[Authorize]
public record ReopenIssue(IssueId IssueId, ReopenReason Reason)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The <see cref="ReporterReopenedIssue"/> event, with the reporter as its compliance subject.</returns>
    public EventForEventSourceId Handle(ISignedInReporter reporter) =>
        ReporterSubjects.Fact(IssueId, reporter.Identity, new ReporterReopenedIssue(Reason));
}

/// <summary>
/// Validates <see cref="ReopenIssue"/>.
/// </summary>
public class ReopenIssueValidator : CommandValidator<ReopenIssue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReopenIssueValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public ReopenIssueValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId)
            .MustBeReportedBy(issue, reporter)
            .MustNotBeWithdrawn(issue)
            .Must(_ => issue is null or { State: IssueState.Resolved or IssueState.Withdrawn })
            .WithMessage("Only a resolved issue can be reopened.");
        RuleFor(c => (string)c.Reason)
            .NotEmpty().WithMessage("Tell support why you reopen the issue.")
            .MaximumLength(ReportLimits.ReopenReasonLength).WithMessage($"Keep the reason within {ReportLimits.ReopenReasonLength} characters.");
    }
}

/// <summary>
/// The reporter reopened a resolved issue. The issue returns to <see cref="IssueState.Reported"/>, because it is new
/// work the desk triages again; it is published as <c>IssueReopened</c>.
/// </summary>
/// <param name="Reason">Why the reporter reopened it.</param>
[EventType]
public record ReporterReopenedIssue(ReopenReason Reason);
