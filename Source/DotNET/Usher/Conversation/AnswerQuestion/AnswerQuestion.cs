// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Conversation.AnswerQuestion;

/// <summary>
/// The reporter answers a question support asked on one of their issues.
/// </summary>
/// <param name="IssueId">The issue the question was asked on.</param>
/// <param name="QuestionId">The question being answered, as the desk minted it.</param>
/// <param name="Answer">The reporter's answer.</param>
[Command]
[Authorize]
public record AnswerQuestion(IssueId IssueId, QuestionId QuestionId, Answer Answer)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="ReporterAnsweredQuestion"/> event.</returns>
    public ReporterAnsweredQuestion Handle() => new(QuestionId, Answer);
}

/// <summary>
/// Validates <see cref="AnswerQuestion"/>.
/// </summary>
public class AnswerQuestionValidator : CommandValidator<AnswerQuestion>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnswerQuestionValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public AnswerQuestionValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId).MustBeReportedBy(issue, reporter).MustNotBeWithdrawn(issue);
        RuleFor(c => c.QuestionId)
            .Must((command, _) => issue?.OpenQuestions.Contains(command.QuestionId) == true)
            .WithMessage("Support has no open question with this id on the issue.");
        RuleFor(c => (string)c.Answer).NotEmpty().WithMessage("Write an answer.");
    }
}

/// <summary>
/// The reporter answered a question support asked. A question is answered once; it is published as
/// <c>QuestionAnswered</c>.
/// </summary>
/// <param name="QuestionId">The question answered.</param>
/// <param name="Answer">The reporter's answer.</param>
[EventType]
public record ReporterAnsweredQuestion(QuestionId QuestionId, Answer Answer);
