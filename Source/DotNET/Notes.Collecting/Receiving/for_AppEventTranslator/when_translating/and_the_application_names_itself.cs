// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;
using Cratis.Showrunner.Notes.Contracts;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

public class and_the_application_names_itself : an_app_event_translator
{
    NamedTag[] _tags;

    async Task Because()
    {
        var context = InboxContext() with { NamedTags = [new(new TagName(AudienceTags.From), "ledger-app")] };
        await _translator.Translate(Given(Tenant.Value), context, Application);
        await ReadRecorded();
        _tags = [.. _recorded.Single().Context.NamedTags];
    }

    [Fact] void should_keep_who_sent_the_note_on_the_private_fact() => _tags.ShouldContain(new NamedTag(new TagName(AudienceTags.From), "ledger-app"));
}
#endif
