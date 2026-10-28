// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Access.RevokeAccess;

/// <summary>
/// The reporter takes back the access they granted support on one of their issues, before it ends by itself.
/// </summary>
/// <param name="IssueId">The issue to revoke access on.</param>
[Command]
[Authorize]
public record RevokeAccess(IssueId IssueId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="ReporterRevokedAccess"/> event.</returns>
    public ReporterRevokedAccess Handle() => new();
}

/// <summary>
/// Validates <see cref="RevokeAccess"/>.
/// </summary>
/// <remarks>
/// It is the one command allowed on a withdrawn issue: revoking only narrows what support can do, and refusing it
/// would leave the reporter unable to take back access they granted.
/// </remarks>
public class RevokeAccessValidator : CommandValidator<RevokeAccess>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevokeAccessValidator"/> class.
    /// </summary>
    /// <param name="issue">The issue, or <see langword="null"/> when it does not exist.</param>
    /// <param name="reporter">The signed-in reporter.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> deciding whether a grant is still live.</param>
    public RevokeAccessValidator(Issue? issue, ISignedInReporter reporter, TimeProvider clock)
    {
        RuleFor(c => c.IssueId)
            .MustBeReportedBy(issue, reporter)
            .Must(_ => issue?.HasLiveGrant(clock.GetUtcNow()) != false)
            .WithMessage("Support has no access to this issue to revoke.");
    }
}

/// <summary>
/// The reporter took back the access they granted support before it ended. It is published as <c>AccessRevoked</c>.
/// </summary>
[EventType]
public record ReporterRevokedAccess;
