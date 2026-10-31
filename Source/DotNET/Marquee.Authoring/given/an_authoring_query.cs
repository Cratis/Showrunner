// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Marquee.Authoring.given;

/// <summary>
/// A query scenario for the announcement read model, run as an operator the host's authoring policy admits unless the
/// specification says otherwise.
/// </summary>
public class an_authoring_query : Specification
{
    protected QueryScenario<Announcement> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as: an operator, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Operators.Operator;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddArcAuthorizationPolicy<AdmitOperators>(MarqueeAuthoring.Policy);
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on an announcement.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="events">The events, in the order they happened; arrays of events are flattened.</param>
    protected void Given(AnnouncementId announcementId, params object[] events) => _scenario.Given.ForEventSource(announcementId).Events(AnnouncementEvents.Then(events));
}
#endif
