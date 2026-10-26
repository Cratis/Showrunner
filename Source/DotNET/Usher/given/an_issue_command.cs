// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// A command on an issue, run through the command pipeline by a signed-in reporter at a fixed time.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class an_issue_command<TCommand> : Specification
    where TCommand : class
{
    /// <summary>
    /// The time the scenario's clock is fixed at.
    /// </summary>
    protected static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    protected CommandScenario<TCommand> _scenario;
    protected IssueId _issueId;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as; the reporter by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Reporters.SignedInAs(Reporters.Subject);

    /// <summary>
    /// Gets the support session AuthProxy vouched for the request being forwarded in; none by default.
    /// </summary>
    protected virtual Access.OnBehalf.OnBehalfAssertion? Session => null;

    /// <summary>
    /// Gets a support session on the issue, as AuthProxy vouches for it.
    /// </summary>
    /// <param name="issueId">The issue the session is for.</param>
    /// <returns>The session.</returns>
    protected Access.OnBehalf.OnBehalfAssertion SessionOn(IssueId issueId) =>
        new(Reporters.Subject, "support-person", "entra", "tenant", issueId.Value.ToString(), "a-session");

    void Establish()
    {
        _issueId = IssueId.New();
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddSingleton<Access.OnBehalf.IOnBehalfForward>(new FixedForward(() => Session));
        _scenario.Services.AddUsher(usher => usher.ApplicationName = "Ledger");
        _scenario.Clock(Now);
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on the issue before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(_issueId).Events(events);

    /// <summary>
    /// Seeds the issue as reported by a reporter, followed by other events.
    /// </summary>
    /// <param name="reporter">Who reported it.</param>
    /// <param name="events">The events that followed, in the order they happened.</param>
    protected void GivenReportedBy(UserIdentity reporter, params object[] events) =>
        Given([IssueEvents.Described(reporter), .. events]);

    /// <summary>
    /// Seeds the issue as reported by the reporter, followed by other events.
    /// </summary>
    /// <param name="events">The events that followed, in the order they happened.</param>
    protected void GivenReported(params object[] events) => GivenReportedBy(Reporters.Identity, events);
}
#endif
