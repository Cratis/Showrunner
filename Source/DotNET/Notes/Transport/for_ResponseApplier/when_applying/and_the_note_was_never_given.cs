// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_ResponseApplier.when_applying;

public class and_the_note_was_never_given : a_response_applier
{
    IReadOnlyList<object> _recorded;

    async Task Because()
    {
        _collector.Answers.Add(Answer(1, NoteResponseKind.Acknowledged));
        await _applier.Apply();
        _recorded = await Recorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
    [Fact] void should_log_that_it_ignored_it() => _logger.Messages.Single().Level.ShouldEqual(Microsoft.Extensions.Logging.LogLevel.Warning);
    [Fact] void should_not_stop_at_it() => _applier.Cursor.ShouldEqual(1ul);
}
#endif
