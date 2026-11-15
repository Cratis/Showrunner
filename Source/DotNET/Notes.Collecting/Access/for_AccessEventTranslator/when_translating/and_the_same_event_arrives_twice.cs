// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Collecting.Access.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.when_translating;

/// <summary>
/// The forwarder can put the same event into the inbox twice; the decision is recorded once.
/// </summary>
public class and_the_same_event_arrives_twice : an_access_event_translator
{
    async Task Because()
    {
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(7), Desk);
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(8), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_it_once() => _recorded.Count(_ => _.Content is FeedbackAccessGrantReceived).ShouldEqual(1);
    [Fact] void should_say_it_was_already_recorded() => _logger.Messages.ShouldContain(_ => _.Name == nameof(AccessEventTranslatorLogging.AlreadyRecorded));
}
#endif
