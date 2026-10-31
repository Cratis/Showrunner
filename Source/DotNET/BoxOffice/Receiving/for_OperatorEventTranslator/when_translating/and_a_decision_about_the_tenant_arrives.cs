// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.when_translating;

public class and_a_decision_about_the_tenant_arrives : an_operator_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new TrialOpened(Tenants.Tenant, new(Tenants.Day(14)), 3), OrganizationContext(), Operator);
        await ReadRecorded();
    }

    [Fact] void should_record_the_decision_as_a_private_fact() => _recorded.Select(_ => _.Content).OfType<OperatorOpenedTrial>().Single().Seats.ShouldEqual(new SeatCount(3));
    [Fact] void should_record_it_when_the_operator_decided_it() => _recorded.Single(_ => _.Content is OperatorOpenedTrial).Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_record_which_organization_the_tenant_is() => _recorded.Select(_ => _.Content).OfType<TenantOrganizationIdentified>().Single().Organization.ShouldEqual(Tenants.Organization);
    [Fact] void should_record_it_in_the_tenants_stream() => _recorded.ShouldContain(_ => _.Context.EventSourceId == TenantStream.Id && _.Content is OperatorOpenedTrial);
}
#endif
