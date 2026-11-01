// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_ResponseApplier.when_applying;

public class and_the_answer_arrives_twice : a_response_applier
{
    IReadOnlyList<object> _recorded;

    async Task Because()
    {
        await Given();
        _collector.Answers.Add(Answer(1, NoteResponseKind.Acknowledged));
        await _applier.Apply();

        // The same answer under another instance of the application, which has not applied anything.
        await Create().Apply();
        _recorded = await Recorded();
    }

    [Fact] void should_record_it_once() => _recorded.OfType<ReceiverAcknowledgedNote>().Count().ShouldEqual(1);
}
#endif
