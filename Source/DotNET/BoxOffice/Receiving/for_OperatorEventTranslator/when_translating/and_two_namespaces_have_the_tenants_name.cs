// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.when_translating;

public class and_two_namespaces_have_the_tenants_name : an_operator_event_translator
{
    protected override IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant, Tenant];

    async Task Because()
    {
        await _translator.Translate(new TenantReinstated(Tenants.Tenant), OrganizationContext(), Operator);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.ShouldBeEmpty();
    [Fact] void should_log_that_the_tenant_can_not_be_told() => _logger.Messages.ShouldContain(_ => _.Level == LogLevel.Warning && _.Name == nameof(OperatorEventTranslatorLogging.AmbiguousTenant));
}
#endif
