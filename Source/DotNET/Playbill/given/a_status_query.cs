// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Cratis.Showrunner.Playbill.Seeing;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// The status query, in an application that has released 1.0.0, 1.1.0 and 1.2.0.
/// </summary>
public class a_status_query : Specification
{
    protected QueryScenario<ReleaseNotesStatus> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => People.SignedInAs(People.Me);

    /// <summary>
    /// Gets the one status the query gave.
    /// </summary>
    protected ReleaseNotesStatus Status => ((IEnumerable<ReleaseNotesStatus>)_result.Data).Single();

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddSingleton<IReleaseNotes>(ReleaseHistory.ThreeReleases());
        _scenario.Services.AddTransient<ISignedInViewer, SignedInViewer>();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets events happen on a person's stream before the query runs.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <param name="events">The events.</param>
    protected void Given(ViewerSubject viewer, params object[] events) => _scenario.Given.ForEventSource(ViewerId.For(viewer)).Events(events);

    /// <summary>
    /// Asks for my status.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task Ask() => _result = await _scenario.Perform(nameof(ReleaseNotesStatus.ForMe), new QueryArguments());
}
#endif
