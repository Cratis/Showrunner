// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Cue.given;

/// <summary>
/// A query scenario for the tours the signed-in person has started.
/// </summary>
public class a_my_tours_query : Specification
{
    protected QueryScenario<StartedTour> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as: the person, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => People.SignedInAs(People.Me);

    /// <summary>
    /// Gets the tours the person started.
    /// </summary>
    protected IEnumerable<string> Tours => ((IEnumerable<StartedTour>)_result.Data).Select(tour => tour.Id.Value);

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddTransient<ISignedInViewer, SignedInViewer>();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds what the application recorded about a person before the query runs.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(ViewerSubject viewer, params object[] events) => _scenario.Given.ForEventSource(ViewerId.For(viewer)).Events(events);

    protected async Task Ask() => _result = await _scenario.Perform(nameof(StartedTour.ByMe), new QueryArguments());
}
#endif
