// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_access;

/// <summary>
/// Every tenant has its own namespace and a publication reads only its tenant's: a user the desk granted in another tenant is
/// not granted in this one, and the other tenant's decisions neither fill this list nor move its version.
/// </summary>
public class and_another_tenant_has_decisions : a_note_collector
{
    static readonly EventStoreNamespaceName Other = "another-tenant";

    EventStoreForTesting _otherStore;
    FeedbackAccessGrants _ours;
    FeedbackAccessGrants _theirs;

    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant, Other];

    void Establish()
    {
        _otherStore = new();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Is<EventStoreNamespaceName?>(_ => _ == Other)).Returns(_otherStore);
    }

    void Destroy() => _otherStore.Dispose();

    async Task Because()
    {
        await DeskDecisions.Grant(_tenantStore.EventLog, DeskDecisions.User("ours"), 0);
        await DeskDecisions.Grant(_otherStore.EventLog, DeskDecisions.User("theirs"), 1);
        await DeskDecisions.Grant(_otherStore.EventLog, DeskDecisions.User("also-theirs"), 2);
        (_, _ours) = await _collector.Access(PublicationName, _publication, null);
        (_, _theirs) = await _collector.Access("pub-other", new NotesPublication(Other.Value, "Ledger"), null);
    }

    [Fact] void should_serve_only_the_tenants_own_users() => _ours.Givers.ShouldContainOnly(Contracts.Access.FeedbackGiverId.For(DeskDecisions.User("ours")).Value);
    [Fact] void should_not_let_the_other_tenant_move_the_version() => _ours.Version.ShouldEqual(1ul);
    [Fact] void should_serve_the_other_tenant_its_own_users() => _theirs.Givers.Count.ShouldEqual(2);
}
#endif
