// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Reporting;

namespace Cratis.Showrunner.Usher.Conversation.AddDetails;

/// <summary>
/// The reporter tells support more about one of their issues.
/// </summary>
/// <param name="IssueId">The issue to add details to.</param>
/// <param name="Details">What the reporter adds.</param>
[Command]
[Authorize]
public record AddDetails(IssueId IssueId, Details Details)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The <see cref="ReporterAddedDetails"/> event, with the reporter as its compliance subject.</returns>
    public EventForEventSourceId Handle(ISignedInReporter reporter) =>
        ReporterSubjects.Fact(IssueId, reporter.Identity, new ReporterAddedDetails(Details));
}

/// <summary>
/// Validates <see cref="AddDetails"/>.
/// </summary>
public class AddDetailsValidator : CommandValidator<AddDetails>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddDetailsValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public AddDetailsValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId).MustBeReportedBy(issue, reporter).MustNotBeWithdrawn(issue);
        RuleFor(c => (string)c.Details)
            .NotEmpty().WithMessage("Tell support what you want to add.")
            .MaximumLength(ReportLimits.TextLength).WithMessage($"Keep what you add within {ReportLimits.TextLength} characters.");
    }
}

/// <summary>
/// The reporter told support more about an issue, either alongside an error report or later in the conversation. It
/// is published as <c>DetailsAdded</c>.
/// </summary>
/// <param name="Details">What the reporter added.</param>
[EventType]
public record ReporterAddedDetails(Details Details);
