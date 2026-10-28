// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

/// <summary>
/// A tenant is its namespace's exact name: a name that differs only in case is another tenant's.
/// </summary>
public class and_the_tenant_differs_only_in_case : a_desk_event_translator
{
    async Task Because()
    {
        await Reported();
        await _translator.Translate(new IssueTriaged("FJORD-LOGISTICS"), InboxContext(), Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_new() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_not_open_the_tenant_namespace() => _client.DidNotReceive().GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant));
    [Fact] void should_not_open_the_namespace() => _client.DidNotReceive().GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == "FJORD-LOGISTICS"));
    [Fact] void should_log_that_it_ignored_it() => _logger.Messages.ShouldContain(_ => _.Level == LogLevel.Warning && _.Name == nameof(DeskEventTranslatorLogging.UnknownTenant));
}
#endif
