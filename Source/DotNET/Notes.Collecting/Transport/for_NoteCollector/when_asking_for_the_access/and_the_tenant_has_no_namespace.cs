// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_access;

/// <summary>
/// The publication's tenant has no namespace in the collecting store: the collector says so, and serves no list.
/// </summary>
public class and_the_tenant_has_no_namespace : a_note_collector
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default];

    CollectOutcome _outcome;
    Contracts.Transport.FeedbackAccessGrants _grants;

    async Task Because() => (_outcome, _grants) = await _collector.Access(PublicationName, _publication, null);

    [Fact] void should_say_the_tenant_is_unknown() => _outcome.ShouldEqual(CollectOutcome.UnknownTenant);
    [Fact] void should_serve_no_list() => _grants.ShouldBeNull();
}
#endif
