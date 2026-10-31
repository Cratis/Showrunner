// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.when_translating;

public class and_the_same_decision_arrives_twice : an_operator_event_translator
{
    async Task Because()
    {
        // The forwarder delivered the decision into the inbox twice: two inbox events, the same decision.
        await _translator.Translate(new ModuleRevoked(Tenants.Tenant, "analytics"), OrganizationContext(7), Operator);
        await _translator.Translate(new ModuleRevoked(Tenants.Tenant, "analytics"), OrganizationContext(8), Operator);
        await ReadRecorded();
    }

    [Fact] void should_record_it_once() => _recorded.Count(_ => _.Content is OperatorRevokedModule).ShouldEqual(1);
    [Fact] void should_record_the_organization_once() => _recorded.Count(_ => _.Content is TenantOrganizationIdentified).ShouldEqual(1);
}
#endif
