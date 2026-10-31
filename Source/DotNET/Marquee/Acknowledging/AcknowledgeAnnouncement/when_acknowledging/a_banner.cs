// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement.when_acknowledging;

/// <summary>
/// A banner can not be dismissed, so it can not be acknowledged.
/// </summary>
public class a_banner : an_acknowledging_person
{
    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.Banner(_announcementId)];

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeAnnouncement(_announcementId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
