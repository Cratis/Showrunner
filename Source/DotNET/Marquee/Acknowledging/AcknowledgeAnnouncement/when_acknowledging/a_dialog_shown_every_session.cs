// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement.when_acknowledging;

/// <summary>
/// Looking is not a fact: a dialog shown every session records nothing, and the person is not told it failed.
/// </summary>
public class a_dialog_shown_every_session : an_acknowledging_person
{
    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogEverySession(_announcementId)];

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeAnnouncement(_announcementId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
