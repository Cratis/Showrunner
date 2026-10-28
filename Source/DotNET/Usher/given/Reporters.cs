// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// The people the specifications act as.
/// </summary>
public static class Reporters
{
    /// <summary>
    /// The subject of the reporter the specifications act as.
    /// </summary>
    public static readonly IdentityProviderSubject Subject = "8c2f6a1e-reporter";

    /// <summary>
    /// The subject of someone else.
    /// </summary>
    public static readonly IdentityProviderSubject SomeoneElse = "d41b77c0-someone-else";

    /// <summary>
    /// The identity provider the reporter signs in with.
    /// </summary>
    public static readonly IdentityProviderName IdentityProvider = "entra";

    /// <summary>
    /// Another identity provider.
    /// </summary>
    public static readonly IdentityProviderName AnotherIdentityProvider = "github";

    /// <summary>
    /// Who the reporter is at their identity provider.
    /// </summary>
    public static readonly UserIdentity Identity = new(IdentityProvider, Subject);

    /// <summary>
    /// Who someone else is at the reporter's identity provider.
    /// </summary>
    public static readonly UserIdentity SomeoneElsesIdentity = new(IdentityProvider, SomeoneElse);

    /// <summary>
    /// Somebody else with the reporter's subject, at another identity provider.
    /// </summary>
    public static readonly UserIdentity SameSubjectAtAnotherProvider = new(AnotherIdentityProvider, Subject);

    /// <summary>
    /// The reporter as they are shared with the desk.
    /// </summary>
    public static readonly Reporter Reporter = new(IdentityProvider, Subject, "Ada Lovelace", "ada@example.com");

    /// <summary>
    /// Creates a signed-in principal.
    /// </summary>
    /// <param name="subject">The subject to sign in as.</param>
    /// <param name="identityProvider">The identity provider to sign in with; the reporter's by default.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal SignedInAs(IdentityProviderSubject subject, IdentityProviderName? identityProvider = null) => new(new ClaimsIdentity(
        [
            new Claim("sub", subject),
            new Claim("name", (string)Reporter.Name),
            new Claim("email", (string)Reporter.Email),
            new Claim(MicrosoftIdentityPlatformClaims.IdentityProvider, (string)(identityProvider ?? IdentityProvider))
        ],
        "Specifications"));

    /// <summary>
    /// Creates an anonymous principal.
    /// </summary>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    /// <summary>
    /// The session support signs in as the reporter in.
    /// </summary>
    public static readonly Access.Sessions.SupportSessionId SessionId = "a-session";

    /// <summary>
    /// Gets a support session on an issue as AuthProxy vouches for it, and as the issue's events decide it.
    /// </summary>
    /// <param name="issueId">The issue the session is for.</param>
    /// <param name="refusal">Why the session is not live; <see langword="null"/> for a live session.</param>
    /// <returns>The session.</returns>
    public static Access.OnBehalf.VerifiedForward SessionOn(IssueId issueId, Access.Sessions.SupportSessionRefusal? refusal) =>
        new(new(Subject, "support-person", IdentityProvider, "tenant", issueId.Value.ToString(), SessionId), refusal);

    /// <summary>
    /// Gets a live support session on an issue.
    /// </summary>
    /// <param name="issueId">The issue the session is for.</param>
    /// <returns>The session.</returns>
    public static Access.OnBehalf.VerifiedForward LiveSessionOn(IssueId issueId) => SessionOn(issueId, null);

    public static ClaimsPrincipal SupportSignedInAs(IdentityProviderSubject subject)
    {
        var principal = SignedInAs(subject);
        ((ClaimsIdentity)principal.Identity!).Actor = new ClaimsIdentity(
            [
                new Claim("sub", "support-person"),
                new Claim(ClaimTypes.Name, "grace@support.example.com")
            ],
            "Specifications");
        return principal;
    }
}

public class FixedForward(Func<Access.OnBehalf.VerifiedForward?> session) : Access.OnBehalf.IOnBehalfForward
{
    public Access.OnBehalf.VerifiedForward? Current => session();
}

/// <summary>
/// An <see cref="ICurrentPrincipalAccessor"/> that gives a fixed principal.
/// </summary>
/// <param name="principal">The principal.</param>
public class FixedPrincipal(ClaimsPrincipal principal) : ICurrentPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Current => principal;
}
#endif
