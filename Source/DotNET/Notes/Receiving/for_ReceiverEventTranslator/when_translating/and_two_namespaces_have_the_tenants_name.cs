// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

public class and_two_namespaces_have_the_tenants_name : a_receiver_event_translator
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant, Tenant];

    async Task Because()
    {
        await Given();
        await _translator.Translate(new NoteAcknowledged(Tenant.Value), InboxContext(), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count(_ => _.Content is ReceiverAcknowledgedNote).ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(ReceiverEventTranslatorLogging.AmbiguousTenant));
}
#endif
