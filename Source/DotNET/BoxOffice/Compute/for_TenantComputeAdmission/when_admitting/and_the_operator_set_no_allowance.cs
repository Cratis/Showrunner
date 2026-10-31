// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_operator_set_no_allowance : a_tenant_admitting_compute
{
    AdmissionDecision _decision;

    async Task Because()
    {
        _entitlements.AllowanceFor(Tokens).Returns(MeterAllowance.None(Tokens));
        _decision = await _admission.Admit(Tokens);
    }

    [Fact] void should_allow_nothing() => _decision.Outcome.ShouldEqual(AdmissionOutcome.AllowanceExhausted);
}
#endif
