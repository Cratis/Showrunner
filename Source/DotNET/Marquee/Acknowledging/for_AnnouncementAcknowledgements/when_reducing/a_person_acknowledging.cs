// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.for_AnnouncementAcknowledgements.when_reducing;

/// <summary>
/// The person is among those who acknowledged the announcement.
/// </summary>
public class a_person_acknowledging : a_read_model<AnnouncementAcknowledgements>
{
    AnnouncementAcknowledgements _acknowledgements;

    async Task Because()
    {
        await Happened(new AnnouncementAcknowledged(_announcementId, Viewers.Me));
        _acknowledgements = _scenario.Instance;
    }

    [Fact] void should_be_for_the_announcement() => _acknowledgements.Id.ShouldEqual(_announcementId);

    [Fact] void should_have_been_acknowledged_by_the_person() => _acknowledgements.WasAcknowledgedBy(Viewers.Me).ShouldBeTrue();

    [Fact] void should_not_have_been_acknowledged_by_somebody_else() => _acknowledgements.WasAcknowledgedBy(Viewers.SomebodyElse).ShouldBeFalse();
}
#endif
