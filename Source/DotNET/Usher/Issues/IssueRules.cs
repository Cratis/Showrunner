// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// The rules every command on an existing issue shares, for the validators of those commands.
/// </summary>
public static class IssueRules
{
    /// <summary>
    /// The issue must exist and be reported by the signed-in reporter. An issue someone else reported is
    /// indistinguishable from one that does not exist, so the id of another person's issue reveals nothing. Support
    /// signed in as the reporter may act only on the issue the session is for; any other is answered the same way.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type the rule sees the issue id as: the concept, or the value it wraps.</typeparam>
    /// <param name="rule">The rule for the command's issue id.</param>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeReportedBy<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Issue? issue, ISignedInReporter reporter) =>
        rule.Must(issueId => issue is not null && reporter.IsSignedIn && issue.ReportedBy == reporter.Identity && reporter.MayActOn(IssueIdOf(issueId)))
            .WithMessage("There is no issue you reported with this id.");

    /// <summary>
    /// Requires that the reporter themselves, not support signed in as them, runs the command. A grant of access is the
    /// reporter's alone to give: support signed in as the reporter could otherwise give itself access, or more of it.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The rule for continuation.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeTheReporterThemselves<T, TProperty>(this IRuleBuilder<T, TProperty> rule, ISignedInReporter reporter) =>
        rule.Must(_ => !reporter.IsSupportActingAsReporter)
            .WithMessage("Only the reporter can grant support access.");

    /// <summary>
    /// The issue must not be withdrawn: nothing changes an issue after its reporter withdrew it.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's issue id.</param>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustNotBeWithdrawn<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Issue? issue) =>
        rule.Must(_ => issue is not { State: IssueState.Withdrawn })
            .WithMessage("The issue was withdrawn and can not be changed.");

    static IssueId IssueIdOf<TProperty>(TProperty value) => value switch
    {
        IssueId issueId => issueId,
        Guid id => new(id),
        _ => new(Guid.Empty)
    };
}
