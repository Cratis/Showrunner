// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;

/// <summary>
/// The reporter withdraws one of their open issues; support no longer needs to look into it.
/// </summary>
/// <param name="IssueId">The issue to withdraw.</param>
[Command]
[Authorize]
public record WithdrawIssue(IssueId IssueId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="ReporterWithdrewIssue"/> event.</returns>
    public ReporterWithdrewIssue Handle() => new();
}

/// <summary>
/// Validates <see cref="WithdrawIssue"/>.
/// </summary>
public class WithdrawIssueValidator : CommandValidator<WithdrawIssue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WithdrawIssueValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public WithdrawIssueValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId)
            .MustBeReportedBy(issue, reporter)
            .MustNotBeWithdrawn(issue)
            .Must(_ => issue is not { State: IssueState.Resolved or IssueState.WontFix })
            .WithMessage("A closed issue can not be withdrawn.");
    }
}

/// <summary>
/// The reporter withdrew an issue. It is final: nothing changes the issue afterwards, except that the reporter can
/// still revoke access they granted. It is published as <c>IssueWithdrawn</c>.
/// </summary>
[EventType]
public record ReporterWithdrewIssue;
