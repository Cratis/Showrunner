// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_ResponseApplier.when_applying;

public class and_an_addressed_answer_names_no_version : a_response_applier
{
    IReadOnlyList<object> _recorded;

    async Task Because()
    {
        await Given();
        _collector.Answers.Add(Answer(1, NoteResponseKind.Addressed));
        await _applier.Apply();
        _recorded = await Recorded();
    }

    [Fact] void should_record_nothing_beyond_the_note() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_log_that_it_is_incomplete() => _logger.Messages.Single().Name.ShouldEqual("UnusableResponse");
}
#endif
