// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// A person marking release notes as seen, in an application that has released 1.0.0, 1.1.0 and 1.2.0.
/// </summary>
public class a_marking_viewer : Specification
{
    protected CommandScenario<MarkReleaseNotesSeen> _scenario;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => People.SignedInAs(People.Me);

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddSingleton<IReleaseNotes>(ReleaseHistory.ThreeReleases());
        _scenario.Services.AddTransient<ISignedInViewer, SignedInViewer>();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets events happen on my stream before the command runs.
    /// </summary>
    /// <param name="events">The events.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(ViewerId.For(People.Me)).Events(events);
}
#endif
