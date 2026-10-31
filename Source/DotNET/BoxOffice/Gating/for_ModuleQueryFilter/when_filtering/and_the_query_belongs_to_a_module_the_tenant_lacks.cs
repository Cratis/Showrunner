// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleQueryFilter.given;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleQueryFilter.when_filtering;

public class and_the_query_belongs_to_a_module_the_tenant_lacks : a_module_query_filter
{
    void Establish() => _gate.RefusedModuleOf("Acme.Analytics.Reports.AllReports").Returns(new ModuleId("analytics"));

    async Task Because() => await Filter("Acme.Analytics.Reports.AllReports");

    [Fact] void should_refuse_the_query() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
