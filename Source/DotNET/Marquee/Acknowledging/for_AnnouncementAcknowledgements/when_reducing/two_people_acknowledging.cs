// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.for_AnnouncementAcknowledgements.when_reducing;

/// <summary>
/// Both are among those who acknowledged it.
/// </summary>
public class two_people_acknowledging : a_read_model<AnnouncementAcknowledgements>
{
    AnnouncementAcknowledgements _acknowledgements;

    async Task Because()
    {
        await Happened(new AnnouncementAcknowledged(_announcementId, Viewers.Me), new AnnouncementAcknowledged(_announcementId, Viewers.SomebodyElse));
        _acknowledgements = _scenario.Instance;
    }

    [Fact] void should_have_been_acknowledged_by_both() => _acknowledgements.AcknowledgedBy.Count().ShouldEqual(2);
}
#endif
