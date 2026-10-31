// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleQueryFilter.given;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleQueryFilter.when_filtering;

public class and_the_tenant_has_the_module : a_module_query_filter
{
    void Establish() => _gate.RefusedModuleOf(Arg.Any<string?>()).Returns((ModuleId?)null);

    async Task Because() => await Filter("Acme.Analytics.Reports.AllReports");

    [Fact] void should_let_the_query_through() => _result.IsAuthorized.ShouldBeTrue();
}
#endif
