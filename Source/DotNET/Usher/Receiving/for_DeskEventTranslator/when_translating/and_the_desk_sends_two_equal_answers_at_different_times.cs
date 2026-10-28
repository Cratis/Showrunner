// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Execution;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

public class and_the_desk_sends_two_equal_answers_at_different_times : a_desk_event_translator
{
    async Task Because()
    {
        await Reported();
        await _translator.Translate(new ReplySent(Tenant.Value, "We are on it.", IssueEvents.Support), InboxContext(3), Desk);
        _correlationId = CorrelationId.New();
        await _translator.Translate(new ReplySent(Tenant.Value, "We are on it.", IssueEvents.Support), InboxContext(4) with { Occurred = Occurred.AddHours(1) }, Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_both() => _recorded.Count(_ => _.Content is SupportReplied).ShouldEqual(2);
}
#endif
