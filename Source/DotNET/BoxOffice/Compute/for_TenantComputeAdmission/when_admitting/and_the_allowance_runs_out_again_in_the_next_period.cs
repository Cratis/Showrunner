// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_allowance_runs_out_again_in_the_next_period : a_tenant_admitting_compute
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        Allow(Tokens, 1000);
        Measured(Tenants.Today, Tokens, 1000);
        Measured(Tenants.Day(1), Tokens, 1000);
        await _admission.Admit(Tokens);
        _clock.MoveTo(Tenants.Day(1));
        await _admission.Admit(Tokens);
        _facts = await TenantFacts();
    }

    [Fact] void should_record_it_once_per_period() => _facts.Count.ShouldEqual(2);
}
#endif
