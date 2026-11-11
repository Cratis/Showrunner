// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_allowance_reached_fact_is_rejected : a_tenant_whose_appends_are_answered
{
    Exception _exception;

    async Task Because() => _exception = await Record.ExceptionAsync(() => AdmitWithTheAllowanceUsedUp(AdmissionAnswering(AppendAnswers.Rejected)));

    [Fact] void should_say_the_fact_was_not_recorded_rather_than_decide() => Assert.IsType<ComputeFactNotRecorded>(_exception);
}
#endif
