// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

/// <summary>
/// The tenant a payload names is never trusted: a note for another tenant, even one that exists, is rejected and logged
/// without what it says.
/// </summary>
public class and_the_note_names_another_tenant : a_note_collector
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant, "another-tenant"];

    CollectOutcome _outcome;

    async Task Because() =>
        _outcome = await _collector.Collect(
            PublicationName,
            _publication,
            string.Empty,
            [Relayed(Given("another-tenant"), 4)]);

    [Fact] void should_reject_it_as_a_mismatch() => _outcome.ShouldEqual(CollectOutcome.TenantMismatch);
    [Fact] async Task should_record_nothing_in_the_publications_tenant() => (await RecordedAnywhere()).Count.ShouldEqual(0);
    [Fact] void should_log_it_as_a_warning() => _logger.Messages.Single().Level.ShouldEqual(LogLevel.Warning);
    [Fact] void should_not_log_what_the_note_says() => _logger.Messages.Any(_ => _.Text.Contains("invoice", StringComparison.OrdinalIgnoreCase)).ShouldBeFalse();
    [Fact] void should_not_log_the_giver() => _logger.Messages.Any(_ => _.Text.Contains("Ada", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
