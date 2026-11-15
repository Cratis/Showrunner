// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Collecting.Access.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.when_translating;

/// <summary>
/// Two namespaces have the tenant's name, so the tenant can not be told: nothing is recorded and the translator warns.
/// </summary>
public class and_two_namespaces_have_the_tenants_name : an_access_event_translator
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant, Tenant];

    async Task Because()
    {
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(AccessEventTranslatorLogging.AmbiguousTenant));
}
#endif
