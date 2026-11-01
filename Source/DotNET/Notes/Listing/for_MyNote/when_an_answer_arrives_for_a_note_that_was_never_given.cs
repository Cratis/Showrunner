// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Receiving;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

public class when_an_answer_arrives_for_a_note_that_was_never_given : a_read_model<MyNote>
{
    async Task Because() => await Happened(new ReceiverAcknowledgedNote());

    [Fact] void should_not_create_a_note() => _scenario.Instance.ShouldBeNull();
}
#endif
