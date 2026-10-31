// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.for_AnnouncementAcknowledgements.when_reducing;

/// <summary>
/// The person is counted once.
/// </summary>
public class the_same_person_acknowledging_twice : a_read_model<AnnouncementAcknowledgements>
{
    AnnouncementAcknowledgements _acknowledgements;

    async Task Because()
    {
        await Happened(new AnnouncementAcknowledged(_announcementId, Viewers.Me), new AnnouncementAcknowledged(_announcementId, Viewers.Me));
        _acknowledgements = _scenario.Instance;
    }

    [Fact] void should_have_been_acknowledged_once() => _acknowledgements.AcknowledgedBy.Count().ShouldEqual(1);
}
#endif
