// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

/// <summary>
/// The namespaces can not be told apart, so the event is not recorded in either.
/// </summary>
public class and_two_namespaces_have_the_tenants_name : a_desk_event_translator
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant, Tenant];

    async Task Because()
    {
        await Reported();
        await _translator.Translate(new IssueTriaged(Tenant.Value), InboxContext(), Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_new() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_not_open_either_namespace() => _client.DidNotReceive().GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant));
    [Fact] void should_log_that_it_ignored_it() => _logger.Messages.ShouldContain(_ => _.Level == LogLevel.Warning && _.Name == nameof(DeskEventTranslatorLogging.AmbiguousTenant));
}
#endif
