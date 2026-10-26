// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Issues;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.ReportError;

/// <summary>
/// The reporter reports an error they hit, optionally saying what they were doing. They can let support know who they
/// are and let support sign in as them for a while; granting access always shares who they are.
/// </summary>
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
    /// Handles the command.
    /// </summary>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <param name="options">The <see cref="UsherOptions"/> naming the application.</param>
    /// <param name="fingerprints">The <see cref="IComputeErrorFingerprints"/> computing the error's fingerprint.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> a grant starts from.</param>
    /// <returns>
    /// <see cref="ReporterFlaggedError"/>, then <see cref="ReporterAddedDetails"/>, <see cref="ReporterSharedIdentity"/>
    /// and <see cref="ReporterGrantedAccess"/> when present.
    /// </returns>
    public IEnumerable<object> Handle(
        ISignedInReporter reporter,
        IOptions<UsherOptions> options,
        IComputeErrorFingerprints fingerprints,
        TimeProvider clock)
    {
        yield return new ReporterFlaggedError(
            Location,
            CorrelationIds,
            StackFrames,
            ErrorMessage,
            ErrorType,
            fingerprints.Compute(ApplicationVersion, Location, ErrorType, ErrorMessage, StackFrames),
            options.Value.ApplicationName,
            ApplicationVersion,
            reporter.Subject);

        if (!string.IsNullOrWhiteSpace(Details))
        {
            yield return new ReporterAddedDetails(Details);
        }

        if (ShareIdentity || GrantAccess)
        {
            yield return new ReporterSharedIdentity(reporter.Reporter);
        }

        if (GrantAccess)
        {
            yield return new ReporterGrantedAccess(AccessDuration.From(clock.GetUtcNow()));
        }
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
        RuleFor(c => (string)c.ErrorMessage).NotEmpty().WithMessage("The error to report has no message.");
        RuleFor(c => c.AccessDuration).IsInEnum().WithMessage("Choose how long support may sign in as you.");
    }
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
/// <param name="ApplicationVersion">The version of the application the reporter runs.</param>
/// <param name="ReporterSubject">
/// The reporter's subject at their identity provider. It is how the reporter finds their own issues even when they do
/// not share who they are; it is a pseudonymous id and never published to the desk.
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
    IdentityProviderSubject ReporterSubject);
