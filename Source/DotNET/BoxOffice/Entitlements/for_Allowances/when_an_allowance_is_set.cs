// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_Allowances;

public class when_an_allowance_is_set : a_tenant_read_model<Allowances>
{
    Allowances _allowances;

    async Task Because()
    {
        await Arrived(
            (Tenants.At(20), new OperatorSetAllowance("compute", 600, AllowancePeriod.Month, OverageBehavior.Park)),
            (Tenants.At(10), new OperatorSetAllowance("compute", 100, AllowancePeriod.Day, OverageBehavior.Block)));
        _allowances = _scenario.Instance!;
    }

    [Fact] void should_keep_the_newest_decision() => _allowances.For("compute").Quantity.ShouldEqual(new MeterQuantity(600));
    [Fact] void should_keep_its_period() => _allowances.For("compute").Period.ShouldEqual(AllowancePeriod.Month);
    [Fact] void should_keep_its_overage_behavior() => _allowances.For("compute").Overage.ShouldEqual(OverageBehavior.Park);
    [Fact] void should_allow_nothing_of_a_meter_without_an_allowance() => _allowances.For("storage").Quantity.ShouldEqual(MeterQuantity.NotSet);
}
#endif
