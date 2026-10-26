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
    /// indistinguishable from one that does not exist, so the id of another person's issue reveals nothing.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's issue id.</param>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeReportedBy<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Issue? issue, ISignedInReporter reporter) =>
        rule.Must(_ => issue is not null && reporter.IsSignedIn && issue.ReporterSubject == reporter.Subject)
            .WithMessage("There is no issue you reported with this id.");

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
}
