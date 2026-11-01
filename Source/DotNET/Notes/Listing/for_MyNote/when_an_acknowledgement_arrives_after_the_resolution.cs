// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Receiving;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

/// <summary>
/// Events about a note can arrive in any order; an acknowledgement never takes back a resolution.
/// </summary>
public class when_an_acknowledgement_arrives_after_the_resolution : a_read_model<MyNote>
{
    MyNote _note;

    async Task Because()
    {
        await Happened(NoteEvents.Given(), new ReceiverAddressedNote("2.0.0"), new ReceiverAcknowledgedNote());
        _note = _scenario.Instance!;
    }

    [Fact] void should_stay_addressed() => _note.Status.ShouldEqual(NoteStatus.Addressed);
}
#endif
