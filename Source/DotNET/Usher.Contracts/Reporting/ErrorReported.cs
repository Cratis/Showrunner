// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// Published by the application when a reporter reports an error they hit. It opens the issue for the desk; what
/// the reporter wrote alongside it follows as <see cref="Conversation.DetailsAdded"/>, and who they are follows as
/// <see cref="Identity.IssueAttributedToReporter"/> only if they chose to share it.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Location">Where in the application the error happened.</param>
/// <param name="CorrelationIds">The correlation ids of the failing command or query and those run just before it; may be empty.</param>
/// <param name="StackFrames">The stack trace of the error, top frame first; empty when the error had none.</param>
/// <param name="ErrorMessage">The message of the error.</param>
/// <param name="ErrorType">The type of the error.</param>
/// <param name="Fingerprint">The fingerprint grouping this error with its duplicates.</param>
/// <param name="ApplicationName">The application the error happened in.</param>
/// <param name="ApplicationVersion">The version of the application the reporter was running.</param>
[EventType]
[Public]
public record ErrorReported(
    TenantId TenantId,
    SliceLocation Location,
    IEnumerable<CorrelationId> CorrelationIds,
    IEnumerable<StackFrame> StackFrames,
    ErrorMessage ErrorMessage,
    ErrorType ErrorType,
    ErrorFingerprint Fingerprint,
    ApplicationName ApplicationName,
    ApplicationVersion ApplicationVersion);
