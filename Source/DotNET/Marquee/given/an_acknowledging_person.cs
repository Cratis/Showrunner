// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Marquee.given;

/// <summary>
/// A command scenario for acknowledging an announcement, as a signed-in person of a tenant that holds the announcements
/// a specification gives it.
/// </summary>
public class an_acknowledging_person : Specification
{
    protected CommandScenario<AcknowledgeAnnouncement> _scenario;
    protected AnnouncementId _announcementId;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as: the person, signed in.
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

    void Establish()
    {
        _announcementId = AnnouncementId.New();
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddSingleton<IStandingAnnouncements>(new FakeStandingAnnouncements([.. Held]));
        _scenario.Services.AddSingleton<ICurrentTenant>(new FixedTenant(Tenant));
        _scenario.Services.AddTransient<ISignedInPerson, SignedInPerson>();
        _scenario.Services.AddSingleton<IAudienceEntitlements>(new FixedEntitlements([], []));
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds what the tenant recorded about the announcement before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(_announcementId).Events(ProductAnnouncements.Then(events));
}
#endif
