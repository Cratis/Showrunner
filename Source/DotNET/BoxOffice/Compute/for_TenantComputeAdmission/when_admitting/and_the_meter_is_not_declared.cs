// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.when_admitting;

public class and_the_meter_is_not_declared : a_tenant_admitting_compute
{
    Exception _exception;

    async Task Because() => _exception = await Catch.Exception(() => _admission.Admit("storage"));

    [Fact] void should_fail_loudly() => _exception.ShouldBeOfExactType<MeterNotDeclared>();
}
#endif
