// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Issues;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.ReportIssue;

/// <summary>
/// The reporter describes a problem by hand. They can let support know who they are and let support sign in as them
/// for a while; granting access always shares who they are.
/// </summary>
/// <param name="IssueId">The id of the new issue, chosen by the caller so a retried report does not open a second issue.</param>
/// <param name="Location">Where in the application the reporter was.</param>
/// <param name="CorrelationIds">The correlation ids of the commands and queries run just before the report; may be empty.</param>
/// <param name="Summary">A short summary of the problem.</param>
/// <param name="Description">What happened.</param>
/// <param name="ApplicationVersion">The version of the application the reporter runs.</param>
/// <param name="ShareIdentity">Whether to let support know who the reporter is.</param>
/// <param name="GrantAccess">Whether to let support sign in as the reporter.</param>
/// <param name="AccessDuration">How long support may sign in as the reporter, when access is granted.</param>
[Command]
[Authorize]
public record ReportIssue(
    IssueId IssueId,
    SliceLocation Location,
    IEnumerable<CorrelationId> CorrelationIds,
    Summary Summary,
    Description Description,
    ApplicationVersion ApplicationVersion,
    bool ShareIdentity,
    bool GrantAccess,
    AccessDuration AccessDuration)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <param name="options">The <see cref="UsherOptions"/> naming the application.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> a grant starts from.</param>
    /// <returns><see cref="ReporterDescribedIssue"/>, then <see cref="ReporterSharedIdentity"/> and <see cref="ReporterGrantedAccess"/> when chosen.</returns>
    public IEnumerable<EventForEventSourceId> Handle(ISignedInReporter reporter, IOptions<UsherOptions> options, TimeProvider clock)
    {
        yield return Fact(new ReporterDescribedIssue(
            Location,
            CorrelationIds,
            Summary,
            Description,
            options.Value.ApplicationName,
            ApplicationVersion,
            reporter.Identity));

        if (ShareIdentity || GrantAccess)
        {
            yield return Fact(new ReporterSharedIdentity(reporter.Reporter));
        }

        if (GrantAccess)
        {
            yield return Fact(new ReporterGrantedAccess(AccessDuration.From(clock.GetUtcNow())));
        }

        EventForEventSourceId Fact(object fact) => ReporterSubjects.Fact(IssueId, reporter.Identity, fact);
    }
}

/// <summary>
/// Validates <see cref="ReportIssue"/>.
/// </summary>
public class ReportIssueValidator : CommandValidator<ReportIssue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportIssueValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue with the command's id, <see langword="null"/> when it does not exist yet.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public ReportIssueValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId).Must(_ => issue is null).WithMessage("An issue with this id is already reported.");
        RuleFor(c => c.IssueId).Must(_ => reporter.IsSignedIn).WithMessage("Sign in to report an issue.");
        RuleFor(c => c.IssueId).MustBeReportedByTheReporterThemselves(reporter);
        RuleFor(c => (string)c.Summary)
            .NotEmpty().WithMessage("Summarize the problem.")
            .MaximumLength(ReportLimits.SummaryLength).WithMessage($"Keep the summary within {ReportLimits.SummaryLength} characters.");
        RuleFor(c => (string)c.Description)
            .NotEmpty().WithMessage("Describe what happened.")
            .MaximumLength(ReportLimits.TextLength).WithMessage($"Keep the description within {ReportLimits.TextLength} characters.");
        RuleFor(c => c.CorrelationIds)
            .Must(_ => _ is null || _.Count() <= ReportLimits.CorrelationIds).WithMessage($"A report can carry at most {ReportLimits.CorrelationIds} correlation ids.");
        RuleFor(c => c.AccessDuration).IsInEnum().WithMessage("Choose how long support may sign in as you.");
    }
}

/// <summary>
/// The reporter described a problem by hand, which opens the issue. It is published as <c>IssueReported</c>.
/// </summary>
/// <param name="Location">Where in the application the reporter was.</param>
/// <param name="CorrelationIds">The correlation ids of the commands and queries run just before the report; may be empty.</param>
/// <param name="Summary">A short summary of the problem.</param>
/// <param name="Description">What happened.</param>
/// <param name="ApplicationName">The application, as configured in <c>AddUsher</c>.</param>
/// <param name="ApplicationVersion">The version of the application the reporter runs.</param>
/// <param name="ReportedBy">
/// Who the reporter is at their identity provider: the provider and the subject. It is how the reporter finds their own
/// issues even when they do not share who they are; it is a pseudonymous id and never published to the desk.
/// </param>
[EventType]
public record ReporterDescribedIssue(
    SliceLocation Location,
    IEnumerable<CorrelationId> CorrelationIds,
    Summary Summary,
    Description Description,
    ApplicationName ApplicationName,
    ApplicationVersion ApplicationVersion,
    UserIdentity ReportedBy);
