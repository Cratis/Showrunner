// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_position;

public class and_the_tenant_has_no_namespace : a_note_collector
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default];

    CollectOutcome _outcome;

    async Task Because() => _outcome = (await _collector.Position(PublicationName, _publication, string.Empty)).Outcome;

    [Fact] void should_say_so() => _outcome.ShouldEqual(CollectOutcome.UnknownTenant);
}
#endif
