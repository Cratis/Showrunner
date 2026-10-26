// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

public class and_the_same_desk_event_arrives_twice : a_desk_event_translator
{
    async Task Because()
    {
        await Reported();
        var question = QuestionId.New();

        // The forwarder delivered the desk event into the inbox twice: two inbox events, the same desk event.
        await _translator.Translate(new QuestionAsked(Tenant.Value, question, "Which browser?", IssueEvents.Support), InboxContext(7), Desk);
        await _translator.Translate(new QuestionAsked(Tenant.Value, question, "Which browser?", IssueEvents.Support), InboxContext(8), Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_it_once() => _recorded.Count(_ => _.Content is SupportAskedQuestion).ShouldEqual(1);
}
#endif
