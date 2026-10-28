// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Identity;

/// <summary>
/// Defines the reporter signed in to the application, read from the current principal.
/// </summary>
public interface ISignedInReporter
{
    /// <summary>
    /// Gets a value indicating whether a reporter is signed in: the principal is authenticated and has a subject.
    /// </summary>
    bool IsSignedIn { get; }

    /// <summary>
    /// Gets the reporter's subject at their identity provider, <see cref="IdentityProviderSubject.NotSet"/> when no
    /// one is signed in. It is a pseudonymous id and is what Usher matches issues to their reporter by.
    /// </summary>
    IdentityProviderSubject Subject { get; }

    /// <summary>
    /// Gets who the reporter is at their identity provider: the provider and the subject, <see cref="UserIdentity.NotSet"/>
    /// when no one is signed in. It is what Usher matches issues to their reporter by; a subject alone is unique only
    /// within its provider.
    /// </summary>
    UserIdentity Identity { get; }

    /// <summary>
    /// Gets the reporter as the desk learns them when they share who they are.
    /// </summary>
    Reporter Reporter { get; }

    /// <summary>
    /// Gets a value indicating whether support, not the reporter, is signed in as the reporter: the request was
    /// forwarded on the reporter's behalf in a support session.
    /// </summary>
    bool IsSupportActingAsReporter { get; }

    /// <summary>
    /// Gets the session support acts as the reporter in, when it is live: AuthProxy vouched for it, it is for this
    /// reporter, the grant is in effect and the session has not ended.
    /// </summary>
    /// <param name="session">The <see cref="LiveSupportSession"/>, when there is one.</param>
    /// <returns>True when support acts as the reporter in a live session.</returns>
    bool TryGetLiveSupportSession([NotNullWhen(true)] out LiveSupportSession? session);

    /// <summary>
    /// Gets whether the signed-in reporter may act on an issue they reported: always for the reporter, and for support
    /// only on the issue the session is for, while the grant is live and the session has not ended.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <returns>True when the issue may be acted on.</returns>
    bool MayActOn(IssueId issueId);
}

/// <summary>
/// Represents an implementation of <see cref="ISignedInReporter"/> that reads the claims of the current principal.
/// </summary>
/// <param name="principals">The <see cref="ICurrentPrincipalAccessor"/> giving the current principal.</param>
/// <param name="forward">The <see cref="IOnBehalfForward"/> giving the session a request was forwarded in.</param>
public class SignedInReporter(ICurrentPrincipalAccessor principals, IOnBehalfForward forward) : ISignedInReporter
{
    /// <inheritdoc/>
    public bool IsSignedIn => Principal?.Identity?.IsAuthenticated == true && Subject != IdentityProviderSubject.NotSet;

    /// <inheritdoc/>
    public IdentityProviderSubject Subject => FirstOf("sub", ClaimTypes.NameIdentifier);

    /// <inheritdoc/>
    public UserIdentity Identity => IsSignedIn ? new(FirstOf(MicrosoftIdentityPlatformClaims.IdentityProvider), Subject) : UserIdentity.NotSet;

    /// <inheritdoc/>
    public Reporter Reporter => new(
        FirstOf(MicrosoftIdentityPlatformClaims.IdentityProvider),
        Subject,
        FirstOf("name", ClaimTypes.Name),
        FirstOf("email", ClaimTypes.Email));

    /// <inheritdoc/>
    public bool IsSupportActingAsReporter => Principal?.IsActedOnBehalfOf() == true;

    ClaimsPrincipal? Principal => principals.Current;

    /// <inheritdoc/>
    public bool TryGetLiveSupportSession([NotNullWhen(true)] out LiveSupportSession? session)
    {
        // Without AuthProxy's verified assertion nothing says which issue the session is for, and without a live grant
        // and an open session the reporter's permission is gone, so support may act on none.
        session = IsSupportActingAsReporter &&
            forward.Current is { IsLive: true, Assertion: var assertion } &&
            assertion.Subject == Subject &&
            Guid.TryParse(assertion.Issue, out var issue)
                ? new LiveSupportSession(issue, assertion.SessionId)
                : null;
        return session is not null;
    }

    /// <inheritdoc/>
    public bool MayActOn(IssueId issueId) =>
        !IsSupportActingAsReporter || (TryGetLiveSupportSession(out var session) && session.IssueId == issueId);

    string FirstOf(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = Principal?.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}
