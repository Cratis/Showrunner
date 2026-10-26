// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Reporting.Capture;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.ReportError;

/// <summary>
/// The reporter reports an error they hit, optionally saying what they were doing. They can let support know who they
/// are and let support sign in as them for a while; granting access always shares who they are.
/// </summary>
/// <remarks>
/// When the backend captured the error of a failing command for one of <paramref name="CorrelationIds"/>, for the same
/// reporter in the same tenant, the error is recorded as the backend saw it - location, type, message, stack frames and
/// application version - and the values the client sent for those are not used for the desk or the fingerprint. The
/// reporter is still shown the message they saw, <paramref name="ErrorMessage"/>: a server's exception message can say
/// things about the backend the reporter was never shown.
/// </remarks>
/// <param name="IssueId">The id of the new issue, chosen by the caller so a retried report does not open a second issue.</param>
/// <param name="Location">Where in the application the error happened.</param>
/// <param name="CorrelationIds">The correlation ids of the failing command or query and those run just before it; may be empty.</param>
/// <param name="StackFrames">The stack trace of the error, top frame first; empty when there is none.</param>
/// <param name="ErrorMessage">The message of the error.</param>
/// <param name="ErrorType">The type of the error.</param>
/// <param name="Details">What the reporter was doing; empty when they wrote nothing.</param>
/// <param name="ApplicationVersion">The version of the application the reporter runs.</param>
/// <param name="ShareIdentity">Whether to let support know who the reporter is.</param>
/// <param name="GrantAccess">Whether to let support sign in as the reporter.</param>
/// <param name="AccessDuration">How long support may sign in as the reporter, when access is granted.</param>
[Command]
[Authorize]
public record ReportError(
    IssueId IssueId,
    SliceLocation Location,
    IEnumerable<CorrelationId> CorrelationIds,
    IEnumerable<StackFrame> StackFrames,
    ErrorMessage ErrorMessage,
    ErrorType ErrorType,
    Details Details,
    ApplicationVersion ApplicationVersion,
    bool ShareIdentity,
    bool GrantAccess,
    AccessDuration AccessDuration)
{
    /// <summary>
    /// Provides the error to record: the one the backend captured for one of the command's correlation ids, for the
    /// signed-in reporter in the current tenant, or, when it captured none, the error as the command describes it.
    /// </summary>
    /// <param name="capturedErrors">The <see cref="ICapturedErrors"/> holding errors the backend captured.</param>
    /// <param name="owner">The <see cref="ICurrentErrorOwner"/> whose captured errors the command may use.</param>
    /// <returns>The <see cref="CapturedError"/> to record.</returns>
    public CapturedError Provide(ICapturedErrors capturedErrors, ICurrentErrorOwner owner)
    {
        if (owner.TryGet(out var errorOwner))
        {
            foreach (var correlationId in CorrelationIds)
            {
                if (capturedErrors.TryGet(errorOwner, correlationId, out var captured))
                {
                    return captured;
                }
            }
        }

        return new(Location, ErrorType, ErrorMessage, StackFrames, ApplicationVersion);
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="error">The error to record, from <see cref="Provide"/>.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <param name="options">The <see cref="UsherOptions"/> naming the application.</param>
    /// <param name="fingerprints">The <see cref="IComputeErrorFingerprints"/> computing the error's fingerprint.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> a grant starts from.</param>
    /// <returns>
    /// <see cref="ReporterFlaggedError"/>, then <see cref="ReporterAddedDetails"/>, <see cref="ReporterSharedIdentity"/>
    /// and <see cref="ReporterGrantedAccess"/> when present.
    /// </returns>
    public IEnumerable<EventForEventSourceId> Handle(
        CapturedError error,
        ISignedInReporter reporter,
        IOptions<UsherOptions> options,
        IComputeErrorFingerprints fingerprints,
        TimeProvider clock)
    {
        yield return Fact(new ReporterFlaggedError(
            error.Location,
            CorrelationIds,
            error.StackFrames,
            error.ErrorMessage,
            error.ErrorType,
            fingerprints.Compute(error.ApplicationVersion, error.Location, error.ErrorType, error.ErrorMessage, error.StackFrames),
            options.Value.ApplicationName,
            error.ApplicationVersion,
            reporter.Identity,
            ErrorMessage));

        if (!string.IsNullOrWhiteSpace(Details))
        {
            yield return Fact(new ReporterAddedDetails(Details));
        }

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
/// Validates <see cref="ReportError"/>.
/// </summary>
public class ReportErrorValidator : CommandValidator<ReportError>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportErrorValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue with the command's id, <see langword="null"/> when it does not exist yet.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public ReportErrorValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId).Must(_ => issue is null).WithMessage("An issue with this id is already reported.");
        RuleFor(c => c.IssueId).Must(_ => reporter.IsSignedIn).WithMessage("Sign in to report an error.");
        RuleFor(c => c.IssueId).MustBeReportedByTheReporterThemselves(reporter);
        RuleFor(c => (string)c.ErrorMessage)
            .NotEmpty().WithMessage("The error to report has no message.")
            .MaximumLength(ReportLimits.ErrorMessageLength).WithMessage($"The error message can be at most {ReportLimits.ErrorMessageLength} characters.");
        RuleFor(c => (string)c.ErrorType)
            .MaximumLength(ReportLimits.ErrorTypeLength).WithMessage($"The error type can be at most {ReportLimits.ErrorTypeLength} characters.");
        RuleFor(c => (string)c.Details)
            .MaximumLength(ReportLimits.TextLength).WithMessage($"Keep what you were doing within {ReportLimits.TextLength} characters.");
        RuleFor(c => c.StackFrames)
            .Must(_ => _ is null || _.Count() <= ReportLimits.StackFrames).WithMessage($"A report can carry at most {ReportLimits.StackFrames} stack frames.")
            .Must(_ => _?.All(frame => frame is null || (Length(frame.Method?.Value) <= ReportLimits.StackFrameTextLength && Length(frame.File?.Value) <= ReportLimits.StackFrameTextLength)) != false)
            .WithMessage($"A stack frame's method and file can be at most {ReportLimits.StackFrameTextLength} characters.");
        RuleFor(c => c.CorrelationIds)
            .Must(_ => _ is null || _.Count() <= ReportLimits.CorrelationIds).WithMessage($"A report can carry at most {ReportLimits.CorrelationIds} correlation ids.");
        RuleFor(c => c.AccessDuration).IsInEnum().WithMessage("Choose how long support may sign in as you.");
    }

    static int Length(string? text) => text?.Length ?? 0;
}

/// <summary>
/// The reporter reported an error they hit, which opens the issue. What they wrote alongside it follows as
/// <see cref="ReporterAddedDetails"/>. It is published as <c>ErrorReported</c>.
/// </summary>
/// <param name="Location">Where in the application the error happened.</param>
/// <param name="CorrelationIds">The correlation ids of the failing command or query and those run just before it; may be empty.</param>
/// <param name="StackFrames">The stack trace of the error, top frame first; empty when there is none.</param>
/// <param name="ErrorMessage">The message of the error.</param>
/// <param name="ErrorType">The type of the error.</param>
/// <param name="Fingerprint">The fingerprint grouping reports of the same error; <see cref="ErrorFingerprint.NotSet"/> when not computed.</param>
/// <param name="ApplicationName">The application, as configured in <c>AddUsher</c>.</param>
/// <param name="ApplicationVersion">The version of the application the error happened in.</param>
/// <param name="ReportedBy">
/// Who the reporter is at their identity provider: the provider and the subject. It is how the reporter finds their own
/// issues even when they do not share who they are; it is a pseudonymous id and never published to the desk.
/// </param>
/// <param name="SeenMessage">
/// The message the reporter saw, as the client sent it. The reporter is shown it as the issue's title; it differs from
/// <paramref name="ErrorMessage"/> when the backend captured the error, and is never published to the desk.
/// </param>
[EventType]
public record ReporterFlaggedError(
    SliceLocation Location,
    IEnumerable<CorrelationId> CorrelationIds,
    IEnumerable<StackFrame> StackFrames,
    ErrorMessage ErrorMessage,
    ErrorType ErrorType,
    ErrorFingerprint Fingerprint,
    ApplicationName ApplicationName,
    ApplicationVersion ApplicationVersion,
    UserIdentity ReportedBy,
    ErrorMessage SeenMessage);
