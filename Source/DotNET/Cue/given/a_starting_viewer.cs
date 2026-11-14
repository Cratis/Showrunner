// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Cue.Starting.StartTour;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Cue.given;

/// <summary>
/// A command scenario for starting a tour, as a signed-in person of an application that has the tours a specification
/// gives it.
/// </summary>
public class a_starting_viewer : Specification
{
    protected static readonly TourId EventModeling = new("event-modeling");
    protected static readonly TourId Brainstorming = new("brainstorming");

    protected CommandScenario<StartTour> _scenario;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as: the person, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => People.SignedInAs(People.Me);

    /// <summary>
    /// Gets the tours the application has; none means it accepts any.
    /// </summary>
    protected virtual IEnumerable<string> Tours => [EventModeling.Value, Brainstorming.Value];

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddSingleton(Options.Create(new CueOptions { Tours = [.. Tours] }));
        _scenario.Services.AddTransient<ISignedInViewer, SignedInViewer>();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds what the application recorded about the person before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(ViewerId.For(People.Me)).Events(events);
}
#endif
