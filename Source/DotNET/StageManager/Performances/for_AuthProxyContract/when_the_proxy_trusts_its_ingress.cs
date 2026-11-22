// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_the_proxy_trusts_its_ingress : Specification
{
    IReadOnlyDictionary<string, string> _variables;
    IReadOnlyDictionary<string, string> _untrusting;

    void Because()
    {
        var performance = PerformanceSamples.Performance(onboarding: false);
        performance.Spec.AuthProxy.TrustedProxies = ["10.0.0.0/8", "192.168.1.5"];
        _variables = AuthProxyContract.EnvironmentVariables(performance);
        _untrusting = AuthProxyContract.EnvironmentVariables(PerformanceSamples.Performance(onboarding: false));
    }

    [Fact] void should_trust_the_first_peer() => _variables["Cratis__AuthProxy__Ingress__TrustedProxies__0"].ShouldEqual("10.0.0.0/8");
    [Fact] void should_trust_the_second_peer() => _variables["Cratis__AuthProxy__Ingress__TrustedProxies__1"].ShouldEqual("192.168.1.5");
    [Fact] void should_trust_none_by_default() => _untrusting.Keys.Any(key => key.Contains("TrustedProxies", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
