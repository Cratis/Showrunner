// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_ending;

public class work_whose_stop_is_rejected : a_tenant_whose_appends_are_answered
{
    Exception _exception;

    async Task Because() => _exception = await Record.ExceptionAsync(() => AdmissionAnswering(AppendAnswers.Rejected).End(new(AgentRuns, ComputeWorkId.New())));

    [Fact] void should_say_the_stop_was_not_recorded() => Assert.IsType<ComputeFactNotRecorded>(_exception);
}
#endif
