// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Collecting.Access.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.when_translating;

/// <summary>
/// What arrives is not about access: nothing is recorded and the translator warns.
/// </summary>
public class and_the_event_is_not_one_from_a_desk : an_access_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new NoteAcknowledged(AccessEvents.Tenant.Value), InboxContext(), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(AccessEventTranslatorLogging.NotAnAccessEvent));
}
#endif
