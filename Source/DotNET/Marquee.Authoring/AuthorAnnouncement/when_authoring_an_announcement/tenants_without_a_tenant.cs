// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// An audience of tenants needs a tenant.
/// </summary>
public class tenants_without_a_tenant : an_announcement_command<AuthorAnnouncement>
{
    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(_announcementId, AnnouncementEvents.Product, Audience.Tenants(), "Title", "Body", Display.Banner, Importance.Warning, ActiveFrom.Immediately, ActiveUntil.Indefinitely));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
