// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_allowance_reached_fact_conflicts_with_an_earlier_one : a_tenant_whose_appends_are_answered
{
    AdmissionDecision _decision;

    async Task Because() => _decision = await AdmitWithTheAllowanceUsedUp(AdmissionAnswering(AppendAnswers.Conflicting));

    [Fact] void should_still_refuse_because_the_allowance_is_exhausted() => _decision.Outcome.ShouldEqual(AdmissionOutcome.AllowanceExhausted);
}
#endif
