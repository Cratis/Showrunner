// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Receiving;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

public class when_the_receiving_side_answers : a_read_model<MyNote>
{
    MyNote _acknowledged;
    MyNote _addressed;

    async Task Because()
    {
        await Happened(NoteEvents.Given(), new ReceiverAcknowledgedNote());
        _acknowledged = _scenario.Instance!;
        await Happened(new ReceiverAddressedNote("2.0.0"));
        _addressed = _scenario.Instance!;
    }

    [Fact] void should_be_acknowledged_after_the_acknowledgement() => _acknowledged.Status.ShouldEqual(NoteStatus.Acknowledged);
    [Fact] void should_be_addressed_after_the_resolution() => _addressed.Status.ShouldEqual(NoteStatus.Addressed);
    [Fact] void should_name_the_version() => _addressed.StatusDetail.ShouldEqual(new NoteStatusDetail("2.0.0"));
    [Fact] void should_have_its_last_activity_at_the_resolution() => _addressed.LastActivityAt.ShouldEqual(At(2));
}
#endif
