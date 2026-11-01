// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_building_the_contract_for_a_published_performance : Specification
{
    IReadOnlyDictionary<string, string> _variables;

    void Because() => _variables = AuthProxyContract.EnvironmentVariables(PerformanceSamples.PublishedPerformance("crm"));

    [Fact] void should_forward_the_backend_to_the_stage_port() => _variables["Cratis__AuthProxy__Services__application__Backend__BaseUrl"].ShouldEqual("http://crm-app:9090/");
    [Fact] void should_forward_the_frontend_to_the_stage_port() => _variables["Cratis__AuthProxy__Services__application__Frontend__BaseUrl"].ShouldEqual("http://crm-app:9090/");
}
#endif
