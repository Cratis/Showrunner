// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Cratis.Showrunner.Marquee.Viewing;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Marquee.given;

/// <summary>
/// A query scenario for the announcements a person is to see, as a signed-in person of a tenant, with the announcements
/// the product holds and the module and plan entitlements a specification gives it.
/// </summary>
public class an_announcements_query : Specification
{
    protected QueryScenario<AnnouncementForMe> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as: the person, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Viewers.SignedInAs(Viewers.Me);

    /// <summary>
    /// Gets the announcements the product holds.
    /// </summary>
    protected virtual IEnumerable<StandingAnnouncement> Held => [];

    /// <summary>
    /// Gets the tenant of the request.
    /// </summary>
    protected virtual TenantId Tenant => ProductAnnouncements.Tenant;

    /// <summary>
    /// Gets what answers module and plan membership.
    /// </summary>
    protected virtual IAudienceEntitlements Entitlements => new FixedEntitlements([], []);

    /// <summary>
    /// Gets the announcements in the result.
    /// </summary>
    protected IEnumerable<AnnouncementForMe> Announcements => (IEnumerable<AnnouncementForMe>)_result.Data;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddSingleton<IStandingAnnouncements>(new FakeStandingAnnouncements([.. Held]));
        _scenario.Services.AddSingleton<ICurrentTenant>(new FixedTenant(Tenant));
        _scenario.Services.AddTransient<ISignedInPerson, SignedInPerson>();
        _scenario.Services.AddSingleton(Entitlements);
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds what the tenant recorded about an announcement.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(AnnouncementId announcementId, params object[] events) => _scenario.Given.ForEventSource(announcementId).Events(ProductAnnouncements.Then(events));

    /// <summary>
    /// Asks for the announcements of the person.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task Ask() => _result = await _scenario.Perform(nameof(AnnouncementForMe.AnnouncementsForMe), new QueryArguments());
}
#endif
