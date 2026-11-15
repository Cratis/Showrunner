// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Access.given;
using Cratis.Showrunner.Notes.Access.Receiving.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.Receiving.for_AccessEventTranslator.when_translating;

/// <summary>
/// The desk names no user: there is nobody to grant, so nothing is recorded and the translator warns.
/// </summary>
public class and_the_event_names_no_user : an_access_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new FeedbackAccessGranted(AccessEvents.Tenant, new FeedbackGiverIdentity("github", string.Empty), AccessEvents.Grantor), InboxContext(), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(AccessEventTranslatorLogging.NoUser));
}
#endif
