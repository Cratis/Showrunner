// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Contracts.Tenants;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorDecisions;

public class when_subscribing : Specification
{
    IEnumerable<Type> _all;

    void Because() => _all = OperatorDecisions.All;

    [Fact] void should_hear_the_operator_register_a_tenant() => _all.ShouldContain(typeof(TenantRegistered));
    [Fact] void should_hear_the_operator_close_a_tenant() => _all.ShouldContain(typeof(TenantClosing));
    [Fact] void should_not_record_the_registration_as_a_decision() => OperatorDecisions.ForTenant(new TenantRegistered(Cratis.Showrunner.BoxOffice.given.Tenants.Tenant, "Fjord", Cratis.Showrunner.BoxOffice.given.Tenants.Ada, "Maintainer")).ShouldBeNull();
}
#endif
