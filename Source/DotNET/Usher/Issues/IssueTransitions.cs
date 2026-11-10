// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// Applies the issue's null-safe state transitions outside the read model's discoverable methods.
/// </summary>
internal static class IssueTransitions
{
    /// <summary>
    /// Opens an issue only when it has not been reported yet.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The reporting event's context.</param>
    /// <param name="reportedBy">The reporter's identity.</param>
    /// <returns>The existing or newly opened issue.</returns>
    internal static Issue Open(Issue? current, EventContext context, UserIdentity reportedBy) => current ?? new(
        new IssueId(Guid.Parse(context.EventSourceId.Value)),
        reportedBy,
        IssueState.Reported,
        false,
        [],
        false,
        Unset.ValidUntil);

    /// <summary>
    /// Changes an existing issue without inventing one for an unreported event source.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="change">The transition to apply.</param>
    /// <returns>The next state.</returns>
    internal static Issue? Change(Issue? current, Func<Issue, Issue> change) => current is null ? null : change(current);

    /// <summary>
    /// Moves an existing issue unless it was withdrawn.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="state">The state requested by the event.</param>
    /// <returns>The next state.</returns>
    internal static Issue? Move(Issue? current, IssueState state) =>
        current is null || current.State == IssueState.Withdrawn ? current : current with { State = state };

    /// <summary>
    /// Ends access only when the lapse belongs to the current grant.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="validUntil">The end of the grant that lapsed.</param>
    /// <returns>The next state.</returns>
    internal static Issue? Lapse(Issue? current, ValidUntil validUntil) =>
        current is null || current.AccessValidUntil != validUntil ? current : current with { AccessGranted = false };

    /// <summary>
    /// Opens a question once on an existing issue.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="questionId">The question support asked.</param>
    /// <returns>The next state.</returns>
    internal static Issue? Ask(Issue? current, QuestionId questionId) =>
        current?.OpenQuestions.Contains(questionId) != false
            ? current
            : current with { OpenQuestions = [.. current.OpenQuestions, questionId] };
}
