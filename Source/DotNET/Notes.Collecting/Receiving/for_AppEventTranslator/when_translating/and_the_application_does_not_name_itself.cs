// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

public class and_the_application_does_not_name_itself : an_app_event_translator
{
    NamedTag[] _tags;

    async Task Because()
    {
        await _translator.Translate(Given(Tenant.Value), InboxContext(), Application);
        await ReadRecorded();
        _tags = [.. _recorded.Single().Context.NamedTags];
    }

    [Fact] void should_not_tag_the_private_fact() => _tags.ShouldBeEmpty();
}
#endif
