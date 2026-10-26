// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// A query run through the query pipeline by a signed-in reporter.
/// </summary>
/// <typeparam name="TReadModel">The read model the query is on.</typeparam>
public class a_query<TReadModel> : Specification
{
    protected QueryScenario<TReadModel> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as; the reporter by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Reporters.SignedInAs(Reporters.Subject);

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddUsher(usher => usher.ApplicationName = "Ledger");
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on an issue.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(IssueId issueId, params object[] events) => _scenario.Given.ForEventSource(issueId).Events(events);
}
#endif
