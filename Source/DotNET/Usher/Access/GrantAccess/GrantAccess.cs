// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Access.GrantAccess;

/// <summary>
/// The reporter lets support sign in as them, for a limited time, to look into one of their issues. Support can only
/// act as someone it knows, so granting access shares the reporter's identity first when it is not shared yet.
/// </summary>
/// <param name="IssueId">The issue support may look into.</param>
/// <param name="AccessDuration">How long support may sign in as the reporter.</param>
[Command]
[Authorize]
public record GrantAccess(IssueId IssueId, AccessDuration AccessDuration)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="issue">The issue.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> the grant starts from.</param>
    /// <returns><see cref="ReporterSharedIdentity"/> when identity is not shared yet, then <see cref="ReporterGrantedAccess"/>.</returns>
    public IEnumerable<object> Handle(Issue? issue, ISignedInReporter reporter, TimeProvider clock)
    {
        if (issue is not { IdentityShared: true })
        {
            yield return new ReporterSharedIdentity(reporter.Reporter);
        }

        yield return new ReporterGrantedAccess(AccessDuration.From(clock.GetUtcNow()));
    }
}

/// <summary>
/// Validates <see cref="GrantAccess"/>.
/// </summary>
public class GrantAccessValidator : CommandValidator<GrantAccess>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GrantAccessValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> deciding whether a grant is still live.</param>
    public GrantAccessValidator(Issue? issue, ISignedInReporter reporter, TimeProvider clock)
    {
        RuleFor(c => c.IssueId)
            .MustBeReportedBy(issue, reporter)
            .MustNotBeWithdrawn(issue)
            .MustBeTheReporterThemselves(reporter)
            .Must(_ => issue?.HasLiveGrant(clock.GetUtcNow()) != true)
            .WithMessage("Support already has access to this issue. Revoke it before granting it again.");
        RuleFor(c => c.AccessDuration).IsInEnum().WithMessage("Choose how long support may sign in as you.");
    }
}

/// <summary>
/// The reporter let support sign in as them until a fixed time to look into the issue. It is published as
/// <c>AccessGranted</c>; the grant ends with <c>ReporterRevokedAccess</c> or, when the time passes,
/// <c>AccessGrantLapsed</c>.
/// </summary>
/// <param name="ValidUntil">When the grant ends.</param>
[EventType]
public record ReporterGrantedAccess(ValidUntil ValidUntil);
