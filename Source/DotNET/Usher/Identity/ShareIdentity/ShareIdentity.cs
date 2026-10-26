// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Identity.ShareIdentity;

/// <summary>
/// The reporter lets support know who they are on one of their issues. Who they are is taken from the signed-in
/// identity, never from the request.
/// </summary>
/// <param name="IssueId">The issue to share identity on.</param>
[Command]
[Authorize]
public record ShareIdentity(IssueId IssueId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <returns>The <see cref="ReporterSharedIdentity"/> event.</returns>
    public ReporterSharedIdentity Handle(ISignedInReporter reporter) => new(reporter.Reporter);
}

/// <summary>
/// Validates <see cref="ShareIdentity"/>.
/// </summary>
public class ShareIdentityValidator : CommandValidator<ShareIdentity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShareIdentityValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    public ShareIdentityValidator(Issue? issue, ISignedInReporter reporter)
    {
        RuleFor(c => c.IssueId)
            .MustBeReportedBy(issue, reporter)
            .MustNotBeWithdrawn(issue)
            .Must(_ => issue is not { IdentityShared: true })
            .WithMessage("You already let support know who you are on this issue.");
    }
}

/// <summary>
/// The reporter let support know who they are on an issue. Until this happens the desk knows the issue but not the
/// person; it is published as <c>IssueAttributedToReporter</c>. The name and email in <see cref="Reporter"/> are
/// personal data, encrypted for the issue as compliance subject.
/// </summary>
/// <param name="Reporter">Who the reporter is, as their identity provider knows them.</param>
[EventType]
public record ReporterSharedIdentity(Reporter Reporter);
